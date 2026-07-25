using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

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
            ListPool<EntityChildView>.Get(out var newChilds);
            GetComponentsInChildren(newChilds);
            foreach (var child in newChilds)
            {
                if (!_children.Contains(child))
                {
                    _children.Add(child);
                }
            }
            ListPool<EntityChildView>.Release(newChilds);
            for (int i = _children.Count - 1; i >= 0; i--)  
            {
                if (_children[i] == null)
                    _children.RemoveAt(i);
            }
        }
    }
}