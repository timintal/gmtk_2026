using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Common.View.ChildViews
{
    public class RigidbodyChildView : EntityChildView
    {
        [SerializeField] Rigidbody2D _rigidbody2D;
        [SerializeField] bool _syncRigidbodyPosition = true;
        [SerializeField, ShowIf("_syncRigidbodyPosition")] float _rigidbodyDamping = 0f;
        [SerializeField] private bool _sendTriggerEvents;
        [SerializeField] private bool _sendCollisionEvents;
        [SerializeField] private bool _initRigidbodyOnInstantiate;
        
        public override void Bind(World<WT>.Entity entity)
        {
            base.Bind(entity);
            entity.Set(new RigidbodyLink()
            {
                Value = _rigidbody2D
            });
            if (_syncRigidbodyPosition)
            {
                entity.Set(new SyncPhysicsPosition() { Damping = _rigidbodyDamping });
            }
            if (_initRigidbodyOnInstantiate)
            {
                entity.Set<NeedInitRigidbody>();
            }
        }
        
        public override void Unbind()
        {
            if (_entity.TryUnpack<WT>(out var entity))
                entity.Delete<RigidbodyLink>();
            
            base.Unbind();
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (_sendTriggerEvents && _entity.TryUnpack<WT>(out var entity))
            {
                var newEntity = W.NewEntity<Default>();
                newEntity.Set(new TriggerEnter2D()
                {
                    Value = other
                });
                newEntity.Set(new W.Link<Owner>(_entity));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_sendTriggerEvents && _entity.TryUnpack<WT>(out var entity))
            {
                var newEntity = W.NewEntity<Default>();
                newEntity.Set(new TriggerExit2D()
                {
                    Value = other
                });
                newEntity.Set(new W.Link<Owner>(_entity));
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_sendCollisionEvents && _entity.TryUnpack<WT>(out var entity))
            {
                var newEntity = W.NewEntity<Default>();
                newEntity.Set(new CollisionEnter2D()
                {
                    Value = collision
                });
                newEntity.Set(new W.Link<Owner>(_entity));
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_sendCollisionEvents && _entity.TryUnpack<WT>(out var entity))
            {
                var newEntity = W.NewEntity<Default>();
                newEntity.Set(new CollisionExit2D()
                {
                    Value = collision
                });
                newEntity.Set(new W.Link<Owner>(_entity));
            }
        }

        private void OnValidate()
        {
            if (_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();

            var parent = GetComponentInParent<EntityView>();
            if (parent)
            {
                parent.TryRegisterChild(this);
            }
        }
    }
}