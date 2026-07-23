using Code.Common.View;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.PlayerControls
{
    public class BreakSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<
                         All<RigidbodyLink, BreakForce>, 
                         None<HasInput>>().Entities())
            {
                var rb = e.Read<RigidbodyLink>();
                var breakForce = e.Read<BreakForce>();

                rb.Value.AddForce(-rb.Value.linearVelocity.normalized * breakForce.Value * dt);
            }
        }
    }
}