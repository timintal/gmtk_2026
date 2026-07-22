using FFS.Libraries.StaticEcs;

namespace Code.Common.Cooldown
{
    public class CooldownTickSystem<T> : ISystem where T : struct, ICooldown
    {
        public void Update()
        {
            foreach (var entity in W.Query<All<T>>().Entities())
            {
                ref var cooldown = ref entity.Ref<T>();
                cooldown.Value -= W.GetResource<DeltaTime>().Value;
                if (cooldown.Value <= 0)
                {
                    entity.Delete<T>();
                }
            }
        }
    }
}