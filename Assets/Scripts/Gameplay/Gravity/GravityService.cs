using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Data;

namespace BlacHole.Gameplay.Gravity
{
    /// <summary>
    /// Manages gravitational attraction of absorb-eligible objects toward players.
    /// Uses SpatialHash for proximity queries to avoid O(N*M) loops.
    /// F = G * Mass / distance^2, clamped to MaxAttractionSpeed.
    /// </summary>
    public class GravityService
    {
        private readonly GravityConfig  _cfg;
        private readonly SpatialHash<IGravityAffectable> _spatialHash;
        private readonly List<IGravityAffectable>        _queryBuffer = new List<IGravityAffectable>();

        public GravityService(GravityConfig cfg)
        {
            _cfg         = cfg;
            _spatialHash = new SpatialHash<IGravityAffectable>(cellSize: 10f);
        }

        // ── Object registration ──────────────────────────────────────────────

        public void Register(IGravityAffectable obj)
            => _spatialHash.Insert(obj.Position, obj);

        public void Unregister(IGravityAffectable obj)
            => _spatialHash.Remove(obj.Position, obj);

        public void Clear() => _spatialHash.Clear();

        // ── Per-tick update ──────────────────────────────────────────────────

        /// <summary>
        /// Pull all objects within pull radius toward the player if player is large enough to absorb them.
        /// </summary>
        public void Tick(PlayerEntity player, float dt)
        {
            if (!player.IsAlive) return;

            float G           = _cfg != null ? _cfg.GravityCoeff         : 9.8f;
            float maxSpeed    = _cfg != null ? _cfg.MaxAttractionSpeed    : 8f;
            float consumeFactor = _cfg != null ? _cfg.ConsumeRadiusFactor : 1.0f;
            float pullRadius  = player.Radius * 5f;  // pull range = 5× player radius

            _spatialHash.Query(player.Position, pullRadius, _queryBuffer);

            foreach (var obj in _queryBuffer)
            {
                if (!obj.IsAvailable) continue;

                // Check eligibility: player must be large enough to consume
                if (player.Radius < obj.RequiredConsumeRadius * consumeFactor) continue;

                Vector2 delta = player.Position - obj.Position;
                float distSq  = delta.sqrMagnitude;
                if (distSq < 0.0001f) { AbsorbImmediate(player, obj); continue; }

                float dist    = Mathf.Sqrt(distSq);
                float force   = G * obj.Mass / distSq;
                float speed   = Mathf.Min(force, maxSpeed);
                Vector2 move  = (delta / dist) * speed * dt;

                // Update position (the MonoBehaviour side reads obj.Position)
                obj.Position += move;
                obj.IsBeingPulled = true;

                // If reached absorption radius, flag for immediate consume
                if (dist <= player.Radius)
                    AbsorbImmediate(player, obj);
            }
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void AbsorbImmediate(PlayerEntity player, IGravityAffectable obj)
        {
            // Delegate to AbsorptionService via event — just mark the object
            obj.IsBeingPulled = false;
            OnAbsorbReady?.Invoke(player, obj);
        }

        /// <summary>Fired when an object reaches the player centre and should be absorbed.</summary>
        public event System.Action<PlayerEntity, IGravityAffectable> OnAbsorbReady;
    }
}
