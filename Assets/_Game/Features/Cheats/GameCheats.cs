using _Game.Features.Enemies;
using _Game.Utils;
using Code.Common;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;

namespace _Game.Features.Cheats
{
    public class GameCheats 
    {
        private static GameCheats _current;

        public static GameCheats Current
        {
            get { return _current; }
        }
        
#if !DISABLE_SRDEBUGGER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnStartup()
        {
            _current = new GameCheats(); // Need to reset options here so if we enter play-mode without a domain reload there will be the default set of options.
            SRDebuggerUtils.AddOptionContainer(Current);
        }
#endif
        [Category("Gameplay")]
        public void WinLevel()
        {
            W.Query<All<Enemy>>().BatchSet(new Destroyed());
        }
        
    }
}