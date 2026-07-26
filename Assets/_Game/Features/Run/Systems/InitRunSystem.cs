using System.Collections.Generic;
using _Game.Features.Blessings;
using _Game.Features.Dice;
using Code.Common;
using Code.Features.EnergyFeature;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class InitRunSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewRunRequest>>();
            if (requestQuery.EntitiesCount() > 0)
            {
                requestQuery.BatchDestroy();
                
                W.Query<All<Blessing>>().BatchSet(new Destroyed());
                W.Query<All<Dice.Dice>>().BatchSet(new Destroyed());
                
                var playerState = new PlayerState()
                {
                    RerollsCount = 999,
                    CurrentLevel = 0,
                };
                
                W.SetResource(playerState);

                var blessingsLibrary = W.GetResource<BlessingsLibrary>();
                blessingsLibrary.CreateBlessing("roll2").PutBlessingInDrawPile();
                blessingsLibrary.CreateBlessing("roll2").PutBlessingInDrawPile();
                blessingsLibrary.CreateBlessing("roll3").PutBlessingInDrawPile();
                blessingsLibrary.CreateBlessing("reroll").PutBlessingInDrawPile();
                
                W.NewEntity<Default>().Set<StartNewLevelRequest>();

            }
            
        }
    }
}