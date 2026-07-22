using FFS.Libraries.StaticEcs;

namespace Code.Common
{
    public class SyncPositionFromTransformSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in W.Query<All<TransformLink, Position, SyncPositionFromTransform>>().Entities())
            {
                ref var pos = ref entity.Ref<Position>();
                var t = entity.Read<TransformLink>().Value;
                pos.Value = ViewTransformUtility.ReadPosition(t);
            }
        }
    }
}