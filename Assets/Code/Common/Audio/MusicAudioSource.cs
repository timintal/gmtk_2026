namespace Code.Common.Audio
{
    public class MusicAudioSource : AudioSourceResource<MusicAudioSource>
    {
        public const string MusicVolume = "MusicVolume";
        protected override string PrefsKey => MusicVolume;
    }
}