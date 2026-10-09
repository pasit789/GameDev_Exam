using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pasit
{
    public class OptionsController : MonoBehaviour
    {
        [Header("UI Sliders")]
        public Slider masterSlider;
        public Slider bgmSlider;
        public Slider sfxSlider;

        [Header("Value Texts")]
        public TextMeshProUGUI masterValueText;
        public TextMeshProUGUI bgmValueText;
        public TextMeshProUGUI sfxValueText;

        private void Start()
        {
            // Auto find components if not assigned
            if (masterSlider == null || bgmSlider == null || sfxSlider == null)
            {
                var sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include);
                foreach (var s in sliders)
                {
                    if (s.gameObject.name.Contains("Master") && masterSlider == null) masterSlider = s;
                    if (s.gameObject.name.Contains("BGM") && bgmSlider == null) bgmSlider = s;
                    if (s.gameObject.name.Contains("SFX") && sfxSlider == null) sfxSlider = s;
                }
            }

            if (masterValueText == null || bgmValueText == null || sfxValueText == null)
            {
                var texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.Contains("Master") && masterValueText == null) masterValueText = t;
                    if (t.gameObject.name.Contains("BGM") && bgmValueText == null) bgmValueText = t;
                    if (t.gameObject.name.Contains("SFX") && sfxValueText == null) sfxValueText = t;
                }
            }

            float initialMaster = AudioManager.Instance.masterVolume;
            float initialBgm = AudioManager.Instance.bgmVolume;
            float initialSfx = AudioManager.Instance.sfxVolume;

            if (masterSlider != null)
            {
                masterSlider.minValue = 0f;
                masterSlider.maxValue = 1f;
                masterSlider.value = initialMaster;
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

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

            UpdateMasterText(initialMaster);
            UpdateBgmText(initialBgm);
            UpdateSfxText(initialSfx);
        }

        public void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
            UpdateMasterText(value);
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

        private void UpdateMasterText(float value)
        {
            if (masterValueText != null)
            {
                int pct = Mathf.RoundToInt(value * 100f);
                masterValueText.text = masterValueText.name.Contains("Badge") ? $"{pct}%" : $"Master Volume: {pct}%";
            }
        }

        private void UpdateBgmText(float value)
        {
            if (bgmValueText != null)
            {
                int pct = Mathf.RoundToInt(value * 100f);
                bgmValueText.text = bgmValueText.name.Contains("Badge") ? $"{pct}%" : $"BGM Volume: {pct}%";
            }
        }

        private void UpdateSfxText(float value)
        {
            if (sfxValueText != null)
            {
                int pct = Mathf.RoundToInt(value * 100f);
                sfxValueText.text = sfxValueText.name.Contains("Badge") ? $"{pct}%" : $"SFX Volume: {pct}%";
            }
        }
    }
}
