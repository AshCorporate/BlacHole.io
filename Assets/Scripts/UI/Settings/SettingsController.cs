using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace BlacHole.UI.Settings
{
    /// <summary>
    /// Settings panel: sound/music volume, quality dropdown, input mode placeholder.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider        soundVolumeSlider;
        [SerializeField] private Slider        musicVolumeSlider;
        [SerializeField] private AudioMixer    audioMixer;

        [Header("Quality")]
        [SerializeField] private TMP_Dropdown  qualityDropdown;

        [Header("Input Mode")]
        [SerializeField] private TMP_Dropdown  inputModeDropdown;

        private const string SoundVolumeKey = "SoundVolume";
        private const string MusicVolumeKey = "MusicVolume";

        private void Awake()
        {
            // Load saved values
            float sound = PlayerPrefs.GetFloat(SoundVolumeKey, 1f);
            float music = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.value = sound;
                soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = music;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>(QualitySettings.names);
                qualityDropdown.AddOptions(options);
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
        }

        private void OnSoundVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SoundVolumeKey, value);
            if (audioMixer != null)
                audioMixer.SetFloat("SoundVolume", LinearToDecibel(value));
        }

        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            if (audioMixer != null)
                audioMixer.SetFloat("MusicVolume", LinearToDecibel(value));
        }

        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
        }

        private static float LinearToDecibel(float linear)
            => linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
    }
}
