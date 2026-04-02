using UnityEngine;

namespace BlacHole.Gameplay.Gravity
{
    /// <summary>Interface for objects that can be pulled by gravity wells.</summary>
    public interface IGravityAffectable
    {
        Vector2  Position             { get; set; }
        float    Mass                 { get; }
        float    RequiredConsumeRadius { get; }
        bool     IsBeingPulled        { get; set; }
        bool     IsAvailable          { get; }
    }
}
