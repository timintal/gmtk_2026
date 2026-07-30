using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Features.Movement
{
    public class ClickToMoveSystem : ISystem
    {
        private readonly MainCamera _camera;
        public ClickToMoveSystem(MainCamera camera)
        {
            _camera = camera;

        }
        public void Update()
        {
            if (W.Query<All<ClickToMoveListener, Position>>().EntitiesCount() == 0)
                return;
            
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            if (_camera.Value == null) return;

            var camera = _camera.Value;
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
