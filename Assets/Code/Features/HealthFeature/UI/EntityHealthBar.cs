using Code.Common;
using Code.Common.View;
using Code.Common.View.UI;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Features.HealthFeature.UI
{
    public class EntityHealthBar : EntityChildView
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField,OnValueChanged(nameof(UpdateOffset))] Vector3 _offset = new Vector3(0, -1, 0);

        EntityGID _target;

        int _currentHealth = 0;
        int _currentMaxHealth = 0;

        public override void Bind(W.Entity entity)
        {
            base.Bind(entity);

            if (entity.Has<W.Link<Target>>() && 
                entity.Read<W.Link<Target>>().Value.TryUnpack<WT>(out var target) && 
                target.Has<Health>() && target.Has<MaxHealth>() && target.Has<TransformLink>())
            {
                _target = target;
                
                var targetTransform = target.Read<TransformLink>().Value;
                
                entity.Set(new WorldSpaceUiFollowLink()
                {
                    UiElement = transform as RectTransform,
                    Target = targetTransform,
                    Offset = _offset
                });
            }
        }

        public override void Unbind()
        {
            _target = default;
            
            if (_entity.TryUnpack<WT>(out var entity))
            {
                entity.Delete<WorldSpaceUiFollowLink>();
            }
            base.Unbind();
        }
        private void UpdateOffset()
        {
            if (_entity.TryUnpack<WT>(out var entity))
            {
                if (entity.Has<WorldSpaceUiFollowLink>())
                {
                    ref var followLink = ref entity.Mut<WorldSpaceUiFollowLink>();
                    followLink.Offset = _offset;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target.TryUnpack<WT>(out var target) && target.Has<Health>() && target.Has<MaxHealth>())
            {
                ref var health = ref target.Mut<Health>();
                ref var maxHealth = ref target.Mut<MaxHealth>();

                if (health.Value != _currentHealth || maxHealth.Value != _currentMaxHealth)
                {
                    _progressBar.SetState(health.Value, maxHealth.Value);
                    _currentHealth = health.Value;
                    _currentMaxHealth = maxHealth.Value;
                }
            }
        }
    }
}