using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View.ChildViews
{

    public class Collider2DChildView : EntityChildView
    {
        [SerializeField] Collider2D _collider2D;
        
        public override void Bind(World<WT>.Entity entity)
        {
            base.Bind(entity);
            entity.Set(new ColliderLink()
            {
                Value = _collider2D
            });
        }

        public override void Unbind()
        {
            if (_entity.TryUnpack<WT>(out var entity))
                entity.Delete<ColliderLink>();

            base.Unbind();
        }

        private void OnValidate()
        {
            if (_collider2D == null)
                _collider2D = GetComponent<Collider2D>();
            
            var parent = GetComponentInParent<EntityView>();
            if (parent)
            {
                parent.TryRegisterChild(this);
            }
        }

    }
}