using _Game.Features.Enemies;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class LevelFinishedSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<Enemy>>().EntitiesCount() == 0)
            {
                
            }
        }
    }
}