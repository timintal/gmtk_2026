using _Game.Features.Blessings;
using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using VContainer;

namespace _Game.Features.Run
{
    public class InitRunSystem : ISystem
    {
        [Inject] internal BlessingsLibrary _blessingsLibrary;
        
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

                _blessingsLibrary.CreateBlessing("roll2").PutBlessingInDrawPile();
                _blessingsLibrary.CreateBlessing("roll2").PutBlessingInDrawPile();
                _blessingsLibrary.CreateBlessing("roll3").PutBlessingInDrawPile();
                _blessingsLibrary.CreateBlessing("reroll").PutBlessingInDrawPile();
                
                W.NewEntity<Default>().Set<StartNewLevelRequest>();

            }
            
        }
    }
}