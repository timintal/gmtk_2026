using FFS.Libraries.StaticEcs;

namespace Code.Common.Fx
{
    public class PlayOneShotParticlesSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in W.Query<All<OneShotParticlesRequest, Position>>().Entities())
            {
                var request = entity.Read<OneShotParticlesRequest>();
                var prefab = request.prefab;
                var poolService = W.GetResource<PoolService>();
                var fxInstance = poolService.GetParticleFx(prefab);
                fxInstance.transform.position = entity.Read<Position>().Value;
                fxInstance.Play();
                entity.Set<Destroyed>();
            }
        }
    }
}