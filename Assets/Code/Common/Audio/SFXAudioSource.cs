namespace Code.Common.Audio
{
    public class SFXAudioSource : AudioSourceResource<SFXAudioSource>
    {
        public const string Sfxvolume = "SFXVolume";
        protected override string PrefsKey => Sfxvolume;
        protected override float DefaultVolume => 1f;
    }
}
