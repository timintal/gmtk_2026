using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    public sealed class WorldSpaceUiFollowSystem : ISystem
    {
        private readonly MainCamera _mainCamera;

        public WorldSpaceUiFollowSystem(MainCamera mainCamera)
        {
            _mainCamera = mainCamera;
        }
        
        public void Update()
        {
            var camera = _mainCamera.Value;

            foreach (var entity in W.Query<All<WorldSpaceUiFollowLink>>().Entities())
            {
                ref readonly var follow = ref entity.Read<WorldSpaceUiFollowLink>();
                var uiElement = follow.UiElement;
                var target = follow.Target;

                if (uiElement.parent is not RectTransform parentRect)
                {
                    continue;
                }

                var worldPosition = target.TransformPoint(follow.Offset);
                var screenPoint = camera.WorldToScreenPoint(worldPosition);
                if (screenPoint.z < 0f)
                {
                    continue;
                }

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect,
                        screenPoint,
                        camera ,
                        out var localPoint))
                {
                    continue;
                }

                uiElement.localPosition = localPoint;
            }
        }
    }
}
