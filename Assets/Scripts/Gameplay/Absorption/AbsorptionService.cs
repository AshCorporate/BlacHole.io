using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.Gravity;
using BlacHole.Gameplay.Objects;
using BlacHole.Gameplay.Progression;

namespace BlacHole.Gameplay.Absorption
{
    // ── EventBus payloads ───────────────────────────────────────────────────
    public readonly struct PlayerAbsorbedObjectEvent
    {
        public readonly PlayerEntity Player;
        public readonly float        MassGained;
        public readonly float        XPGained;
        public readonly float        ScoreGained;
        public PlayerAbsorbedObjectEvent(PlayerEntity p, float mass, float xp, float score)
        { Player = p; MassGained = mass; XPGained = xp; ScoreGained = score; }
    }

    /// <summary>
    /// Handles an object reaching the player centre:
    ///   • Despawns / returns the object to pool
    ///   • Awards Mass, XP, Score to player
    ///   • Fires PlayerAbsorbedObjectEvent via EventBus
    /// </summary>
    public class AbsorptionService
    {
        private readonly EventBus          _eventBus;
        private readonly ProgressionService _progression;

        public AbsorptionService(EventBus eventBus, ProgressionService progression)
        {
            _eventBus    = eventBus;
            _progression = progression;
        }

        public void Absorb(PlayerEntity player, IGravityAffectable obj)
        {
            if (!obj.IsAvailable) return;

            float mass  = obj.Mass;
            float xp    = (obj as AbsorbableObject)?.XPReward   ?? mass * 0.1f;
            float score = (obj as AbsorbableObject)?.XPReward   ?? mass;

            _progression.AddMass(mass);
            _progression.AddXP(xp);
            _progression.AddScore(score);

            _eventBus?.Fire(new PlayerAbsorbedObjectEvent(player, mass, xp, score));

            // Despawn: notify the MonoBehaviour via a flag
            if (obj is AbsorbableObject ao)
                ao.Despawn();
        }
    }
}
