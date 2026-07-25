using Code.Common.View;
using EasyTweens;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public class ExactCountdownView : ResourceMonoBehaviour<ExactCountdownView>
    {
        private const string ExactCountdownTutorialShown = "ExactCountdownTutorialShown";
        
        [SerializeField] TweenAnimation _animation;
        [SerializeField] private GameObject _exactCountdownTutorial;
        
        public void ExactCountdownFeedback()
        {
            _animation.Play();
            if (PlayerPrefs.GetInt(ExactCountdownTutorialShown, 0) == 0)
            {
                _animation.OnPlayForwardFinished += ShowTutorial;
            }
        }
        private void ShowTutorial()
        {
            _animation.OnPlayForwardFinished -= ShowTutorial;
            _exactCountdownTutorial.SetActive(true);
            PlayerPrefs.SetInt(ExactCountdownTutorialShown, 1);
            PlayerPrefs.Save();
        }
    }
}