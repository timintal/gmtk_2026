using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using VContainer;

namespace _Game.Features.Visuals
{
    [ExecuteAlways]
    public class SnapToScreenAnchor : MonoBehaviour
    {
        public enum OffsetSpace
        {
            WorldUnits,
            Pixels
        }

        public enum SnapMode
        {
            OnEnableOnly,
            OnScreenChange,
            EveryFrame
        }

        [Tooltip("Viewport anchor: (0,0) is the bottom-left screen corner, (1,1) is the top-right one.")]
        public Vector2 anchor = new(0.5f, 0.5f);

        [Tooltip("Shift from the anchor, applied along the camera's right/up axes.")]
        public Vector2 offset = Vector2.zero;

        public OffsetSpace offsetSpace = OffsetSpace.WorldUnits;
        public SnapMode snapMode = SnapMode.OnScreenChange;

        public bool snapX = true;
        public bool snapY = true;

        [Inject] internal MainCamera _mainCamera;

        private Vector2Int _lastScreenSize;
        private bool _snapped;

        private void OnEnable()
        {
            _snapped = false;
            Snap();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Snap();
            }
        }
#endif

        private void Update()
        {
            switch (snapMode)
            {
                case SnapMode.EveryFrame:
                    Snap();
                    break;
                case SnapMode.OnScreenChange when !_snapped || ScreenSizeChanged():
                    Snap();
                    break;
                case SnapMode.OnEnableOnly when !_snapped:
                    Snap();
                    break;
            }
        }

        private bool ScreenSizeChanged() => _lastScreenSize != new Vector2Int(Screen.width, Screen.height);

        [ContextMenu("Snap")]
        public void Snap()
        {
            var camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            var camTransform = camera.transform;
            var position = transform.position;

            // Keep the object on its current plane relative to the camera so perspective cameras stay correct.
            var depth = Vector3.Dot(position - camTransform.position, camTransform.forward);

            var viewportAnchor = anchor;
            if (offsetSpace == OffsetSpace.Pixels)
            {
                var pixelRect = camera.pixelRect;
                if (pixelRect.width > 0f && pixelRect.height > 0f)
                {
                    viewportAnchor += new Vector2(offset.x / pixelRect.width, offset.y / pixelRect.height);
                }
            }

            var target = camera.ViewportToWorldPoint(new Vector3(viewportAnchor.x, viewportAnchor.y, depth));
            if (offsetSpace == OffsetSpace.WorldUnits)
            {
                target += camTransform.right * offset.x + camTransform.up * offset.y;
            }

            transform.position = new Vector3(
                snapX ? target.x : position.x,
                snapY ? target.y : position.y,
                position.z);

            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _snapped = true;
        }

        private Camera ResolveCamera()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                return _mainCamera.Value;
#if UNITY_EDITOR
            }

            return Camera.main;
#endif
        }
    }
}