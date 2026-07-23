using Code.Common.View;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.PlayerControls
{
    public class ClampSpeedSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<RigidbodyLink, MaxSpeed>>().Entities())
            {
                var rb = e.Read<RigidbodyLink>();
                var maxSpeed = e.Read<MaxSpeed>();

                if (rb.Value.linearVelocity.magnitude > maxSpeed.Value)
                {
                    rb.Value.linearVelocity = rb.Value.linearVelocity.normalized * maxSpeed.Value;
                }
            }
            
            foreach (var e in W.Query<All<RigidbodyLink, MaxRotationSpeed>>().Entities())
            {
                var rb = e.Read<RigidbodyLink>();
                var maxRotationSpeed = e.Read<MaxRotationSpeed>();

                if (Mathf.Abs(rb.Value.angularVelocity) > maxRotationSpeed.Value)
                {
                    rb.Value.angularVelocity = Mathf.Sign(rb.Value.angularVelocity) * maxRotationSpeed.Value;       
                }
            }
        }
    }
}