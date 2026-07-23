using System.Collections.Generic;

namespace Code.Features.Stats
{
    // Shared per-stat-type scratch buffer used to detect real CurrentValue changes across a frame.
    // ResetStatSystem<T> stores each stat's pre-reset value here; ApplyStatModifierSystem<T, _>
    // reads it back after all modifiers are applied to decide whether to mark the stat as Changed.
    // Keyed by the entity's raw slot id, which is stable for the duration of a single frame.
    internal static class StatChangeBuffer<T> where T : struct, IStat
    {
        public static readonly Dictionary<uint, float> Previous = new();
    }
}
