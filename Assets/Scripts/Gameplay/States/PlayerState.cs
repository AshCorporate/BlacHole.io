using UnityEngine;

namespace BlacHole.Gameplay.States
{
    /// <summary>Player state enum.</summary>
    public enum PlayerState
    {
        Safe,       // On own territory
        Capturing,  // Outside own territory drawing a tail
        Dead,       // Eliminated
        Stunned     // Tail timeout — brief control lock
    }
}
