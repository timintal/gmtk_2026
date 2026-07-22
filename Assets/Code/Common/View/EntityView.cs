using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Common.View
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] bool _syncWithEntityPosition = true;
        [SerializeField, ShowIf("_syncWithEntityPosition")] float _syncDamping = 0f;

        [SerializeField] List<EntityChildView> _children = new();

        protected EntityGID _entity;

        public void Bind(W.Entity entity)
        {
            _entity = entity;

            entity.Set(new TransformLink
            {
                Value = transform
            });
            if (_syncWithEntityPosition)
            {
                entity.Set(new SyncViewPosition
                {
                    Damping = _syncDamping
                });
            }

            foreach (var child in _children)
                child.Bind(entity);
        }

        public void Unbind()
        {
            if (TryGetBoundEntity(out var entity))
            {
                entity.Delete<TransformLink>();
                if (_syncWithEntityPosition)
                {
                    entity.Delete<SyncViewPosition>();
                }
            }

            foreach (var child in _children)
                child.Unbind();

            _entity = default;
        }

        public bool TryGetBoundEntity(out W.Entity entity)
        {
            return _entity.TryUnpack(out entity);
        }

        public void TryRegisterChild(EntityChildView child)
        {
            if (_children.Contains(child)) 
                return;
            
            _children.Add(child);
        }

        [Button]
        public void GatherChildren()
        {
            _children.Clear();
            GetComponentsInChildren(_children);
        }
    }
}