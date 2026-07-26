using _Game.Features.Run;
using Code.Common.Audio;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;
using Libraries.GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class RunGameState : FSMState
    {
        public override async UniTask OnEnter()
        {
            W.GetResource<MusicAudioSource>().PlayLooped(SoundType.MainTheme);
            W.NewEntity<Default>().Set<StartNewRunRequest>();
        }
        
        public override async UniTask OnExit()
        {
        }
    }
}