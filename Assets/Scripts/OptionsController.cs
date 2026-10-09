using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pasit
{
    public class OptionsController : MonoBehaviour
    {
        [Header("UI Sliders")]
        public Slider bgmSlider;
        public Slider sfxSlider;

        [Header("Value Texts")]
        public TextMeshProUGUI bgmValueText;
        public TextMeshProUGUI sfxValueText;

        private void Start()
        {
            // Auto find components if not assigned
            if (bgmSlider == null || sfxSlider == null)
            {
                var sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
                foreach (var s in sliders)
                {
                    if (s.gameObject.name.Contains("BGM") && bgmSlider == null) bgmSlider = s;
                    if (s.gameObject.name.Contains("SFX") && sfxSlider == null) sfxSlider = s;
                }
            }

            if (bgmValueText == null || sfxValueText == null)
            {
                var texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.Contains("BGM") && bgmValueText == null) bgmValueText = t;
                    if (t.gameObject.name.Contains("SFX") && sfxValueText == null) sfxValueText = t;
                }
            }

            float initialBgm = AudioManager.Instance.bgmVolume;
            float initialSfx = AudioManager.Instance.sfxVolume;

            if (bgmSlider != null)
            {
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.value = initialBgm;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.value = initialSfx;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            UpdateBgmText(initialBgm);
            UpdateSfxText(initialSfx);
        }

        public void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }
            UpdateBgmText(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
                // Play preview sound so player can hear volume change
                AudioManager.Instance.PlayItemSFX();
            }
            UpdateSfxText(value);
        }

        private void UpdateBgmText(float value)
        {
            if (bgmValueText != null)
            {
                bgmValueText.text = "BGM Volume: " + Mathf.RoundToInt(value * 100f) + "%";
            }
        }

        private void UpdateSfxText(float value)
        {
            if (sfxValueText != null)
            {
                sfxValueText.text = "SFX Volume: " + Mathf.RoundToInt(value * 100f) + "%";
            }
        }
    }
}
