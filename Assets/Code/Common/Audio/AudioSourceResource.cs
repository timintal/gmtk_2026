using Code.Common.View;
using UnityEngine;

namespace Code.Common.Audio
{
    public class AudioSourceResource<T> : ResourceMonoBehaviour<T> where T : AudioSourceResource<T>
    {
        public AudioSource AudioSource;
        
        public void SetVolume(float volume)
        {
            AudioSource.volume = volume;
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