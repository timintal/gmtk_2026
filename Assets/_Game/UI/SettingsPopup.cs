using Code.Common.Audio;
using Code.Common.View;
using GameFlow.FSM;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SettingsPopup : ResourceMonoBehaviour<SettingsPopup>
{
    [SerializeField] Toggle _musicToggle;
    [SerializeField] Toggle _sfxToggle;
    [SerializeField] Button _closeButton;
    
    [Inject] internal MusicGenericAudioSource _musicGenericAudioSource;
    [Inject] internal SfxGenericAudioSource _sfxGenericAudioSource;
    [Inject] internal GameFSM _fsm;

    void Start()
    {
        _musicToggle.isOn = Mathf.Approximately(PlayerPrefs.GetFloat(MusicGenericAudioSource.MusicVolume, 0.2f), 0.2f);
        _sfxToggle.isOn = Mathf.Approximately(PlayerPrefs.GetFloat(SfxGenericAudioSource.Sfxvolume, 1), 1);
    }

    private void OnEnable()
    {
        _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        _sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        _closeButton.onClick.AddListener(ClosePopup);
    }
    
    
    private void OnMusicToggleChanged(bool isOn)
    {
        float volume = isOn ? 0.2f : 0f;
        _musicGenericAudioSource.SetVolume(volume);
    }
    
    private void OnSfxToggleChanged(bool isOn)
    {
        float volume = isOn ? 1f : 0f;
        _sfxGenericAudioSource.SetVolume(volume);
    }
    
    private void ClosePopup()
    {
        _fsm.PopState();
    }
    
    private void OnDisable()
    {
        _musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
        _sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);
        _closeButton.onClick.RemoveListener(ClosePopup);
    }
}
