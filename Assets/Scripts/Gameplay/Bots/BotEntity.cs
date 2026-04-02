using UnityEngine;
using BlacHole.Gameplay.Player;

namespace BlacHole.Gameplay.Bots
{
    /// <summary>
    /// Data class mirroring PlayerEntity for bot-specific fields.
    /// Bots reuse the same PlayerEntity for all gameplay data.
    /// </summary>
    public class BotEntity
    {
        public PlayerEntity Entity { get; }

        // AI state
        public float  StateTimer   { get; set; }
        public string CurrentBehavior { get; set; } = "Wander";
        public Vector2 WanderTarget { get; set; }

        public BotEntity(int id, string name, Color color, float startMass = 10f, float startRadius = 0.5f)
        {
            Entity = new PlayerEntity
            {
                Id          = id,
                Name        = name,
                Mass        = startMass,
                Radius      = startRadius,
                PlayerColor = color,
                IsAlive     = true
            };
        }
    }
}
