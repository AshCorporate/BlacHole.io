using UnityEngine;

namespace BlacHole.Data
{
    public enum SpawnZonePreference { Uniform, Center, Edge }
    public enum SpawnType           { Static, Dynamic }

    /// <summary>Per-category spawn configuration ScriptableObject.</summary>
    [CreateAssetMenu(fileName = "SpawnCategoryConfig", menuName = "BlacHole/Config/SpawnCategoryConfig")]
    public class SpawnCategoryConfig : ScriptableObject
    {
        public string              CategoryName         = "Micro";
        public GameObject[]        Prefabs;
        public float               MinWeight            = 0.5f;
        public float               MaxWeight            = 1.5f;
        public float               XPReward             = 0.1f;
        public int                 TargetCount          = 300;
        public float               MinSpacing           = 0.5f;
        public float               MaxSpacing           = 1.0f;
        public SpawnZonePreference SpawnZonePreference  = SpawnZonePreference.Uniform;
        public SpawnType           SpawnType            = SpawnType.Static;
    }
}
