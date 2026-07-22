using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTweens
{
    public class SampleScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private TweenAnimation _showAnimation;
        [SerializeField] private TweenAnimation _hideAnimation;

        private async Task Awake()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            _showAnimation.Play();
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveAllListeners();
        }

        private void OnPlayButtonClicked()
        {
            _hideAnimation.Play();
        }
    }
}