using Code.Ecs;
using Code.GameFlow;
using Code.UI;
using Cysharp.Threading.Tasks;
using EasyTweens;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Greetings : MonoBehaviour
{
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] int startCountdown = 2026;
    [SerializeField] TweenAnimation _tweenAnimation;
    [SerializeField] TweenAnimation _bounceAnimation;
    [SerializeField] private DisableOnAnyInput _startButton;

    private void OnEnable()
    {
        _startButton.OnTriggered.AddListener(StartGame);
    }
    private void StartGame()
    {
        W.GetResource<FSM>().Value.Push<RunGameState>();
    }

    void Awake()
    {
        _countdownText.text = startCountdown.ToString();
        _tweenAnimation.Play();
        _tweenAnimation.OnPlayForwardFinished += OnAnimationFinished;
    }
    private void OnAnimationFinished()
    {
        _tweenAnimation.OnPlayForwardFinished -= OnAnimationFinished;
        PlayCountdown().Forget();
    }
    private async UniTaskVoid PlayCountdown()
    {
        while (startCountdown > 0)
        {
            await UniTask.Delay(Random.Range(30, 120), cancellationToken:destroyCancellationToken);
            startCountdown--;
            _countdownText.text = startCountdown.ToString();
            _bounceAnimation.Play();
        }
        
        _countdownText.text = "C'mon! Click some button already!! Jeez..";
    }
}