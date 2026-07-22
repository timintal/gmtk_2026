using Code.Common.View;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common
{

    public class SyncTransformFromPositionSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in W.Query<All<TransformLink, SyncViewPosition, Rotation>>().Entities())
            {
                ref var rotation = ref entity.Ref<Rotation>();
                var transformLink = entity.Read<TransformLink>().Value;
                transformLink.localRotation = Quaternion.Euler(0, 0, rotation.Value);
            }
            
            foreach (var entity in W.Query<All<TransformLink, SyncViewPosition, Position>>().Entities())
            {
                ref var pos = ref entity.Ref<Position>();
                var t = entity.Read<TransformLink>().Value;
                ref readonly var sync = ref entity.Read<SyncViewPosition>();
                
                var preservedZ = ViewTransformUtility.ReadPreservedZ(t);
                var targetPosition = pos.Value;

                if (sync.Damping > 0 && !entity.Has<SkipSyncViewPositionDamping>())
                {
                    var currentPosition = ViewTransformUtility.ReadPosition(t);
                    var dt = W.GetResource<DeltaTime>().Value;
                    targetPosition = Vector2.Lerp(currentPosition, targetPosition, sync.Damping * dt);
                }

                ViewTransformUtility.WritePosition(t, targetPosition, preservedZ);
            }
            
            foreach (var entity in W.Query<All<RigidbodyLink, SyncPhysicsPosition, Position>>().Entities())
            {
                ref var pos = ref entity.Ref<Position>();
                var rb = entity.Read<RigidbodyLink>().Value;
                ref readonly var sync = ref entity.Read<SyncPhysicsPosition>();
                
                var targetPosition = pos.Value;

                if (sync.Damping > 0 && !entity.Has<SkipSyncViewPositionDamping>())
                {
                    var currentPosition = rb.position;
                    var dt = W.GetResource<DeltaTime>().Value;
                    targetPosition = Vector2.Lerp(currentPosition, targetPosition, sync.Damping * dt);
                }
                rb.position = targetPosition;

            }
        }
    }
}
