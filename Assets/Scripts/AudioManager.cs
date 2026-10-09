using UnityEngine;

namespace Pasit
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip bgmClip;
        public AudioClip itemSfxClip;
        public AudioClip obstacleSfxClip;

        [Header("Volume Settings (0.0 to 1.0)")]
        [Range(0f, 1f)] public float masterVolume = 1.0f;
        [Range(0f, 1f)] public float bgmVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        private const string MASTER_KEY = "MasterVolume";
        private const string BGM_KEY = "BGMVolume";
        private const string SFX_KEY = "SFXVolume";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1.0f);
            bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 0.8f);
            sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.8f);

            UpdateAudioSourcesVolume();

            if (bgmClip == null) bgmClip = Resources.Load<AudioClip>("Audio/BGM_Default");
            if (itemSfxClip == null) itemSfxClip = Resources.Load<AudioClip>("Audio/SFX_Item");
            if (obstacleSfxClip == null) obstacleSfxClip = Resources.Load<AudioClip>("Audio/SFX_Obstacle");

            PlayBGM();
        }

        public void PlayBGM()
        {
            if (bgmSource != null && bgmClip != null)
            {
                if (bgmSource.clip != bgmClip)
                {
                    bgmSource.clip = bgmClip;
                }
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }

        private void UpdateAudioSourcesVolume()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = bgmVolume * masterVolume;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAudioSourcesVolume();
            PlayerPrefs.SetFloat(MASTER_KEY, masterVolume);
            PlayerPrefs.Save();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateAudioSourcesVolume();
            PlayerPrefs.SetFloat(BGM_KEY, bgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAudioSourcesVolume();
            PlayerPrefs.SetFloat(SFX_KEY, sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlayItemSFX()
        {
            if (sfxSource != null && itemSfxClip != null)
            {
                sfxSource.PlayOneShot(itemSfxClip, sfxVolume * masterVolume);
            }
        }

        public void PlayObstacleSFX()
        {
            if (sfxSource != null && obstacleSfxClip != null)
            {
                sfxSource.PlayOneShot(obstacleSfxClip, sfxVolume * masterVolume);
            }
        }
    }
}
