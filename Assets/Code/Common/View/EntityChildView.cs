using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    public class EntityChildView : MonoBehaviour
    {
        protected EntityGID _entity;
        
        public virtual void Bind(W.Entity entity)
        {
            _entity = entity;
        }
        
        public virtual void Unbind()
        {
            _entity = default;
        }

        public bool TryGetBoundEntity(out W.Entity entity)
        {
            return _entity.TryUnpack(out entity);
        }
    }
}