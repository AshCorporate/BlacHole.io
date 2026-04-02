using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.Buffs;
using BlacHole.Data;

namespace BlacHole.Gameplay.Progression
{
    // ── EventBus payload ────────────────────────────────────────────────────
    public readonly struct ProgressionChangedEvent
    {
        public readonly PlayerEntity Player;
        public ProgressionChangedEvent(PlayerEntity p) { Player = p; }
    }

    /// <summary>
    /// Manages mass, XP, score, and radius recalculation for a single player.
    /// </summary>
    public class ProgressionService
    {
        private readonly PlayerEntity  _entity;
        private readonly BuffSystem    _buffs;
        private readonly PlayerConfig  _cfg;
        private          EventBus      _eventBus;

        public ProgressionService(PlayerEntity entity, BuffSystem buffs, PlayerConfig cfg)
        {
            _entity = entity;
            _buffs  = buffs;
            _cfg    = cfg;
        }

        public void SetEventBus(EventBus bus) => _eventBus = bus;

        public void AddMass(float amount)
        {
            _entity.Mass += amount;
            RecalculateRadius();
            _eventBus?.Fire(new ProgressionChangedEvent(_entity));
        }

        public void AddXP(float amount)
        {
            _entity.XP += amount;
            CheckLevelUp();
            _eventBus?.Fire(new ProgressionChangedEvent(_entity));
        }

        public void AddScore(float amount)
        {
            _entity.Score += amount;
            _eventBus?.Fire(new ProgressionChangedEvent(_entity));
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void RecalculateRadius()
        {
            float baseRadius = _cfg != null ? _cfg.BaseRadius      : 0.5f;
            float k          = _cfg != null ? _cfg.RadiusScaleK    : 0.3f;
            _entity.Radius   = baseRadius + k * Mathf.Sqrt(Mathf.Max(0f, _entity.Mass));
        }

        private void CheckLevelUp()
        {
            // Placeholder: level threshold = 100 * level
            float threshold = 100f * _entity.Level;
            while (_entity.XP >= threshold)
            {
                _entity.XP   -= threshold;
                _entity.Level++;
                threshold = 100f * _entity.Level;
            }
        }
    }
}
