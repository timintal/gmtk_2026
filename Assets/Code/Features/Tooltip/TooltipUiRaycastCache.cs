using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine.EventSystems;

namespace Code.Features.Tooltip
{
    public sealed class TooltipUiRaycastCache : IResource
    {
        public readonly List<RaycastResult> Results = new();
    }
}
