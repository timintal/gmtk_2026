using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.Fx
{
    public struct FxPrefab : IComponent { public GameObject Value; }
    public struct OneShotParticlesRequest : IComponent { public ParticleSystem prefab;}
}