using FFS.Libraries.StaticEcs;

namespace Code.Common
{
    public class RotationSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<All<Rotation, RotationSpeed>>().Entities())
            {
                ref var rotation = ref e.Ref<Rotation>();
                ref readonly var rotationSpeed = ref e.Read<RotationSpeed>();

                rotation.Value += rotationSpeed.Value * dt;
                rotation.Value %= 360;
            }
        }
    }
}