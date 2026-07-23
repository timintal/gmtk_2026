using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Features.PlayerControls
{
    public class InputSystem : ISystem
    {
        private DefaultInputActions _actions;
        private InputAction _move;

        public void Init()
        {
            _actions = new DefaultInputActions();
            _move = _actions.Player.Move;
            _move.Enable();
        }

        public void Update()
        {
            W.Query<All<HasInput>>().BatchDelete<HasInput>();

            var move = _move.ReadValue<Vector2>();
            if (move == Vector2.zero)
                return;

            foreach (var e in W.Query<All<Player>>().Entities())
            {
                e.Set(new Direction { Value = move.normalized });
                e.Set<HasInput>();
            }
        }

        public void Destroy()
        {
            _move?.Disable();
            _actions?.Dispose();
            _move = null;
            _actions = null;
        }
    }
}
