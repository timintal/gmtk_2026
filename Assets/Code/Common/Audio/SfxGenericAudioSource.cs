namespace Code.Common.Audio
{
    public class SfxGenericAudioSource : GenericAudioSource
    {
        public const string Sfxvolume = "SFXVolume";
        protected override string PrefsKey => Sfxvolume;
        protected override float DefaultVolume => 1f;
    }
}
