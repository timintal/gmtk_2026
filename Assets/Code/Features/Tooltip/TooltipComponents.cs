using System;
using FFS.Libraries.StaticEcs;

namespace Code.Features.Tooltip
{
    public enum TooltipType : byte
    {
        Text = 0,
    }

    [Serializable]
    public struct Tooltip : IComponent
    {
        public TooltipType Type;
        public string Text;
    }

    public struct TooltipSettings : IResource
    {
        public bool Enabled;
    }
}
