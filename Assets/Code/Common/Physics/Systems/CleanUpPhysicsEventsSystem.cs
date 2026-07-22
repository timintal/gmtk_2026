using FFS.Libraries.StaticEcs;

namespace Code.Common.Physics
{
    public class CleanUpPhysicsEventsSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<CollisionEnter2D>>().BatchSet<Destroyed>();
            W.Query<All<CollisionExit2D>>().BatchSet<Destroyed>();
            W.Query<All<TriggerEnter2D>>().BatchSet<Destroyed>();
            W.Query<All<TriggerExit2D>>().BatchSet<Destroyed>();
        }
    }
}