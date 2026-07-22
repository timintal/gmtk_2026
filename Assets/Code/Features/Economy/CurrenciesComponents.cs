using FFS.Libraries.StaticEcs;

namespace Code.Features.Economy
{
    public struct Gold : IComponent, ITrackableAdded, ITrackableChanged
    {
        public int Value;
    }
}
