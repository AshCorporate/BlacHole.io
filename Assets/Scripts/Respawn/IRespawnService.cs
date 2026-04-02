using BlacHole.Gameplay.Player;

namespace BlacHole.Respawn
{
    /// <summary>Interface for the respawn service.</summary>
    public interface IRespawnService
    {
        /// <summary>Initiate respawn sequence for the given entity.</summary>
        void RequestRespawn(PlayerEntity entity);

        bool IsRespawning { get; }
        float RespawnCountdown { get; }
    }
}
