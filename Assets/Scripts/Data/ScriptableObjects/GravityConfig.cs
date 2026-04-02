using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Gravity / attraction configuration.</summary>
    [CreateAssetMenu(fileName = "GravityConfig", menuName = "BlacHole/Config/GravityConfig")]
    public class GravityConfig : ScriptableObject
    {
        public float GravityCoeff        = 9.8f;
        public float MaxAttractionSpeed  = 8f;
        public float ConsumeRadiusFactor = 1.0f;
    }
}
