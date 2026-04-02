using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlacHole.Core;

namespace BlacHole.UI.MainMenu
{
    /// <summary>
    /// Main menu controller.
    /// Wires Play, Settings, Quit buttons; exposes seed field, bot-count slider, map-size dropdown.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Game Settings")]
        [SerializeField] private TMP_InputField seedInputField;
        [SerializeField] private Slider         botCountSlider;
        [SerializeField] private TMP_Text       botCountLabel;
        [SerializeField] private TMP_Dropdown   mapSizeDropdown;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            if (playButton     != null) playButton.onClick.AddListener(OnPlayClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (quitButton     != null) quitButton.onClick.AddListener(OnQuitClicked);

            if (botCountSlider != null)
            {
                botCountSlider.minValue = 1;
                botCountSlider.maxValue = 10;
                botCountSlider.wholeNumbers = true;
                botCountSlider.onValueChanged.AddListener(OnBotCountChanged);
                UpdateBotCountLabel((int)botCountSlider.value);
            }

            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OnPlayClicked()
        {
            // Store settings in PlayerPrefs so Game scene can read them
            if (seedInputField != null && !string.IsNullOrWhiteSpace(seedInputField.text))
                PlayerPrefs.SetString("Seed", seedInputField.text);
            else
                PlayerPrefs.DeleteKey("Seed");

            if (botCountSlider != null)
                PlayerPrefs.SetInt("BotCount", (int)botCountSlider.value);

            if (mapSizeDropdown != null)
                PlayerPrefs.SetInt("MapSize", mapSizeDropdown.value);

            ServiceLocator.Get<SceneLoader>().LoadScene("Game");
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnBotCountChanged(float value)
        {
            UpdateBotCountLabel((int)value);
        }

        private void UpdateBotCountLabel(int count)
        {
            if (botCountLabel != null)
                botCountLabel.text = $"Bots: {count}";
        }
    }
}
