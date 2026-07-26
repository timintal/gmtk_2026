using System;
using Code.Common.Audio;
using Cysharp.Threading.Tasks;
using Libraries.GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class GameWonState : FSMState<GameOverProperties>
    {
        public async override UniTask OnEnter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            W.GetResource<GameWonScreen>().gameObject.SetActive(true);
            W.DestroyAllLoadedEntities();
            W.GetResource<MusicAudioSource>().Stop();
            W.GetResource<SFXAudioSource>().Play(SoundType.GameWin);
        }

        public override UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }
    }
}