using FFS.Libraries.StaticEcs;

namespace Code.Common.Cleanup
{
    public class CleanupDestroyedEntitiesSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<Destroyed>>().For(entity =>
            {
                entity.Destroy();
            });
        }
    }

}