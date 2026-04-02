using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.MapGeneration;
using BlacHole.Gameplay.Bots;

namespace BlacHole.Core
{
    /// <summary>
    /// Owns the running game session.
    /// Holds references to all services, starts/stops the game loop, handles pause.
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject botPrefab;

        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private BotSpawner botSpawner;

        public bool IsPaused { get; private set; }
        public bool IsGameOver { get; private set; }

        private EventBus _eventBus;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _eventBus = ServiceLocator.Get<EventBus>();
            StartGame();
        }

        public void StartGame()
        {
            IsGameOver = false;
            IsPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameSessionManager] Game started.");
        }

        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            _eventBus?.Fire(new GamePausedEvent(true));
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            _eventBus?.Fire(new GamePausedEvent(false));
        }

        public void EndGame(int winnerId)
        {
            IsGameOver = true;
            Time.timeScale = 0f;
            _eventBus?.Fire(new GameOverEvent(winnerId));
            Debug.Log($"[GameSessionManager] Game Over. Winner: {winnerId}");
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            ServiceLocator.Get<SceneLoader>().LoadScene("Game");
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            ServiceLocator.Get<SceneLoader>().LoadScene("MainMenu");
        }
    }

    // ── Events ──────────────────────────────────────────────────────────────
    public readonly struct GamePausedEvent { public readonly bool IsPaused; public GamePausedEvent(bool p) { IsPaused = p; } }
    public readonly struct GameOverEvent   { public readonly int  WinnerId;  public GameOverEvent(int id)   { WinnerId = id; } }
}
