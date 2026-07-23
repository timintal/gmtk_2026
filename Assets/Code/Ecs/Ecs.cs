using FFS.Libraries.StaticEcs;

public struct WT : IWorldType { }

public abstract class W : World<WT> { }

public struct GameSystems : ISystemsType { }

public abstract class GameSys : W.Systems<GameSystems> { }

public struct FixedGameSystems : ISystemsType { }

public abstract class FixedSys : W.Systems<FixedGameSystems> { }
