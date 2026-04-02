using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Movement configuration. Edit in Inspector — zero hardcode in gameplay code.</summary>
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "BlacHole/Config/MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        [Header("Speed")]
        public float BaseSpeed        = 10f;
        public float Vmin             = 4f;
        public float Vmax             = 15f;
        public float AccelerationTime = 0.25f;

        [Header("Turning")]
        public float MaxTurnRate      = 360f;
        public float TurnPenaltyCoeff = 0.05f;
    }
}
