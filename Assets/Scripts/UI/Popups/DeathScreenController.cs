using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;
using BlacHole.Respawn;

namespace BlacHole.UI.Popups
{
    /// <summary>
    /// Death screen: death message, respawn countdown, restart button.
    /// </summary>
    public class DeathScreenController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private TMP_Text   deathMessageText;
        [SerializeField] private TMP_Text   respawnTimerText;
        [SerializeField] private Button     restartButton;

        private EventBus _eventBus;

        private void Awake()
        {
            ServiceLocator.TryGet(out _eventBus);

            if (deathPanel != null) deathPanel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestart);

            if (_eventBus != null)
            {
                _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
                _eventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            _eventBus?.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (deathPanel != null)   deathPanel.SetActive(true);
            if (deathMessageText != null) deathMessageText.text = "YOU DIED";
            StartCoroutine(UpdateCountdown());
        }

        private void OnPlayerRespawned(PlayerRespawnedEvent evt)
        {
            if (deathPanel != null) deathPanel.SetActive(false);
        }

        private IEnumerator UpdateCountdown()
        {
            IRespawnService respawn = null;
            ServiceLocator.TryGet(out respawn);

            while (respawn != null && respawn.IsRespawning)
            {
                if (respawnTimerText != null)
                    respawnTimerText.text = $"Respawning in {respawn.RespawnCountdown:F1}s";
                yield return null;
            }

            if (respawnTimerText != null)
                respawnTimerText.text = "";
        }

        private void OnRestart()
        {
            if (ServiceLocator.TryGet(out GameSessionManager mgr))
                mgr.RestartGame();
        }
    }
}
