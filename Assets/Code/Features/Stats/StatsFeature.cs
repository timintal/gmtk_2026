using Code.Common;

namespace Code.Features.Stats
{
    public static partial class StatsFeature
    {
        // Stats are recomputed every frame: reset CurrentValue to BaseValue, then apply modifiers.
        // Both run in PreUpdate so stats are finalized before gameplay reads them in Update.
        public const short StatResetOrder = Order.PreUpdate;
        public const short StatApplyOrder = (short)(Order.PreUpdate + 1);

        public static void AddToWorld()
        {
            RegisterGeneratedStatSystems();
        }

        // Implemented by StatSystemsGenerator for every IStat in this assembly.
        // If there are no stats, the call above is elided by the compiler.
        static partial void RegisterGeneratedStatSystems();
    }
}
