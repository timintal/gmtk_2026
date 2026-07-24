using _Game.Features.Enemies;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class LevelFinishedSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<LevelStarted>>().EntitiesCount() == 0)
                return;
            
            if (W.Query<All<Enemy>>().EntitiesCount() == 0)
            {
                W.Query<All<LevelStarted>>().BatchDestroy();
                W.NewEntity<Default>().Set<StartNewLevelRequest>();
            }
        }
    }
}