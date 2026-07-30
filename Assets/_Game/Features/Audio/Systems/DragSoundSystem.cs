using Code.Common.Audio;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Audio.Systems
{
    public sealed class DragSoundSystem : ISystem
    {
        private EventReceiver<WT, DragStarted> _started;
        private EventReceiver<WT, DragEnded> _ended;
        private bool _initialized;
        
        private readonly SfxGenericAudioSource _sfxGenericAudioSource;

        public DragSoundSystem(SfxGenericAudioSource sfxGenericAudioSource)
        {
            _sfxGenericAudioSource = sfxGenericAudioSource;
        }

        public void Init()
        {
            _started = W.RegisterEventReceiver<DragStarted>();
            _ended = W.RegisterEventReceiver<DragEnded>();
            _initialized = true;
        }

        public void Update()
        {
            foreach (var dragEvent in _started)
            {
                if (TryGetDragSound(dragEvent.Value.Draggable, out var sound))
                {
                    Play(sound.Pickup);
                }
            }

            foreach (var dragEvent in _ended)
            {
                ref readonly var ended = ref dragEvent.Value;
                if (TryGetDragSound(ended.Draggable, out var sound))
                {
                    Play(ended.Accepted ? sound.Drop : sound.Reject);
                }
            }
        }

        public void Destroy()
        {
            if (!_initialized || W.Status != WorldStatus.Initialized)
            {
                return;
            }

            W.DeleteEventReceiver(ref _started);
            W.DeleteEventReceiver(ref _ended);
            _initialized = false;
        }

        private static bool TryGetDragSound(EntityGID gid, out DragSound sound)
        {
            if (gid.TryUnpack<WT>(out var entity) && entity.Has<DragSound>())
            {
                sound = entity.Read<DragSound>();
                return true;
            }

            sound = default;
            return false;
        }

        private void Play(SoundType type)
        {
            if (type == SoundType.None)
            {
                return;
            }

            _sfxGenericAudioSource.Play(type);
        }
    }
}
