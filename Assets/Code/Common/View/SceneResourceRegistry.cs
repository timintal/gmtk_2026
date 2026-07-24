using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Code.Common.View
{
    // Drop this on a single GameObject in the scene and wire up every resource the scene depends on,
    // including ones living on GameObjects that start disabled. It forces them into the world up front
    // so all scene dependencies are ready regardless of active state.
    //
    // Runs after EcsBootstrap (which creates the world at -10000) but before regular gameplay.
    [DefaultExecutionOrder(-9000)]
    public sealed class SceneResourceRegistry : MonoBehaviour
    {
        [SerializeField] private List<ResourceMonoBehaviour> _resources = new();

        private void Awake()
        {
            foreach (var resource in _resources)
            {
                if (resource == null) continue;
                resource.RegisterResource();
            }
        }
        
        private void OnDestroy()
        {
            foreach (var resource in _resources)
            {
                if (resource == null) continue;
                resource.UnregisterResource();
            }
        }
        
        [Button]
        void GatherAllResourcesOnScene()
        {
            _resources.Clear();
            var allResources = FindObjectsByType<ResourceMonoBehaviour>(FindObjectsInactive.Include);
            foreach (var resource in allResources)
            {
                if (!_resources.Contains(resource))
                {
                    _resources.Add(resource);
                }
            }
        }
    }
}
