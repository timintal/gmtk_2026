using FFS.Libraries.StaticEcs;

namespace Code.Common.View
{
    public class InitRigidbodySystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<RigidbodyLink, NeedInitRigidbody, Speed, Direction>>().Entities())
            {
                ref var rb = ref e.Ref<RigidbodyLink>();
                ref var speed = ref e.Ref<Speed>();
                ref var direction = ref e.Ref<Direction>();
                rb.Value.linearVelocity = direction.Value.normalized * speed.Value;
                e.Delete<Speed>();
                e.Delete<Direction>();
                    
            }
            foreach (var e in W.Query<All<RigidbodyLink, NeedInitRigidbody, Rotation, RotationSpeed>>().Entities())
            {
                ref var rb = ref e.Ref<RigidbodyLink>();
                ref var rotation = ref e.Ref<Rotation>();
                ref var rotationSpeed = ref e.Ref<RotationSpeed>();
                
                rb.Value.rotation = rotation.Value;
                rb.Value.angularVelocity = rotationSpeed.Value;
                e.Delete<Rotation>();
                e.Delete<RotationSpeed>();
            }
            
            W.Query<All<RigidbodyLink,NeedInitRigidbody>>().BatchDelete<NeedInitRigidbody>();
        }
    }
}