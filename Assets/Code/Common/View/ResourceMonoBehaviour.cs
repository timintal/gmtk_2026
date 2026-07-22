using FFS.Libraries.StaticEcs;
using UnityEngine;
namespace Code.Common.View
{
    public abstract class ResourceMonoBehaviour : MonoBehaviour
    {
        public abstract void RegisterResource();
        public abstract void UnregisterResource();
    }

    public abstract class ResourceMonoBehaviour<T> : ResourceMonoBehaviour, IResource where T : class, IResource
    {
        private T Resource => this as T;

        private bool _registered;

        void Awake()
        {
            RegisterResource();
        }
    
        void OnDestroy()
        {
            UnregisterResource();
        }

        public override void RegisterResource()
        {
            if (_registered) return;
            if (W.Status != WorldStatus.Initialized) return;

            W.SetResource(Resource);
            _registered = true;
        }

        public override void UnregisterResource()
        {
            if (!_registered) return;
            if (W.Status != WorldStatus.Initialized) return;

            W.RemoveResource<T>();
            _registered = false;
        }
    }
}
