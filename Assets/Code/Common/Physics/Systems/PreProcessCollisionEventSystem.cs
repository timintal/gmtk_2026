using Code.Common.View;
using FFS.Libraries.StaticEcs;

namespace Code.Common.Physics
{
    public class PreProcessCollisionEventSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<TriggerEnter2D, W.Link<Owner>>>().Entities())
            {
                var triggerEnter2D = e.Read<TriggerEnter2D>();

                if (W.GetResource<ColliderRegistry>().TryResolve(triggerEnter2D.Value, out var otherGid))
                {
                    if (otherGid.TryUnpack<WT>(out var otherEntity))
                    {
                        e.Set(new W.Link<OtherEntity>(otherEntity));
                    }
                }
            }
            
            foreach (var e in W.Query<All<TriggerExit2D, W.Link<Owner>>>().Entities())
            {
                var triggerExit2D = e.Read<TriggerExit2D>();

                if (W.GetResource<ColliderRegistry>().TryResolve(triggerExit2D.Value, out var otherGid))
                {
                    if (otherGid.TryUnpack<WT>(out var otherEntity))
                    {
                        e.Set(new W.Link<OtherEntity>(otherEntity));
                    }
                }
            }
            
            foreach (var e in W.Query<All<CollisionEnter2D, W.Link<Owner>>>().Entities())
            {
                var collisionEnter2D = e.Read<CollisionEnter2D>();

                var hasFirstCollider = W.GetResource<ColliderRegistry>().TryResolve(collisionEnter2D.Value.collider, out var firstGid);
                var hasSecondCollider = W.GetResource<ColliderRegistry>().TryResolve(collisionEnter2D.Value.otherCollider, out var secondGid);
                if (hasSecondCollider && hasFirstCollider)
                {
                    if (firstGid.TryUnpack<WT>(out var firstEntity) && firstEntity != e.Read<W.Link<Owner>>().Value)
                    {
                        e.Set(new W.Link<OtherEntity>(firstEntity));
                    }
                    else if (secondGid.TryUnpack<WT>(out var secondEntity) && secondEntity != e.Read<W.Link<Owner>>().Value)
                    {
                        e.Set(new W.Link<OtherEntity>(secondEntity));
                    }
                }
            }
            
            foreach (var e in W.Query<All<CollisionExit2D, W.Link<Owner>>>().Entities())
            {
                var collisionExit2D = e.Read<CollisionExit2D>();
                
                var hasFirstCollider = W.GetResource<ColliderRegistry>().TryResolve(collisionExit2D.Value.collider, out var firstGid);
                var hasSecondCollider = W.GetResource<ColliderRegistry>().TryResolve(collisionExit2D.Value.otherCollider, out var secondGid);
                
                if (hasSecondCollider && hasFirstCollider)
                {
                    if (firstGid.TryUnpack<WT>(out var firstEntity) && firstEntity != e.Read<W.Link<Owner>>().Value)
                    {
                        e.Set(new W.Link<OtherEntity>(firstEntity));
                    }
                    else if (secondGid.TryUnpack<WT>(out var secondEntity) && secondEntity != e.Read<W.Link<Owner>>().Value)
                    {
                        e.Set(new W.Link<OtherEntity>(secondEntity));
                    }
                }
            }
        }
    }
}