using UnityEngine;

namespace BlacHole.Data
{
    /// <summary>UI styling configuration.</summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "BlacHole/Config/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        [Header("Colors")]
        public Color PanelBackground   = new Color(0.051f, 0.051f, 0.169f, 0.9f);  // #0D0D2B
        public Color PrimaryText       = new Color(0f,     1f,     1f,     1f);    // neon cyan
        public Color SecondaryText     = Color.white;
        public Color AccentColor       = new Color(0f,     1f,     1f,     1f);

        [Header("Font Sizes")]
        public float HeaderFontSize    = 32f;
        public float BodyFontSize      = 20f;
        public float SmallFontSize     = 14f;

        [Header("Leaderboard")]
        public int   MaxLeaderboardEntries = 10;
    }
}
