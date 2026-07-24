using Code.Ecs;
using Code.GameFlow;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    
    private void OnEnable()
    {
        _button.onClick.AddListener(OpenSettings);
    }
    
    private void OnDisable()
    {
        _button.onClick.RemoveListener(OpenSettings);
    }
    public void OpenSettings()
    {
        W.GetResource<FSM>().Value.Push<SettingsState>();
    }
}
