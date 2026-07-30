using System;
using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Common.View;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using GameFlow.FSM;
using UnityEngine;
using VContainer.Unity;

[DefaultExecutionOrder(-10000)]
public class EcsBootstrap : ITickable, IFixedTickable, IInitializable, IDisposable
{
    private readonly ISystemFactory _systems;
    
    private GameFSM _fsm;

    public EcsBootstrap(ISystemFactory systems, GameFSM fsm)
    {
        _systems = systems;
        _fsm = fsm;
    }
    
    public void Initialize()
    {
        BootstrapEcs();
    }

    public void Dispose()
    {
        TeardownEcs();
    }

    public void Tick()
    {
        _fsm.Tick();

        UpdateDeltaTime(Time.deltaTime, Time.unscaledDeltaTime);

        GameSys.Update();
        W.Tick();
    }

    public void FixedTick()
    {
        UpdateDeltaTime(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);

        FixedSys.Update();
    }

    private static void UpdateDeltaTime(float deltaTime, float unscaledDeltaTime)
    {
        var paused = W.Query<All<Pause>>().EntitiesCount() > 0;

        ref var dt = ref W.GetResource<DeltaTime>();
        dt.Value = paused ? 0 : deltaTime;
        dt.Unscaled = unscaledDeltaTime;
    }



    private void BootstrapEcs()
    {
        W.Create(WorldConfig.Default());
        W.Types().RegisterAll();
        UnityEventTypes.Register<WT>();

        GameSys.Create();
        FixedSys.Create();

        _systems.CreateFeature<GameplayFeature>();

        EcsDebug<WT>.AddWorld<GameSystems>();

        W.Initialize();

        SetUpResources();

        GameSys.Initialize();
        FixedSys.Initialize();
    }

    private void SetUpResources()
    {
        W.SetResource(new DeltaTime());
        W.SetResource(new ColliderRegistry(128));
        W.SetResource(new TooltipPointerState());
        W.SetResource(new TooltipUiRaycastCache());
        // W.SetResource(new PoolService());
    }

    private void TeardownEcs()
    {
        FixedSys.Destroy();
        GameSys.Destroy();
        EcsDebug<WT>.RemoveWorld();
        W.Destroy();
    }

}