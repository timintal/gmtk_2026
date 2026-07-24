using UnityEngine;

namespace Code.Common.Audio
{
    public class SFXAudioSource : AudioSourceResource<SFXAudioSource>
    {
        [SerializeField] private AudioConfig _config;

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

        public void PlayDiceRoll()
        {
            Play(SoundType.DiceRoll);
        }
    }
}
