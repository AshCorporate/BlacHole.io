using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.Territory;
using BlacHole.Data;

namespace BlacHole.Gameplay.Bots
{
    /// <summary>
    /// Utility-based AI brain.
    /// Each behaviour has a weight; the highest-scoring action wins each tick.
    /// </summary>
    public class BotBrain
    {
        private readonly BotEntity         _bot;
        private readonly BotConfig         _cfg;
        private readonly ITerritoryService _territory;
        private readonly SpatialHash<PlayerEntity> _playerHash;

        private const float TickInterval = 0.25f;
        private float _tickTimer;

        // Last computed desired direction
        public Vector2 DesiredInput { get; private set; }

        public BotBrain(BotEntity bot, BotConfig cfg,
                        ITerritoryService territory,
                        SpatialHash<PlayerEntity> playerHash)
        {
            _bot        = bot;
            _cfg        = cfg;
            _territory  = territory;
            _playerHash = playerHash;
        }

        public void Tick(float dt)
        {
            _tickTimer -= dt;
            if (_tickTimer > 0f) return;
            _tickTimer = TickInterval;

            DesiredInput = EvaluateBehaviors();
        }

        // ── Behaviour evaluation ─────────────────────────────────────────────

        private Vector2 EvaluateBehaviors()
        {
            var entity    = _bot.Entity;
            var candidates = new List<(string name, float utility, Vector2 dir)>();

            float seekFoodW    = _cfg != null ? _cfg.SeekFoodWeight         : 1f;
            float returnHomeW  = _cfg != null ? _cfg.ReturnHomeWeight       : 1.2f;

            // ReturnHome — high utility when tail is long or far from base
            Vector2 home = _territory != null
                ? _territory.GridToWorld(entity.Id * 5 % _territory.GridWidth,
                                         entity.Id * 5 % _territory.GridHeight)
                : Vector2.zero;
            float homeDist = Vector2.Distance(entity.Position, home);
            candidates.Add(("ReturnHome", returnHomeW * Mathf.Clamp01(homeDist / 30f), (home - entity.Position).normalized));

            // Wander — baseline
            if (_bot.WanderTarget == Vector2.zero || Vector2.Distance(entity.Position, _bot.WanderTarget) < 2f)
                _bot.WanderTarget = entity.Position + Random.insideUnitCircle.normalized * 15f;
            candidates.Add(("Wander", 0.3f, (_bot.WanderTarget - entity.Position).normalized));

            // SeekFood — move toward nearest neutral territory / absorbable object
            candidates.Add(("SeekFood", seekFoodW * 0.5f, Random.insideUnitCircle.normalized));

            // Pick highest utility
            candidates.Sort((a, b) => b.utility.CompareTo(a.utility));
            _bot.CurrentBehavior = candidates[0].name;
            return candidates[0].dir;
        }
    }
}
