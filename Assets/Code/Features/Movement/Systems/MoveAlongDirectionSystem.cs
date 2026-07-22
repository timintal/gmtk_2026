using FFS.Libraries.StaticEcs;

namespace Code.Common
{
    public class MoveAlongDirectionSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<All<Position, Speed, Direction, MoveAlongDirection>>().Entities())
            {
                ref var pos = ref e.Ref<Position>();
                ref readonly var speed = ref e.Read<Speed>();
                ref readonly var direction = ref e.Read<Direction>();

                var delta = direction.Value.normalized * speed.Value * dt;
                pos.Value += delta;
            }
        }
    }
}