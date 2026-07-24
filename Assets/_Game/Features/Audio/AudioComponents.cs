using System;
using Code.Common.Audio;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Audio
{
    /// <summary>
    /// Selects which sounds a draggable plays across its drag lifecycle.
    /// Any field left as <see cref="SoundType.None"/> is silent.
    /// </summary>
    [Serializable]
    public struct DragSound : IComponent
    {
        public SoundType Pickup;
        public SoundType Drop;
        public SoundType Reject;
    }
}
