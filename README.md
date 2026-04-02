# BlacHole.io

> **Sci-fi arcade territory game** — combine Paper.io territory capture with black-hole absorption mechanics in a cyberpunk top-down world.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [How to Open in Unity](#how-to-open-in-unity)
4. [Scene Load Order](#scene-load-order)
5. [Scene Setup Instructions](#scene-setup-instructions)
6. [Where to Change Balance](#where-to-change-balance)
7. [How to Change / Set the Seed](#how-to-change--set-the-seed)
8. [How to Add New Object Categories](#how-to-add-new-object-categories)
9. [How to Extend AI Behaviours](#how-to-extend-ai-behaviours)
10. [Performance Notes](#performance-notes)
11. [Multiplayer Readiness Notes](#multiplayer-readiness-notes)

---

## Project Overview

**BlacHole.io** is a single-player-vs-bots arcade game with two intertwined mechanics:

| Mechanic | Description |
|---|---|
| **Territory capture** (Paper.io) | Leave your safe zone, draw a tail across the map, return home to claim the enclosed area. |
| **Black-hole absorption** | Your player exerts gravity on nearby objects smaller than yourself. Pull them in, grow bigger, move faster. |

Visual style: **cyberpunk / sci-fi top-down 2D** — dark navy grid map, neon-coloured objects (green, orange, purple, red, gold), neon-cyan player with pulsing ring.

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  Boot Scene                                                      │
│  └── GameBootstrap (initialises ServiceLocator, SceneLoader)    │
└────────────────────┬────────────────────────────────────────────┘
                     │ loads
┌────────────────────▼────────────────────────────────────────────┐
│  MainMenu Scene                                                  │
│  └── MainMenuController (Play/Settings/Quit, seed, bots)        │
└────────────────────┬────────────────────────────────────────────┘
                     │ loads
┌────────────────────▼────────────────────────────────────────────┐
│  Game Scene                                                      │
│                                                                  │
│  GameSessionManager ──► EventBus ──► HUD / DeathScreen          │
│       │                                                          │
│       ├── MapGenerator (SpatialHash + System.Random seed)        │
│       │       └── ObjectSpawner → AbsorbableObject[]            │
│       │                                                          │
│       ├── TerritoryService (ownerGrid[,])                        │
│       │       └── TerritoryRenderer (tile overlay)              │
│       │                                                          │
│       ├── PlayerController                                       │
│       │       ├── PlayerEntity (pure C# data)                   │
│       │       ├── TopDownMovementMotor                           │
│       │       ├── TailService + TailRenderer                    │
│       │       ├── PlayerStateMachine (Safe/Capturing/Stunned)   │
│       │       ├── ProgressionService (mass→radius, XP, score)   │
│       │       └── BuffSystem (speed modifiers)                  │
│       │                                                          │
│       ├── BotSpawner                                             │
│       │       └── BotController × N                             │
│       │               └── (same subsystems as player)           │
│       │                                                          │
│       ├── GravityService (SpatialHash queries, F=GM/r²)         │
│       ├── AbsorptionService (despawn, award mass/XP/score)      │
│       ├── CollisionResolver (bounce, absorb, slide)             │
│       ├── RespawnService (death countdown, re-place)            │
│       └── DynamicRespawnService (refill micro/small objects)    │
└─────────────────────────────────────────────────────────────────┘

  ServiceLocator: static registry — any system can Get<T>() anywhere.
  EventBus: typed pub/sub — all cross-system communication.
  ObjectPool<T>: generic pool — tail points, VFX, pooled objects.
  SpatialHash<T>: O(1) proximity queries — gravity, spawn, collision.
```

---

## How to Open in Unity

**Minimum Unity version:** `2022.3 LTS` or newer (tested with 2022.3.x).

1. Clone this repository.
2. Open **Unity Hub → Open → Add project from disk** and select the repo root.
3. Unity will import assets. The first import may take a few minutes.
4. Install **TextMeshPro** if prompted (Window → Package Manager → TextMeshPro → Install).
5. Open the **Boot** scene: `Assets/Scenes/Boot.unity`
6. Press **Play** — the boot scene loads MainMenu, then you can start the game.

> **Required packages** (install via Package Manager if missing):
> - `com.unity.textmeshpro` ≥ 3.0
> - `com.unity.inputsystem` *(optional — legacy Input is used by default)*

---

## Scene Load Order

```
Boot  →  MainMenu  →  Game
```

| Scene | Purpose |
|---|---|
| **Boot** | `GameBootstrap` initialises all core services and calls `SceneLoader.LoadScene("MainMenu")` |
| **MainMenu** | Player configures seed, bot count, map size; presses Play |
| **Game** | Full gameplay session |

---

## Scene Setup Instructions

### Boot Scene
1. Create a new empty scene named `Boot`.
2. Add an empty `GameObject` called **Bootstrap**.
3. Add `GameBootstrap.cs` to it.
4. Set *Main Menu Scene Name* = `"MainMenu"`.

### MainMenu Scene
1. Create a new scene named `MainMenu`.
2. Add a **Canvas** (Screen Space — Overlay).
3. Add `MainMenuController.cs` to the Canvas root.
4. Wire up UI elements: Play Button, Settings Button, Quit Button, Seed Input Field, Bot Count Slider, Map Size Dropdown.
5. Add a dark background sprite (colour `#0A0A1A`).

### Game Scene
1. Create a new scene named `Game`.
2. Add the following GameObjects:

| GameObject | Component(s) |
|---|---|
| **GameSession** | `GameSessionManager` |
| **MapGenerator** | `MapGenerator`, `ObjectSpawner` |
| **Camera** | `Camera` — follows player in `LateUpdate` |
| **Boundary** | `MapBoundary`, `LineRenderer` |
| **TerritoryRenderer** | `TerritoryRenderer` |
| **RespawnService** | `RespawnService` |
| **BotSpawner** | `BotSpawner` |
| **CollisionResolver** | `CollisionResolver` |
| **DynamicRespawn** | `DynamicRespawnService` |
| **HUDCanvas** | Canvas (Screen Space), `HUDController` |
| **PopupCanvas** | Canvas (Screen Space), `PauseMenuController`, `DeathScreenController` |

3. Instantiate **PlayerPrefab** at runtime from `GameSessionManager`.
4. Assign all `ScriptableObject` config assets in Inspector fields.

---

## Where to Change Balance

All game values live in **ScriptableObject assets** in `Assets/Data/Configs/`.
Create them via **Assets → Create → BlacHole → Config → …**

| ScriptableObject | Key Fields | Effect |
|---|---|---|
| `MovementConfig` | `BaseSpeed`, `Vmin`, `Vmax`, `AccelerationTime`, `MaxTurnRate`, `TurnPenaltyCoeff` | Player/bot movement feel |
| `PlayerConfig` | `BaseRadius`, `RadiusScaleK`, `StartMass` | How fast the player grows with mass |
| `TailConfig` | `RecordInterval`, `MaxTailDuration`, `StunDuration`, `MaxPoints` | Tail length, timeout, stun |
| `GravityConfig` | `GravityCoeff`, `MaxAttractionSpeed`, `ConsumeRadiusFactor` | Pull strength and range |
| `TerritoryConfig` | `GridCellSize`, `EnemyTerritorySpeedDebuff`, `StartTerritoryRadius` | Grid resolution, penalty |
| `BotConfig` | `BotCount`, `SeekFoodWeight`, `ReturnHomeWeight`, `FleeDangerWeight` | AI aggression/behaviour mix |
| `GameRulesConfig` | `BounceThreshold`, `AbsorptionMassRatio`, `RespawnDelay`, `TailKillsPlayer` | Core game rules |
| `MapGenerationConfig` | `MapWidth`, `MapHeight`, `DefaultSeed`, `UseRandomSeed`, `CategoriesConfig[]` | Map size and object density |
| `SpawnCategoryConfig` × 5 | `TargetCount`, `MinSpacing`, `SpawnZonePreference` | Per-category object counts |
| `UIConfig` | `PanelBackground`, `PrimaryText`, `MaxLeaderboardEntries` | HUD colours and sizes |

---

## How to Change / Set the Seed

**Via UI (MainMenu):**
- Type any integer into the **Seed** input field on the Main Menu.
- Leave it blank to use a random seed on each run.

**Via ScriptableObject:**
1. Open `Assets/Data/Configs/MapGenerationConfig`.
2. Set `UseRandomSeed = false`.
3. Set `DefaultSeed` to any integer.

**Via PlayerPrefs (scripted):**
```csharp
PlayerPrefs.SetString("Seed", "12345");
```
`MapGenerator` reads `PlayerPrefs.GetString("Seed")` on Start if the key exists.

---

## How to Add New Object Categories

1. **Create a new `SpawnCategoryConfig` asset:**
   - Right-click in Project → Create → BlacHole → Config → SpawnCategoryConfig
   - Set `CategoryName`, `TargetCount`, `MinSpacing`, `SpawnZonePreference`, `SpawnType`

2. **Add palette entry in `ObjectView.cs`:**
   ```csharp
   ("MyCategory", new Color(r, g, b), baseSize),
   ```

3. **Reference it in `MapGenerationConfig.CategoriesConfig[]`:**
   - Add the new asset to the array in Inspector.

4. *(Optional)* Wire prefabs in `ObjectSpawner.categories[]` for custom art.

---

## How to Extend AI Behaviours

Bot AI lives in `BotBrain.cs` using a **utility-based** system.

Each behaviour returns a `(name, utility, direction)` tuple. The highest-utility action wins each tick (every 0.25 s).

**To add a new behaviour:**
1. Write a method that returns `(string, float, Vector2)`.
2. Add it to the `candidates` list in `EvaluateBehaviors()`.
3. Expose its weight in `BotConfig` as a new `[SerializeField]` float.
4. Add the field to `BotConfig.cs` ScriptableObject.

Example:
```csharp
// In BotBrain.EvaluateBehaviors()
float snipeWeight = _cfg != null ? _cfg.SnipeWeight : 0.5f;
if (NearbyWeakerEnemy(out Vector2 enemyDir))
    candidates.Add(("SnipeEnemy", snipeWeight, enemyDir));
```

---

## Performance Notes

| System | Strategy |
|---|---|
| **Map generation** | Runs in `<1 frame` using `System.Random` + `SpatialHash` occupancy map (no `Physics.OverlapCircle` loops). |
| **Gravity** | `SpatialHash` proximity query — only objects within `5 × playerRadius` are iterated. |
| **Collision** | `SpatialHash` for player-vs-player; no broad-phase Physics calls. |
| **Territory rendering** | Tile array with `SpriteRenderer.color` updates only; no mesh rebuilding. Can be optimised with a GPU texture if needed. |
| **Tail** | `ObjectPool<Vector2>` for point allocation. Max 500 points per tail (configurable). |
| **Bot AI** | Ticks every 0.25 s via `BotBrain._tickInterval`, not every frame. |
| **Object pooling** | All map objects go through `ObjectPool` / `ObjectSpawner` pool queues. |

**If you hit performance issues:**
- Reduce `TerritoryConfig.GridCellSize` resolution (larger cells = fewer tiles).
- Reduce `MapGenerationConfig.MapWidth/Height`.
- Reduce `TailConfig.MaxPoints`.
- Increase `BotBrain.TickInterval`.

---

## Multiplayer Readiness Notes

The codebase is structured for eventual multiplayer extension:

- **`PlayerEntity`** is a pure C# class with no Unity lifecycle — it can be serialised over a network.
- **`IMovementMotor`** abstracts movement so a `NetworkMovementMotor` can replace `TopDownMovementMotor`.
- **`ITailService` / `ITerritoryService`** interfaces allow server-authoritative implementations.
- **`EventBus`** decouples systems — network events can be injected without changing gameplay code.
- **`ServiceLocator`** allows swapping implementations at startup (e.g., `NetworkTerritoryService`).
- **`System.Random(seed)`** ensures deterministic map generation — all clients can generate identical maps from the same seed without syncing individual object data.

For networked play (e.g., Netcode for GameObjects / Mirror):
1. Replace `TopDownMovementMotor` with a predicted + reconciled motor.
2. Make `TerritoryService` server-authoritative; broadcast `TerritoryChangedEvent` to clients.
3. Replace `GravityService.Tick` with a server tick + client-side interpolation.

---

## Cyberpunk Art Palette Reference

| Element | Colour | Hex |
|---|---|---|
| Map background | Very dark navy | `#0A0A1A` |
| Map grid lines | Dark purple-blue | `#1A1A3E` |
| Boundary frame | Neon cyan | `#00FFFF` |
| Micro objects | Neon green | `#39FF14` |
| Small objects | Neon orange | `#FF6B00` |
| Medium objects | Neon purple | `#B026FF` |
| Large objects | Neon red | `#FF073A` |
| Mega objects | Gold | `#FFD700` |
| Player | Neon cyan | `#00FFFF` |
| UI panels | Dark navy | `#0D0D2B` |

---

*BlacHole.io — AshCorporate 2026*