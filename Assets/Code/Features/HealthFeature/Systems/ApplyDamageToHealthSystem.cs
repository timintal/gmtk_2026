using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;

namespace Code.Features.HealthFeature.Systems
{
    public sealed class ApplyDamageToHealthSystem : ISystem
    {
        private readonly List<W.Entity> _damages = new();

        public void Update()
        {
            _damages.Clear();
            foreach (var damageEntity in W.Query<All<Damage>, None<Destroyed>>().Entities())
                _damages.Add(damageEntity);

            foreach (var damageEntity in _damages)
            {
                ref readonly var damage = ref damageEntity.Read<Damage>()!;

                if (damage.Amount > 0
                    && damage.Target.TryUnpack<WT>(out var target)
                    && !target.Has<Destroyed>()
                    && target.Has<Health>())
                {
                    target.Mut<Health>().Value -= damage.Amount;
                }

                damageEntity.Set<Destroyed>();
            }
        }
    }
}
