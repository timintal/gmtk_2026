using System;
using Code.Common.Audio;
using Cysharp.Threading.Tasks;
using Libraries.GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class GameOverProperties : IStateProperties
    {
        public string Reason;
    }
    
    [Preserve]
    public class GameOverState : FSMState<GameOverProperties>
    {
        public override async UniTask OnEnter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            W.GetResource<GameOverScreen>().gameObject.SetActive(true);
            W.DestroyAllLoadedEntities();
            W.GetResource<MusicAudioSource>().Stop();
            W.GetResource<SFXAudioSource>().Play(SoundType.GameOver);
        }

        public override UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }
    }
}
