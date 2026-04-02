using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;

namespace BlacHole.UI.Popups
{
    /// <summary>
    /// Pause menu: Resume / Restart / MainMenu / Settings.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(OnResume);
            if (restartButton  != null) restartButton.onClick.AddListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);

            if (pausePanel != null) pausePanel.SetActive(false);

            // Listen for pause events
            if (ServiceLocator.TryGet(out EventBus bus))
                bus.Subscribe<GamePausedEvent>(OnPauseChanged);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out EventBus bus))
                bus.Unsubscribe<GamePausedEvent>(OnPauseChanged);
        }

        private void OnPauseChanged(GamePausedEvent evt)
        {
            if (pausePanel != null)
                pausePanel.SetActive(evt.IsPaused);
        }

        private void OnResume()
        {
            if (ServiceLocator.TryGet(out GameSessionManager mgr))
                mgr.ResumeGame();
        }

        private void OnRestart()
        {
            if (ServiceLocator.TryGet(out GameSessionManager mgr))
                mgr.RestartGame();
        }

        private void OnMainMenu()
        {
            if (ServiceLocator.TryGet(out GameSessionManager mgr))
                mgr.GoToMainMenu();
        }

        private void OnSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}
