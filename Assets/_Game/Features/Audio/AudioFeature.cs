using _Game.Features.Audio.Systems;
using _Game.Infrastructure.ECS;
using Code.Ecs;
using Code.Features.DragAndDrop;

namespace _Game.Features.Audio
{
    public class AudioFeature : IFeature
    {
        public AudioFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<DragSoundSystem>(), DragAndDropSystemOrder.Resolve + 2);
        }
    }
}
