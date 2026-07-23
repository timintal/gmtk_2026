using Code.Features.DragAndDrop;

namespace _Game.Features.Enemies
{
    public static class EnemiesFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new UpdateEnemyCountdownSystem());
            GameSys.Add(new DiceToEnemyContainerValidationSystem(), DragAndDropSystemOrder.UserValidationStart);
            GameSys.Add(new DiceDragAcceptSystem(), DragAndDropSystemOrder.Resolve + 1);
        }
    }
}