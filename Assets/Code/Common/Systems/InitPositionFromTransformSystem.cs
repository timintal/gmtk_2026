using FFS.Libraries.StaticEcs;

namespace Code.Common
{
    public class InitPositionFromTransformSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<Position, TransformLink, InitPositionFromTransform>>().For((W.Entity e) =>
            {
                var transform = e.Read<TransformLink>().Value;
                if (transform == null) return;

                e.Set(new Position { Value = ViewTransformUtility.ReadPosition(transform) });
                e.Delete<InitPositionFromTransform>();
            });
        }
    }
}
