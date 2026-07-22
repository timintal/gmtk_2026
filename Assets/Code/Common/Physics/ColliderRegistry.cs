using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    /// <summary>
    /// Reverse index mapping a Unity collider's <see cref="EntityId"/> to the ECS entity that owns it.
    /// Populated automatically by <see cref="ColliderLink"/>'s OnAdd/OnDelete hooks, so physics
    /// events (which only hand you the other <see cref="Collider2D"/>) can be resolved back to an entity.
    /// </summary>
    public struct ColliderRegistry : IResource
    {
        private Dictionary<EntityId, EntityGID> _map;

        public ColliderRegistry(int capacity) => _map = new Dictionary<EntityId, EntityGID>(capacity);

        public void Register(EntityId colliderId, EntityGID entity) => _map[colliderId] = entity;

        public void Unregister(EntityId colliderId) => _map.Remove(colliderId);

        public bool TryResolve(EntityId colliderId, out EntityGID entity) => _map.TryGetValue(colliderId, out entity);

        public bool TryResolve(Collider2D collider, out EntityGID entity)
        {
            if (!ReferenceEquals(collider, null))
                return _map.TryGetValue(collider.GetEntityId(), out entity);

            entity = default;
            return false;
        }
    }
}
