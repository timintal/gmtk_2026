using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using UnityEngine;

namespace Code.Common
{
    [StaticEcsEditorGroup("Transform", "00FFFF")]
    public partial struct TransformLink : IComponent { public Transform Value; }
    public struct ParentTransform : IComponent { public Transform Value; }
    
    [StaticEcsEditorGroup("Transform", "00FFFF")]
    public partial struct InitPositionFromTransform  : ITag { }

    [StaticEcsEditorGroup("Transform", "00FFFF")]
    public struct SyncViewPosition : IComponent { public float Damping; }
    [StaticEcsEditorGroup("Physics", "00FFFF")]
    public struct SyncPhysicsPosition : IComponent { public float Damping; }
    [StaticEcsEditorGroup("Transform", "00FFFF")]
    public struct SyncPositionFromTransform : ITag { }
    
    public struct SkipSyncViewPositionDamping : ITag{}

    public struct MainCamera : IResource { public Camera Value; }
    
    public struct Pause : ITag { }
    
    public struct Destroyed : ITag { }
    public struct AutoDestroy : IComponent { public float Delay; }
    
    public struct Activated : ITag { }
    
    public struct Target : ILinkType{}
    public struct Targets : ILinksType{}
    
    public struct Owner : ILinkType {}
    public struct Children : ILinksType {}
}
