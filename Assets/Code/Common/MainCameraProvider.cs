using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-9000)]
    public sealed class MainCameraProvider : MonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            if (W.Status != WorldStatus.Initialized) return;
            W.SetResource(new MainCamera { Value = _camera });
        }

        private void OnDisable()
        {
            if (W.Status != WorldStatus.Initialized) return;
            if (!W.HasResource<MainCamera>()) return;
            if (ReferenceEquals(W.GetResource<MainCamera>().Value, _camera))
            {
                W.RemoveResource<MainCamera>();
            }
        }
    }
}
