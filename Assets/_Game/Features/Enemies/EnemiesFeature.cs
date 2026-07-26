using Code.Common;
using Code.Features.DragAndDrop;

namespace _Game.Features.Enemies
{
    public static class EnemiesFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new UpdateEnemyCountdownSystem());
            GameSys.Add(new UpdateAttackViewSystem());
            GameSys.Add(new DiceToEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart);
            GameSys.Add(new EvenEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart + 1);
            GameSys.Add(new OddEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart + 2);
            GameSys.Add(new BiggerEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart + 3);
            GameSys.Add(new SmallerEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart + 4);
            
            
            GameSys.Add(new ShowDragRejectReasonSystem(), DragAndDropSystemOrder.Resolve + 1);
            GameSys.Add(new DiceDragAcceptSystem(), DragAndDropSystemOrder.Resolve + 2);
            
            GameSys.Add(new CheckExactCountdownSystem(), Order.PreCleanup);
        }
    }
}