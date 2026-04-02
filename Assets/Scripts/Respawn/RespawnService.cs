using System.Collections;
using UnityEngine;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.States;
using BlacHole.Data;

namespace BlacHole.Respawn
{
    // ── EventBus payloads ───────────────────────────────────────────────────
    public readonly struct PlayerDiedEvent     { public readonly PlayerEntity Player; public PlayerDiedEvent(PlayerEntity p)     { Player = p; } }
    public readonly struct PlayerRespawnedEvent{ public readonly PlayerEntity Player; public PlayerRespawnedEvent(PlayerEntity p) { Player = p; } }

    /// <summary>
    /// Handles the death/respawn cycle:
    ///   • Clears tail
    ///   • Hides player for RespawnDelay seconds
    ///   • Respawns on own remaining territory or map edge
    ///   • Fires events for UI
    /// </summary>
    public class RespawnService : MonoBehaviour, IRespawnService
    {
        [Header("Config")]
        [SerializeField] private GameRulesConfig rulesConfig;

        [Header("Map bounds")]
        [SerializeField] private Vector2 mapMin = new Vector2(-50, -50);
        [SerializeField] private Vector2 mapMax = new Vector2( 50,  50);

        private EventBus _eventBus;

        public bool  IsRespawning     { get; private set; }
        public float RespawnCountdown { get; private set; }

        private void Awake()
        {
            ServiceLocator.TryGet(out _eventBus);
        }

        public void RequestRespawn(PlayerEntity entity)
        {
            if (IsRespawning) return;
            StartCoroutine(RespawnCoroutine(entity));
        }

        private IEnumerator RespawnCoroutine(PlayerEntity entity)
        {
            IsRespawning = true;

            // Notify UI
            _eventBus?.Fire(new PlayerDiedEvent(entity));

            float delay = rulesConfig != null ? rulesConfig.RespawnDelay : 3f;
            RespawnCountdown = delay;

            while (RespawnCountdown > 0f)
            {
                yield return null;
                RespawnCountdown -= Time.unscaledDeltaTime;
            }
            RespawnCountdown = 0f;

            // Respawn
            entity.Position = ChooseRespawnPosition();
            entity.Mass     = 10f;
            entity.Radius   = 0.5f;
            entity.IsAlive  = true;
            entity.State    = PlayerState.Safe;

            _eventBus?.Fire(new PlayerRespawnedEvent(entity));
            IsRespawning = false;
        }

        private Vector2 ChooseRespawnPosition()
        {
            // Spawn near map edge (random edge)
            float margin = 5f;
            int edge = Random.Range(0, 4);
            switch (edge)
            {
                case 0: return new Vector2(Random.Range(mapMin.x + margin, mapMax.x - margin), mapMin.y + margin);
                case 1: return new Vector2(Random.Range(mapMin.x + margin, mapMax.x - margin), mapMax.y - margin);
                case 2: return new Vector2(mapMin.x + margin, Random.Range(mapMin.y + margin, mapMax.y - margin));
                default: return new Vector2(mapMax.x - margin, Random.Range(mapMin.y + margin, mapMax.y - margin));
            }
        }
    }
}
