using UnityEngine;

namespace BlacHole.Gameplay.Movement
{
    /// <summary>Contract for all movement implementations.</summary>
    public interface IMovementMotor
    {
        /// <summary>Advance movement one tick.</summary>
        void Tick(Vector2 input, float deltaTime);

        /// <summary>Current velocity (world units/sec).</summary>
        Vector2 Velocity { get; }

        /// <summary>Set map bounds for clamping (min corner, max corner).</summary>
        void SetBounds(Vector2 min, Vector2 max);
    }
}
