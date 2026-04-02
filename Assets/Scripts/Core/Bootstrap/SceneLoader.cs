using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlacHole.Core
{
    /// <summary>
    /// Async scene loading with optional loading screen support.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Loading Screen")]
        [SerializeField] private string loadingSceneName = "Loading";
        [SerializeField] private bool useLoadingScreen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            if (useLoadingScreen)
                yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            yield return op;

            if (useLoadingScreen)
                SceneManager.UnloadSceneAsync(loadingSceneName);
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            if (useLoadingScreen)
                yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

            var op = SceneManager.LoadSceneAsync(sceneIndex);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            yield return op;

            if (useLoadingScreen)
                SceneManager.UnloadSceneAsync(loadingSceneName);
        }
    }
}
