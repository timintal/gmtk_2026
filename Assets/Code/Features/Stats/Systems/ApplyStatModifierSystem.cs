using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.Stats
{
    public abstract class ApplyStatModifierSystem<T1, T2> : ISystem
        where T1 : struct, IStat
        where T2 : struct, IStatModifier<T1>
    {
        private readonly List<ValidEntry> _valid = new();

        private struct ValidEntry
        {
            public W.Entity Modifier;
            public W.Entity Target;
        }

        public void Update()
        {
            _valid.Clear();
            foreach (var e in W.Query<All<T2,  W.Link<Target>>>().Entities())
            {
                ref var statModifier = ref e.Ref<T2>();
                var target = e.Read< W.Link<Target>>();
                if (!target.Value.TryUnpack<WT>(out var targetEntity) || !targetEntity.Has<T1>())
                    continue;

                ref var stat = ref targetEntity.Ref<T1>();
                stat.CurrentValue *= 1 + statModifier.Multiplicative;
                _valid.Add(new ValidEntry { Modifier = e, Target = targetEntity });
            }

            for (var i = 0; i < _valid.Count; i++)
            {
                var entry = _valid[i];
                ref var statModifier = ref entry.Modifier.Ref<T2>()!;
                ref var stat = ref entry.Target.Ref<T1>()!;
                stat.CurrentValue += statModifier.Additive;
            }

            // All modifiers are applied; CurrentValue now holds this frame's final value.
            // Compare against the pre-reset snapshot and only mark stats whose value actually changed.
            // Iterating every stat of this type (not just modified ones) also covers stats without
            // modifiers, whose value changes when BaseValue changes.
            var previous = StatChangeBuffer<T1>.Previous;
            foreach (var e in W.Query<All<T1>>().Entities())
            {
                if (!previous.TryGetValue(e.ID, out var oldValue))
                    continue;

                if (!Mathf.Approximately(oldValue, e.Read<T1>()!.CurrentValue))
                    e.Mut<T1>();
            }
        }
    }
}
