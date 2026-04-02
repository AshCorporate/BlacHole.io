using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Map generation configuration.</summary>
    [CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "BlacHole/Config/MapGenerationConfig")]
    public class MapGenerationConfig : ScriptableObject
    {
        public int                    MapWidth         = 100;
        public int                    MapHeight        = 100;
        public int                    DefaultSeed      = 12345;
        public bool                   UseRandomSeed    = true;
        public SpawnCategoryConfig[]  CategoriesConfig;
    }
}
