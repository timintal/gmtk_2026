using Code.Common;
using FFS.Libraries.StaticEcs;

namespace Code.Features.Tooltip
{
    public static class TooltipFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new TooltipHoverSystem(), (short)(Order.Input + 10));
        }

        public static void SetEnabled(bool enabled)
        {
            if (W.Status != WorldStatus.Initialized || !W.HasResource<TooltipSettings>())
            {
                return;
            }

            ref var settings = ref W.GetResource<TooltipSettings>();
            settings.Enabled = enabled;
        }

        public static bool IsEnabled()
        {
            return W.Status == WorldStatus.Initialized
                   && W.HasResource<TooltipSettings>()
                   && W.GetResource<TooltipSettings>().Enabled;
        }
    }
}
