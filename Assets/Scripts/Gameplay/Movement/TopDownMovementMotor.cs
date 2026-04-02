using UnityEngine;
using BlacHole.Gameplay.Player;
using BlacHole.Data;

namespace BlacHole.Gameplay.Movement
{
    /// <summary>
    /// Custom top-down movement controller. No Rigidbody — uses Transform.position only.
    ///
    /// Formula summary:
    ///   Direction     = Normalize(inputVector)
    ///   Speed         = Clamp(Vbase / sqrt(Mass) * SpeedModifier, Vmin, Vmax)
    ///   TurnPenalty   = sqrt(Mass) * TurnPenaltyCoeff
    ///   TargetVelocity = Direction * Speed
    ///   Velocity      = SmoothDamp(Velocity, TargetVelocity, AccelerationTime)
    /// </summary>
    public class TopDownMovementMotor : IMovementMotor
    {
        private readonly PlayerEntity  _entity;
        private readonly MovementConfig _cfg;

        public Vector2 Velocity { get; private set; }

        private Vector2 _smoothRef;
        private Vector2 _boundsMin = new Vector2(-50f, -50f);
        private Vector2 _boundsMax = new Vector2( 50f,  50f);

        public TopDownMovementMotor(PlayerEntity entity, MovementConfig cfg)
        {
            _entity = entity;
            _cfg    = cfg;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            _boundsMin = min;
            _boundsMax = max;
        }

        public void Tick(Vector2 input, float dt)
        {
            // ── Direction ────────────────────────────────────────────────────
            Vector2 desiredDir = input.sqrMagnitude > 0.01f ? input.normalized : _entity.Direction;

            // Apply max turn rate with mass-based penalty
            float baseSpeed   = _cfg != null ? _cfg.BaseSpeed        : 10f;
            float vMin        = _cfg != null ? _cfg.Vmin             : 4f;
            float vMax        = _cfg != null ? _cfg.Vmax             : 15f;
            float accelTime   = _cfg != null ? _cfg.AccelerationTime : 0.25f;
            float maxTurnDeg  = _cfg != null ? _cfg.MaxTurnRate      : 360f;
            float turnPenCoeff= _cfg != null ? _cfg.TurnPenaltyCoeff : 0.05f;

            float mass           = Mathf.Max(0.01f, _entity.Mass);
            float turnPenalty    = Mathf.Sqrt(mass) * turnPenCoeff;
            float effectiveTurn  = Mathf.Max(0f, maxTurnDeg - turnPenalty) * dt;

            // Rotate current direction towards desired, clamped by effectiveTurn
            float currentAngle = Mathf.Atan2(_entity.Direction.y, _entity.Direction.x) * Mathf.Rad2Deg;
            float desiredAngle = Mathf.Atan2(desiredDir.y, desiredDir.x) * Mathf.Rad2Deg;
            float newAngle     = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, effectiveTurn);
            float rad          = newAngle * Mathf.Deg2Rad;
            _entity.Direction  = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // ── Speed ────────────────────────────────────────────────────────
            float speedMod   = _entity.SpeedModifier;
            float speed      = Mathf.Clamp(baseSpeed / Mathf.Sqrt(mass) * speedMod, vMin, vMax);

            Vector2 targetVel = _entity.Direction * speed * (input.sqrMagnitude > 0.01f ? 1f : 0f);

            // SmoothDamp towards target velocity
            Velocity = Vector2.SmoothDamp(Velocity, targetVel, ref _smoothRef, accelTime);

            // ── Position ─────────────────────────────────────────────────────
            Vector2 newPos = _entity.Position + Velocity * dt;

            // Boundary clamp
            float r = _entity.Radius;
            newPos.x = Mathf.Clamp(newPos.x, _boundsMin.x + r, _boundsMax.x - r);
            newPos.y = Mathf.Clamp(newPos.y, _boundsMin.y + r, _boundsMax.y - r);

            _entity.Position = newPos;
        }
    }
}
