namespace Code.Common
{
    public class MovementFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new ClickToMoveSystem(), Order.Update);
            GameSys.Add(new MoveToTargetSystem(), Order.Update);
            GameSys.Add(new LerpToTargetSystem(), Order.Update);
            GameSys.Add(new MoveAlongDirectionSystem(), Order.Update);
            GameSys.Add(new RotationSystem(), Order.Update);
        }
    }
}