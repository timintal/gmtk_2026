using _Game.Infrastructure.ECS;
using Code.Ecs;
using Code.Features.Movement;

namespace Code.Common
{
    public class MovementFeature : IFeature
    {
        public MovementFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<ClickToMoveSystem>());
            GameSys.Add(systems.Create<MoveToTargetSystem>());
            GameSys.Add(systems.Create<LerpToTargetSystem>());
            GameSys.Add(systems.Create<MoveAlongDirectionSystem>());
            GameSys.Add(systems.Create<RotationSystem>());
        }
    }
}