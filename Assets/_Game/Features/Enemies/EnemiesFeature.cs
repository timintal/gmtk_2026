using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;
using Code.Features.DragAndDrop;

namespace _Game.Features.Enemies
{
    public class EnemiesFeature : IFeature
    {
        public EnemiesFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<UpdateEnemyCountdownSystem>());
            GameSys.Add(systems.Create<UpdateAttackViewSystem>());
            GameSys.Add(systems.Create<DiceToEnemyContainerValidationSystem>(), DragAndDropSystemOrder.UserValidationStart);
            GameSys.Add(systems.Create<EvenEnemyContainerValidationSystem>(), DragAndDropSystemOrder.UserValidationStart + 1);
            GameSys.Add(systems.Create<OddEnemyContainerValidationSystem>(), DragAndDropSystemOrder.UserValidationStart + 2);
            GameSys.Add(systems.Create<BiggerEnemyContainerValidationSystem>(), DragAndDropSystemOrder.UserValidationStart + 3);
            GameSys.Add(systems.Create<SmallerEnemyContainerValidationSystem>(), DragAndDropSystemOrder.UserValidationStart + 4);
            
            
            GameSys.Add(systems.Create<ShowDragRejectReasonSystem>(), DragAndDropSystemOrder.Resolve + 1);
            GameSys.Add(systems.Create<DiceDragAcceptSystem>(), DragAndDropSystemOrder.Resolve + 2);
            
            GameSys.Add(systems.Create<CheckExactCountdownSystem>(), Order.PreCleanup);
        }
    }
}