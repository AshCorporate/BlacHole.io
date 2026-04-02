using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>Tail behaviour configuration.</summary>
    [CreateAssetMenu(fileName = "TailConfig", menuName = "BlacHole/Config/TailConfig")]
    public class TailConfig : ScriptableObject
    {
        public float RecordInterval  = 0.08f;
        public float MaxTailDuration = 10f;
        public float StunDuration    = 1f;
        public int   MaxPoints       = 500;
    }
}
