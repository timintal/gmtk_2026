using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View.ChildViews
{
    public class SpriteLinkChildView : EntityChildView
    {
        [SerializeField] SpriteRenderer _spriteRenderer;
        
        public override void Bind(World<WT>.Entity entity)
        {
            base.Bind(entity);
            entity.Set(new SpriteLink() { Value = _spriteRenderer });
        }
        
        public override void Unbind()
        {
            if (_entity.TryUnpack<WT>(out var entity))
                entity.Delete<SpriteLink>();
            
            base.Unbind();
        }
        
        private void OnValidate()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            var parent = GetComponentInParent<EntityView>();
            if (parent)
            {
                parent.TryRegisterChild(this);
            }
        }
    }
}