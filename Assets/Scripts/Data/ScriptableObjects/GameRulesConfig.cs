using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Game rules configuration.</summary>
    [CreateAssetMenu(fileName = "GameRulesConfig", menuName = "BlacHole/Config/GameRulesConfig")]
    public class GameRulesConfig : ScriptableObject
    {
        public float BounceThreshold    = 0.2f;
        public float AbsorptionMassRatio = 1.2f;
        public float BounceLockDuration = 0.3f;
        public float RespawnDelay       = 3f;
        public bool  TailKillsPlayer    = true;
    }
}
