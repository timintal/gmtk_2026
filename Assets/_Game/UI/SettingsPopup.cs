using System;
using Code.Common.Audio;
using Code.Common.View;
using Code.Ecs;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : ResourceMonoBehaviour<SettingsPopup>
{
    [SerializeField] Toggle _musicToggle;
    [SerializeField] Toggle _sfxToggle;
    [SerializeField] Button _closeButton;
    
    void Start()
    {
        _musicToggle.isOn = Mathf.Approximately(PlayerPrefs.GetFloat(MusicAudioSource.MusicVolume, 0.2f), 0.2f);
        _sfxToggle.isOn = Mathf.Approximately(PlayerPrefs.GetFloat(SFXAudioSource.Sfxvolume, 1), 1);
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
        W.GetResource<MusicAudioSource>().SetVolume(volume);
    }
    
    private void OnSfxToggleChanged(bool isOn)
    {
        float volume = isOn ? 1f : 0f;
        W.GetResource<SFXAudioSource>().SetVolume(volume);
    }
    
    private void ClosePopup()
    {
        W.GetResource<FSM>().Value.PopState();
    }
    
    private void OnDisable()
    {
        _musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
        _sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);
        _closeButton.onClick.RemoveListener(ClosePopup);
    }
}
