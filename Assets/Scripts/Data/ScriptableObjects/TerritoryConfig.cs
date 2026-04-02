using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Territory capture configuration.</summary>
    [CreateAssetMenu(fileName = "TerritoryConfig", menuName = "BlacHole/Config/TerritoryConfig")]
    public class TerritoryConfig : ScriptableObject
    {
        public float GridCellSize               = 1f;
        public float EnemyTerritorySpeedDebuff  = -0.3f;
        public float StartTerritoryRadius       = 5f;
    }
}
