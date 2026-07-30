using System.Collections;
using Code.Common.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace _Game.Features.Visuals
{
    public class Lightning : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _lightning;
        
        [Inject] internal SfxGenericAudioSource _sfxGenericAudioSource;
    
        [Button]
        public void PlayLightning(Vector2 start, Vector2 end, float duration, float delay)
        {
            _lightning.enabled = false;
            transform.position = start;
            StartCoroutine(PlayLightningCoroutine(start, end, duration, delay));
        }
    
        IEnumerator PlayLightningCoroutine(Vector2 start, Vector2 end, float duration, float delay)
        {
            yield return new WaitForSeconds(delay);
            _lightning.enabled = true;
            _sfxGenericAudioSource.Play(SoundType.Zap);
            var signedAngle = Vector2.SignedAngle(Vector2.up, end - start);
            _lightning.transform.rotation = Quaternion.Euler(0, 0, signedAngle);
            _lightning.size = new Vector2(1, Vector2.Distance(start, end));
            yield return new WaitForSeconds(duration);
            Destroy(gameObject);
        }

    }
}
