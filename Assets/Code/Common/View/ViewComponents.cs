using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using UnityEngine;

namespace Code.Common.View
{
    public struct ViewPrefab : IComponent { public EntityView Prefab; }
    
    public partial struct ViewLink : IComponent { public EntityView View; }
    
    public struct NeedInitializeView : ITag { }
    public partial struct NeedBindView : ITag { }
    public struct NeedInitRigidbody : ITag { }
    
    [StaticEcsEditorGroup("Physics", "00FFFF")]
    public struct ColliderLink : IComponent
    {
        public Collider2D Value;

        public void OnAdd<TWorld>(World<TWorld>.Entity self) where TWorld : struct, IWorldType
        {
            if (!ReferenceEquals(Value, null))
                World<TWorld>.GetResource<ColliderRegistry>().Register(Value.GetEntityId(), self.GID);
        }

        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason) where TWorld : struct, IWorldType
        {
            if (reason == HookReason.WorldDestroy) return;

            // Use ReferenceEquals, not ==: on entity destruction the collider may be a Unity fake-null,
            // yet GetEntityId() still returns the original id so we can remove the stale entry.
            if (!ReferenceEquals(Value, null))
                World<TWorld>.GetResource<ColliderRegistry>().Unregister(Value.GetEntityId());
        }
    }
    [StaticEcsEditorGroup("Physics", "00FFFF")]
    public partial struct RigidbodyLink : IComponent { public Rigidbody2D Value; }
    public partial struct SpriteLink : IComponent { public SpriteRenderer Value; }
}