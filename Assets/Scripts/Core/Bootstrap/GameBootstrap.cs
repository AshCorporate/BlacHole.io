using UnityEngine;
using UnityEngine.SceneManagement;
using BlacHole.Core.Infrastructure;

namespace BlacHole.Core
{
    /// <summary>
    /// Entry-point MonoBehaviour. Placed in the Boot scene.
    /// Initialises all core services, registers them with ServiceLocator,
    /// then loads the MainMenu scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitialiseServices();
        }

        private void InitialiseServices()
        {
            // Infrastructure
            var eventBus = new EventBus();
            ServiceLocator.Register<EventBus>(eventBus);

            var tickManager = gameObject.AddComponent<TickManager>();
            ServiceLocator.Register<TickManager>(tickManager);

            // SceneLoader
            var sceneLoaderGo = new GameObject("SceneLoader");
            sceneLoaderGo.transform.SetParent(transform);
            var sceneLoader = sceneLoaderGo.AddComponent<SceneLoader>();
            ServiceLocator.Register<SceneLoader>(sceneLoader);

            Debug.Log("[GameBootstrap] Services initialised. Loading MainMenu...");
        }

        private void Start()
        {
            var loader = ServiceLocator.Get<SceneLoader>();
            loader.LoadScene(mainMenuSceneName);
        }
    }
}
