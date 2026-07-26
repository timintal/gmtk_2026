namespace Code.Common.Audio
{
    public class MusicAudioSource : AudioSourceResource<MusicAudioSource>
    {
        public const string MusicVolume = "MusicVolume";
        protected override string PrefsKey => MusicVolume;
        protected override float DefaultVolume => 0.2f;
    }
}