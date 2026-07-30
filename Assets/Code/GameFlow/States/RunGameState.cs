using _Game.Features.Run;
using Code.Common.Audio;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;
using GameFlow.FSM;
using VContainer;

namespace Code.GameFlow
{
    [UnityEngine.Scripting.Preserve]
    public class RunGameState : FSMState
    {
        [Inject] internal MusicGenericAudioSource _musicGenericAudioSource;
        
        public override UniTask OnEnter()
        {
            _musicGenericAudioSource.PlayLooped(SoundType.MainTheme);
            W.NewEntity<Default>().Set<StartNewRunRequest>();
            return UniTask.CompletedTask;
        }
    }
}