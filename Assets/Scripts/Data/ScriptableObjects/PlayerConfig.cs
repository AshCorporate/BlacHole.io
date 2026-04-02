using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Player progression configuration.</summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "BlacHole/Config/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        public float BaseRadius   = 0.5f;
        public float RadiusScaleK = 0.3f;
        public float StartMass    = 10f;
    }
}
