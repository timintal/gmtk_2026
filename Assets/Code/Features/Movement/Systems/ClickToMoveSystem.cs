using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Common
{
    public class ClickToMoveSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<ClickToMoveListener, Position>>().EntitiesCount() == 0)
                return;
            
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            if (!W.HasResource<MainCamera>()) return;

            var camera = W.GetResource<MainCamera>().Value;
            var screen = pointer.position.ReadValue();
            var world3 = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
            var target = new Vector2(world3.x, world3.y);

            foreach (var e in W.Query<All<ClickToMoveListener, Position>>().Entities())
            {
                e.Set(new MoveTarget { Value = target });
            }
        }
    }
}
