using UnityEngine;
using BlacHole.Gameplay.Player;
using BlacHole.Data;

namespace BlacHole.Gameplay.Collision
{
    /// <summary>
    /// MonoBehaviour that handles:
    ///   • Map boundary clamping
    ///   • Player-vs-player bounce (mass diff &lt; 20%) with 0.3s control lock
    ///   • Player absorption (massA &gt; massB * 1.2)
    ///   • Smooth slide along static objects
    /// </summary>
    public class CollisionResolver : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameRulesConfig rulesConfig;

        // ── Boundary clamp ───────────────────────────────────────────────────

        public void ClampToBounds(PlayerEntity entity, Vector2 boundsMin, Vector2 boundsMax)
        {
            float r = entity.Radius;
            float x = Mathf.Clamp(entity.Position.x, boundsMin.x + r, boundsMax.x - r);
            float y = Mathf.Clamp(entity.Position.y, boundsMin.y + r, boundsMax.y - r);
            entity.Position = new Vector2(x, y);
        }

        // ── Player vs Player ─────────────────────────────────────────────────

        public enum PlayerCollisionResult { None, Bounce, AbsorbA, AbsorbB }

        public PlayerCollisionResult ResolvePlayerVsPlayer(
            PlayerEntity a, PlayerEntity b, out PlayerEntity absorbed)
        {
            absorbed = null;
            float dist = Vector2.Distance(a.Position, b.Position);
            if (dist > a.Radius + b.Radius) return PlayerCollisionResult.None;

            float ratio        = rulesConfig != null ? rulesConfig.AbsorptionMassRatio  : 1.2f;
            float bounceThresh = rulesConfig != null ? rulesConfig.BounceThreshold       : 0.2f;
            float lockDur      = rulesConfig != null ? rulesConfig.BounceLockDuration    : 0.3f;

            float massDiff = Mathf.Abs(a.Mass - b.Mass) / Mathf.Max(a.Mass, b.Mass);

            if (a.Mass > b.Mass * ratio)
            {
                absorbed = b;
                return PlayerCollisionResult.AbsorbA;
            }
            if (b.Mass > a.Mass * ratio)
            {
                absorbed = a;
                return PlayerCollisionResult.AbsorbB;
            }

            // Bounce
            if (massDiff <= bounceThresh)
            {
                Bounce(a, b, lockDur);
                return PlayerCollisionResult.Bounce;
            }

            return PlayerCollisionResult.None;
        }

        private void Bounce(PlayerEntity a, PlayerEntity b, float lockDur)
        {
            Vector2 normal = (a.Position - b.Position).normalized;
            float overlap  = (a.Radius + b.Radius) - Vector2.Distance(a.Position, b.Position);

            // Push apart
            a.Position += normal * overlap * 0.5f;
            b.Position -= normal * overlap * 0.5f;

            // Lock controls briefly
            a.ControlLockTimer = lockDur;
            b.ControlLockTimer = lockDur;
        }

        // ── Slide along static circle obstacle ───────────────────────────────

        public void SlideAlongCircle(PlayerEntity entity, Vector2 obstacleCenter, float obstacleRadius)
        {
            Vector2 delta = entity.Position - obstacleCenter;
            float dist    = delta.magnitude;
            float minDist = entity.Radius + obstacleRadius;

            if (dist < minDist && dist > 0.001f)
                entity.Position = obstacleCenter + delta.normalized * minDist;
        }
    }
}
