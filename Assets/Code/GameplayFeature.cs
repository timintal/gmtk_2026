using _Game.Features.Audio;
using _Game.Features.Blessings;
using _Game.Features.Dice;
using _Game.Features.Enemies;
using _Game.Features.PlayerControls;
using _Game.Features.Run;
using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Common.Fx;
using Code.Common.View;
using Code.Ecs;
using Code.Features.DragAndDrop;
using Code.Features.EnergyFeature;
using Code.Features.HealthFeature;
using Code.Features.Stats;
using Code.Features.Tooltip;

public class GameplayFeature : IFeature
{
    public GameplayFeature(ISystemFactory systems)
    {
        systems.CreateFeature<CommonSystems>();
        systems.CreateFeature<DragAndDropFeature>();
        systems.CreateFeature<TooltipFeature>();
        systems.CreateFeature<EnergyFeature>();
        systems.CreateFeature<HealthFeature>();
        systems.CreateFeature<StatsFeature>();
        systems.CreateFeature<MovementFeature>();
        systems.CreateFeature<FxFeature>();
        
        systems.CreateFeature<ViewFeature>();
        
        systems.CreateFeature<PlayerControlsFeature>();
        systems.CreateFeature<DiceFeature>();
        systems.CreateFeature<EnemiesFeature>();
        systems.CreateFeature<AudioFeature>();
        systems.CreateFeature<BlessingsFeature>();
        systems.CreateFeature<RunFeature>();
    }
}