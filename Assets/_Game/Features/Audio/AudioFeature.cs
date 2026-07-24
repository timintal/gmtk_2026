using _Game.Features.Audio.Systems;
using Code.Features.DragAndDrop;

namespace _Game.Features.Audio
{
    public static class AudioFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new DragSoundSystem(), DragAndDropSystemOrder.Resolve + 2);
        }
    }
}
