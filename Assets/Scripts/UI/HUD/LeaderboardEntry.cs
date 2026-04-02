using UnityEngine;

namespace BlacHole.UI.HUD
{
    /// <summary>Data class for a leaderboard row.</summary>
    public class LeaderboardEntry
    {
        public string Name  { get; set; }
        public float  Score { get; set; }
        public Color  Color { get; set; } = Color.white;
    }
}
