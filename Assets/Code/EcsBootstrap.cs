using _Game.Features.Audio;
using _Game.Features.Dice;
using _Game.Features.Enemies;
using _Game.Features.PlayerControls;
using Code.Common;
using Code.Common.Fx;
using Code.Common.View;
using Code.Configs;
using Code.Ecs;
using Code.Features.DragAndDrop;
using Code.Features.Economy;
using Code.Features.EnergyFeature;
using Code.Features.HealthFeature;
using Code.Features.Stats;
using Code.Features.Tooltip;
using Code.GameFlow;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using Libraries.GameFlow.FSM;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class EcsBootstrap : MonoBehaviour
{
    private bool _initialized;
    [SerializeField] VisualConfig _visualConfig;
    private GameFSM _fsm;

    private void Awake()
    {
        if (GetComponent<TooltipPointerInputBridge>() == null)
        {
            gameObject.AddComponent<TooltipPointerInputBridge>();
        }

        BootstrapEcs();
        BuildGameFSM();

        _initialized = true;
    }
    private void BuildGameFSM()
    {
        _fsm = new GameFSM(new CustomGameStateFactory());
        W.SetResource(new FSM()
        {
            Value = _fsm
        });

        _fsm.Push<MainGameState>();
    }
    
    private void Update()
    {
        if (!_initialized) return;

        _fsm.Tick();

        UpdateDeltaTime(Time.deltaTime, Time.unscaledDeltaTime);

        GameSys.Update();
        W.Tick();
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;

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

    private void OnDestroy()
    {
        TeardownEcs();
    }

    private void OnApplicationQuit()
    {
        TeardownEcs();
    }


    private void BootstrapEcs()
    {
        W.Create(WorldConfig.Default());
        W.Types().RegisterAll();
        UnityEventTypes.Register<WT>();

        GameSys.Create();
        FixedSys.Create();

        CommonSystems.AddToWorld();
        
        DragAndDropFeature.AddToWorld();
        TooltipFeature.AddToWorld();
        EconomyFeature.AddToWorld();
        EnergyFeature.AddToWorld();
        HealthFeature.AddToWorld();
        StatsFeature.AddToWorld();
        MovementFeature.AddToWorld();
        FxFeature.AddToWorld();
        PlayerControlsFeature.AddToWorld();
        DiceFeature.AddToWorld();
        EnemiesFeature.AddToWorld();
        AudioFeature.AddToWorld();

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
        W.SetResource(new TooltipSettings { Enabled = true });
        W.SetResource(new TooltipPointerState());
        W.SetResource(new TooltipUiRaycastCache());
        W.SetResource(_visualConfig);
        W.SetResource(new PoolService());
    }

    private void TeardownEcs()
    {
        if (!_initialized) return;
        _initialized = false;

        FixedSys.Destroy();
        GameSys.Destroy();
        EcsDebug<WT>.RemoveWorld();
        W.Destroy();
    }
}