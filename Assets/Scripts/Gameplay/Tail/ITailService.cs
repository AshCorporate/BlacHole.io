using System.Collections.Generic;
using UnityEngine;

namespace BlacHole.Gameplay.Tail
{
    /// <summary>Interface for tail management systems.</summary>
    public interface ITailService
    {
        /// <summary>Advance the tail one tick.</summary>
        void Tick(float deltaTime);

        /// <summary>Read-only list of tail world positions (newest first).</summary>
        IReadOnlyList<Vector2> TailPoints { get; }

        /// <summary>Time remaining before tail timeout stun.</summary>
        float TimeRemaining { get; }

        /// <summary>True when the tail is active (player is outside own territory).</summary>
        bool IsActive { get; }

        /// <summary>Clear all tail points.</summary>
        void Clear();

        /// <summary>Begin recording tail from given position.</summary>
        void StartTail(Vector2 startPos);

        /// <summary>Stop and seal the tail (player returned to territory).</summary>
        void StopTail();
    }
}
