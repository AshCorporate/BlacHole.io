using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.MapGeneration;
using BlacHole.Gameplay.Bots;

namespace BlacHole.Core
{
    /// <summary>
    /// Owns the running game session.
    /// Holds references to all services, starts/stops the game loop, handles pause.
    /// If services were not pre-registered by GameBootstrap (e.g. when starting Game scene directly),
    /// this manager bootstraps them itself so the scene always works in isolation.
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

            EnsureServices();
        }

        /// <summary>
        /// Ensure all required services exist in the ServiceLocator.
        /// If GameBootstrap already ran (via Boot scene), this is a no-op.
        /// If the Game scene was opened directly (e.g. in editor), we self-bootstrap.
        /// </summary>
        private void EnsureServices()
        {
            if (!ServiceLocator.TryGet<EventBus>(out _))
            {
                var eventBus = new EventBus();
                ServiceLocator.Register<EventBus>(eventBus);
                Debug.Log("[GameSessionManager] Self-bootstrapped EventBus (Boot scene was not loaded).");
            }

            if (!ServiceLocator.TryGet<TickManager>(out _))
            {
                var tickManager = gameObject.AddComponent<TickManager>();
                ServiceLocator.Register<TickManager>(tickManager);
                Debug.Log("[GameSessionManager] Self-bootstrapped TickManager.");
            }

            if (!ServiceLocator.TryGet<SceneLoader>(out _))
            {
                var sceneLoaderGo = new GameObject("SceneLoader");
                sceneLoaderGo.transform.SetParent(transform);
                var sceneLoader = sceneLoaderGo.AddComponent<SceneLoader>();
                ServiceLocator.Register<SceneLoader>(sceneLoader);
                Debug.Log("[GameSessionManager] Self-bootstrapped SceneLoader.");
            }
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

    // ── Events ───────────────────────────────────────────────────────────────
    public readonly struct GamePausedEvent { public readonly bool IsPaused; public GamePausedEvent(bool p) { IsPaused = p; } }
    public readonly struct GameOverEvent   { public readonly int  WinnerId;  public GameOverEvent(int id)   { WinnerId = id; } }
}