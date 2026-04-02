using System;
using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Data;
using BlacHole.Gameplay.Objects;
using BlacHole.Gameplay.Spawning;

namespace BlacHole.Gameplay.MapGeneration
{
    /// <summary>
    /// Seeded deterministic map generator.
    /// Spawns ~650 objects across 5 size categories with no overlap.
    /// Uses SpatialHash occupancy map for O(1) overlap validation — no Physics.OverlapCircle loops.
    /// Must complete in <1 frame.
    /// </summary>
    public class MapGenerator : MonoBehaviour, IMapGenerator
    {
        [Header("Config")]
        [SerializeField] private MapGenerationConfig config;

        [Header("Object Spawner")]
        [SerializeField] private ObjectSpawner spawner;

        private System.Random _rng;
        private readonly List<GameObject> _spawnedObjects = new List<GameObject>();
        private readonly SpatialHash<float> _occupancy = new SpatialHash<float>(cellSize: 2f);

        private int _currentSeed;
        private int _currentWidth;
        private int _currentHeight;

        // ── IMapGenerator ────────────────────────────────────────────────────

        public IReadOnlyList<GameObject> GetSpawnedObjects() => _spawnedObjects;

        public Bounds GetMapBounds()
        {
            int w = config != null ? config.MapWidth  : 100;
            int h = config != null ? config.MapHeight : 100;
            return new Bounds(Vector3.zero, new Vector3(w, h, 1f));
        }

        public void GenerateMap(int seed, int width, int height)
        {
            _currentSeed   = seed;
            _currentWidth  = width;
            _currentHeight = height;
            _rng           = new System.Random(seed);

            ClearMap();
            SpawnObjects(width, height);
        }

        public void ClearMap()
        {
            foreach (var go in _spawnedObjects)
                if (go != null) Destroy(go);

            _spawnedObjects.Clear();
            _occupancy.Clear();
        }

        public void Regenerate()
        {
            bool useRandom = config != null && config.UseRandomSeed;
            int  seed      = useRandom ? Environment.TickCount : (config != null ? config.DefaultSeed : 12345);
            int  w         = config != null ? config.MapWidth  : 100;
            int  h         = config != null ? config.MapHeight : 100;
            GenerateMap(seed, w, h);
        }

        // ── MonoBehaviour ────────────────────────────────────────────────────

        private void Start()
        {
            Regenerate();
        }

        // ── Spawning ─────────────────────────────────────────────────────────

        private void SpawnObjects(int w, int h)
        {
            float hw = w * 0.5f;
            float hh = h * 0.5f;

            // Definitions: (categoryName, target count, half-size, near-centre preference)
            var categories = new (string name, int count, float radius, bool preferCentre)[]
            {
                ("Mega",   50,  4.0f, true),
                ("Large",  50,  2.0f, true),
                ("Medium", 100, 1.0f, false),
                ("Small",  150, 0.4f, false),
                ("Micro",  300, 0.2f, false),
            };

            // Use config categories if available
            if (config != null && config.CategoriesConfig != null && config.CategoriesConfig.Length > 0)
                SpawnFromConfig(w, h, hw, hh);
            else
                SpawnDefaults(categories, w, h, hw, hh);
        }

        private void SpawnDefaults(
            (string name, int count, float radius, bool preferCentre)[] categories,
            int w, int h, float hw, float hh)
        {
            foreach (var cat in categories)
            {
                int spawned   = 0;
                int attempts  = cat.count * 20;
                float spacing = cat.radius * 2f + 0.1f;

                for (int attempt = 0; attempt < attempts && spawned < cat.count; attempt++)
                {
                    Vector2 pos = cat.preferCentre
                        ? RandomInZone(hw * 0.7f, hh * 0.7f)
                        : RandomInZone(hw, hh);

                    if (!IsOverlapping(pos, spacing))
                    {
                        PlaceObject(pos, cat.name, cat.radius);
                        spawned++;
                    }
                }

                if (spawned < cat.count)
                    Debug.LogWarning($"[MapGenerator] Could only place {spawned}/{cat.count} {cat.name} objects.");
            }
        }

        private void SpawnFromConfig(int w, int h, float hw, float hh)
        {
            foreach (var catCfg in config.CategoriesConfig)
            {
                if (catCfg == null) continue;

                int    count   = catCfg.TargetCount;
                float  spacing = catCfg.MinSpacing;
                bool   centre  = catCfg.SpawnZonePreference == SpawnZonePreference.Center;

                int spawned  = 0;
                int attempts = count * 20;

                for (int attempt = 0; attempt < attempts && spawned < count; attempt++)
                {
                    Vector2 pos = centre
                        ? RandomInZone(hw * 0.7f, hh * 0.7f)
                        : RandomInZone(hw, hh);

                    if (!IsOverlapping(pos, spacing))
                    {
                        PlaceObjectFromConfig(pos, catCfg);
                        spawned++;
                    }
                }

                if (spawned < count)
                    Debug.LogWarning($"[MapGenerator] Could only place {spawned}/{count} {catCfg.CategoryName} objects.");
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Vector2 RandomInZone(float hw, float hh)
            => new Vector2(
                (float)(_rng.NextDouble() * 2.0 - 1.0) * hw,
                (float)(_rng.NextDouble() * 2.0 - 1.0) * hh);

        private bool IsOverlapping(Vector2 pos, float spacing)
        {
            return _occupancy.HasAny(pos, spacing);
        }

        private void PlaceObject(Vector2 pos, string categoryName, float radius)
        {
            GameObject go;
            if (spawner != null)
                go = spawner.SpawnByCategory(categoryName, pos);
            else
            {
                go = CreateDefaultObject(pos, categoryName, radius);
            }

            if (go == null) return;

            _occupancy.Insert(pos, radius);
            _spawnedObjects.Add(go);
        }

        private void PlaceObjectFromConfig(Vector2 pos, SpawnCategoryConfig catCfg)
        {
            float radius = (catCfg.MinSpacing + catCfg.MaxSpacing) * 0.25f;
            PlaceObject(pos, catCfg.CategoryName, radius);
        }

        private GameObject CreateDefaultObject(Vector2 pos, string categoryName, float radius)
        {
            var go = new GameObject($"Obj_{categoryName}");
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            var ao = go.AddComponent<AbsorbableObject>();
            ao.Category             = categoryName;
            ao.Weight               = radius * 10f;
            ao.XPReward             = radius * 2f;
            ao.RequiredConsumeRadius = radius * 0.8f;

            var view = go.AddComponent<ObjectView>();
            view.Initialise(categoryName, radius);

            return go;
        }
    }
}
