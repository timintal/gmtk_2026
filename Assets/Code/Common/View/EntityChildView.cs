using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    public class EntityChildView : MonoBehaviour
    {
        protected EntityGID _entity;
        
        public W.Entity Entity
        {
            get
            {
                if (_entity.TryUnpack<WT>(out var entity))
                    return entity;
                else
                {
                    Debug.LogError($"EntityChildView {name} is not bound to an entity.");
                    return default;
                }
            }
        }

        public virtual void Bind(W.Entity entity)
        {
            _entity = entity;
        }
        
        public virtual void Unbind()
        {
            _entity = default;
        }
        
        public virtual void PostBind() { }
        public virtual void PostUnbind() { }

        public bool TryGetBoundEntity(out W.Entity entity)
        {
            return _entity.TryUnpack(out entity);
        }
    }
}