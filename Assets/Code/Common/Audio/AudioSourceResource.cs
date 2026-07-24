using Code.Common.View;
using UnityEngine;

namespace Code.Common.Audio
{
    public abstract class AudioSourceResource<T> : ResourceMonoBehaviour<T> where T : AudioSourceResource<T>
    {
        public AudioSource AudioSource;
        protected abstract string PrefsKey { get; }
        
        [SerializeField] private AudioConfig _config;

        void Awake()
        {
            SetVolume(PlayerPrefs.GetFloat(PrefsKey, 1f));
        }
        
        public void Play(SoundType type)
        {
            if (_config == null || type == SoundType.None)
            {
                return;
            }

            var clip = _config.GetClip(type);
            if (clip != null)
            {
                AudioSource.PlayOneShot(clip);
            }
        }
        
        public void SetVolume(float volume)
        {
            AudioSource.volume = volume;
            PlayerPrefs.SetFloat(PrefsKey, volume);
            PlayerPrefs.Save();
        }
        
        public void PlayOneShot(AudioClip clip)
        {
            AudioSource.PlayOneShot(clip);
        }
        
        public void Play()
        {
            AudioSource.Play();
        }
    }
}