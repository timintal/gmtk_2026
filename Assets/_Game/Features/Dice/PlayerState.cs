using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice
{
    public class PlayerState : IResource
    {
        public int RerollsCount = 999;
        public int CurrentLevel = 1;
        public int DrawPerTurn = 3;
        
        public List<string> AllBlessings;
        public List<string> DrawPile;
        public List<string> DiscardPile;

        public void RestoreBlessings()
        {
            DrawPile.Clear();
            DrawPile.AddRange(AllBlessings);
            DiscardPile.Clear();
        }
    }
}