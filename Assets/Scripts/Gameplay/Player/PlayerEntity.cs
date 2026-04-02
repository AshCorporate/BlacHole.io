using UnityEngine;
using BlacHole.Gameplay.States;

namespace BlacHole.Gameplay.Player
{
    /// <summary>
    /// Pure C# data class. Holds all runtime state for a single player entity.
    /// Shared between PlayerController and BotController.
    /// </summary>
    public class PlayerEntity
    {
        public int      Id          { get; set; }
        public string   Name        { get; set; } = "Player";
        public Vector2  Position    { get; set; }
        public Vector2  Direction   { get; set; } = Vector2.up;
        public float    Mass        { get; set; } = 10f;
        public float    Radius      { get; set; } = 0.5f;
        public float    XP          { get; set; }
        public float    Score       { get; set; }
        public int      Level       { get; set; } = 1;
        public bool     IsAlive     { get; set; } = true;
        public PlayerState State    { get; set; } = PlayerState.Safe;
        public Color    PlayerColor { get; set; } = Color.cyan;

        // Speed modifier applied by buffs (enemy territory debuff, etc.)
        public float    SpeedModifier { get; set; } = 1f;

        // Control lock timer (bounce / stun)
        public float    ControlLockTimer { get; set; }

        public bool IsControlLocked => ControlLockTimer > 0f;
    }
}
