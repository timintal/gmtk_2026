using FFS.Libraries.StaticEcs;

namespace Code.Common.Cleanup
{
    public class CleanupChildrenForDestroyedEntitySystem : ISystem
    {
        public void Update()
        {
            W.Query<All<Destroyed>>().For(static (W.Entity e, in W.Links<Children> links) =>
            {
                foreach (var child in links)
                {
                    if (child.Value.TryUnpack<WT>(out var childEntity))
                    {
                        childEntity.Set<Destroyed>();
                    }
                }
            });
        }
    }
}