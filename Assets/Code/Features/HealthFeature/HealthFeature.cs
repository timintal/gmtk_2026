using Code.Common;
using Code.Features.HealthFeature.Systems;

namespace Code.Features.HealthFeature
{
    public class HealthFeature
    {
            public static void AddToWorld()
            {
                GameSys.Add(new InitHealthSystem(), Order.PostInit + 1);
                
                
                GameSys.Add(new ApplyDamageToHealthSystem(), Order.PreCleanup - 1);
                GameSys.Add(new KillZeroHealthEntities(), Order.PreCleanup);
            }
    }
}