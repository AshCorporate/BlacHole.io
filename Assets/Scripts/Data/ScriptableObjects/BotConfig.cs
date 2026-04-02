using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Bot AI configuration.</summary>
    [CreateAssetMenu(fileName = "BotConfig", menuName = "BlacHole/Config/BotConfig")]
    public class BotConfig : ScriptableObject
    {
        public int   BotCount                = 5;
        public float RespawnDelay            = 3f;
        public float SeekFoodWeight          = 1f;
        public float CaptureTerritoryWeight  = 0.8f;
        public float ReturnHomeWeight        = 1.2f;
        public float FleeDangerWeight        = 1.5f;
        public float ChaseWeight             = 0.6f;
    }
}
