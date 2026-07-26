using Code.Common.View;
using EasyTweens;
using TMPro;
using UnityEngine;

public class ErrorMessage : ResourceMonoBehaviour<ErrorMessage>
{
    [SerializeField] TMP_Text _text;
    
    [SerializeField] TweenAnimation _tweenAnimation;
    
    public void Show(string message)
    {
        _text.text = message;
        _tweenAnimation.Play();
    }
}
