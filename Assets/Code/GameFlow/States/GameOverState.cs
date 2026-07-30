using System;
using Code.Common.Audio;
using Cysharp.Threading.Tasks;
using GameFlow.FSM;
using VContainer;

namespace Code.GameFlow
{
    [UnityEngine.Scripting.Preserve]
    public class GameOverProperties : IStateProperties
    {
        public string Reason;
    }
    
    [UnityEngine.Scripting.Preserve]
    public class GameOverState : FSMState<GameOverProperties>
    {
        [Inject] internal MusicGenericAudioSource _musicGenericAudioSource;
        [Inject] internal SfxGenericAudioSource _sfxGenericAudioSource;
        
        public override async UniTask OnEnter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            W.GetResource<GameOverScreen>().gameObject.SetActive(true);
            W.DestroyAllLoadedEntities();
            
            _musicGenericAudioSource.Stop();
            _sfxGenericAudioSource.Play(SoundType.GameOver);
        }

        public override UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }
    }
}
