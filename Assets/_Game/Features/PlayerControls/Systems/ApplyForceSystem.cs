using Code.Common;
using Code.Common.View;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.PlayerControls
{
    public class ApplyForceSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<All<RigidbodyLink, Direction, Force, HasInput>>().Entities())
            {
                var rb = e.Read<RigidbodyLink>();
                var direction = e.Read<Direction>();
                var force = e.Read<Force>();

                rb.Value.linearVelocity += direction.Value * force.Value * dt;
                // rb.Value.AddForce(direction.Value * force.Value * dt);
            }
        }
    }
}