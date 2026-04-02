using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.Bots;
using BlacHole.Gameplay.Progression;
using BlacHole.Respawn;

namespace BlacHole.UI.HUD
{
    /// <summary>
    /// HUD controller: mass, XP bar, score, territory%, tail timer, leaderboard, pause button.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private TMP_Text  massText;
        [SerializeField] private TMP_Text  scoreText;
        [SerializeField] private TMP_Text  xpText;
        [SerializeField] private Slider    xpBar;
        [SerializeField] private TMP_Text  territoryText;
        [SerializeField] private TMP_Text  stateText;
        [SerializeField] private TMP_Text  tailTimerText;

        [Header("Leaderboard")]
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private TMP_Text  leaderboardEntryPrefab;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;

        private EventBus            _eventBus;
        private PlayerController    _player;
        private List<TMP_Text>      _leaderboardRows = new List<TMP_Text>();

        private void Awake()
        {
            ServiceLocator.TryGet(out _eventBus);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (_eventBus != null)
                _eventBus.Subscribe<ProgressionChangedEvent>(OnProgressionChanged);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ProgressionChangedEvent>(OnProgressionChanged);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _player);
        }

        private void Update()
        {
            if (_player == null)
            {
                ServiceLocator.TryGet(out _player);
                return;
            }

            var entity = _player.Entity;

            if (massText      != null) massText.text      = $"Mass: {entity.Mass:F1}";
            if (scoreText     != null) scoreText.text     = $"Score: {entity.Score:F0}";
            if (xpText        != null) xpText.text        = $"XP: {entity.XP:F0} (Lv{entity.Level})";
            if (xpBar         != null) xpBar.value        = (entity.XP % 100f) / 100f;
            if (stateText     != null) stateText.text     = entity.State.ToString();

            if (tailTimerText != null)
            {
                // TailService provides TimeRemaining via ITailService
                tailTimerText.text = "";  // updated via event or direct reference in extended version
            }

            RefreshLeaderboard();
        }

        // ── Leaderboard ──────────────────────────────────────────────────────

        private void RefreshLeaderboard()
        {
            if (leaderboardContainer == null) return;

            // Gather entries
            var entries = new List<LeaderboardEntry>();
            if (_player != null)
            {
                var e = _player.Entity;
                entries.Add(new LeaderboardEntry { Name = e.Name, Score = e.Score, Color = e.PlayerColor });
            }

            var bots = FindObjectsOfType<BotController>();
            foreach (var bot in bots)
            {
                if (bot.Entity == null) continue;
                entries.Add(new LeaderboardEntry { Name = bot.Entity.Name, Score = bot.Entity.Score, Color = bot.Entity.PlayerColor });
            }

            entries.Sort((a, b) => b.Score.CompareTo(a.Score));

            // Ensure enough row objects
            while (_leaderboardRows.Count < entries.Count && leaderboardEntryPrefab != null)
            {
                var row = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                _leaderboardRows.Add(row);
            }

            for (int i = 0; i < _leaderboardRows.Count; i++)
            {
                var row = _leaderboardRows[i];
                if (i < entries.Count)
                {
                    row.gameObject.SetActive(true);
                    row.text  = $"{i + 1}. {entries[i].Name}  {entries[i].Score:F0}";
                    row.color = entries[i].Color;
                }
                else
                {
                    row.gameObject.SetActive(false);
                }
            }
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void OnProgressionChanged(ProgressionChangedEvent evt)
        {
            // Force immediate refresh on progression event
        }

        private void OnPauseClicked()
        {
            if (ServiceLocator.TryGet(out GameSessionManager mgr))
                mgr.PauseGame();
        }
    }
}
