using _Game.Features.Blessings;
using Code.Features.DragAndDrop;
using DG.Tweening;
using FFS.Libraries.StaticEcs;
using UnityEngine;
namespace _Game.Features.Tutorial
{
    public class Tutorial : MonoBehaviour
    {
        public const string TutorialStep = "tutorStep";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _draggingTutorial;
        [SerializeField] private GameObject _dragDiceToEnemyTutorial;
        [SerializeField] private GameObject _blessingsTutorial;
        [SerializeField] private RectFade _toolboxFade;
        [SerializeField] private RectTransform _toolboxAnchor;
        [SerializeField] private RectTransform _dragOnEnemiesRect;

        bool _shown;
        int _tutorialStep;
        bool _isDragTutorialShown;

        private int _dicesInitialCount = -1;

        private void Awake()
        {
            _tutorialStep = PlayerPrefs.GetInt(TutorialStep, 0);

            if (_tutorialStep >= 2)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _blessingsTutorial.SetActive(false);
            _draggingTutorial.SetActive(false);
            _dragDiceToEnemyTutorial.SetActive(false);
            Hide(true);
        }

        void Update()
        {
            if (_tutorialStep == 0)
            {
                _toolboxFade.SetRect(_toolboxAnchor.anchoredPosition, _toolboxAnchor.sizeDelta);
                Show();
                _draggingTutorial.SetActive(true);

                if (W.Query<All<Dice.Dice>>().EntitiesCount() > 0)
                {
                    _tutorialStep = 1;
                    PlayerPrefs.SetInt(TutorialStep, _tutorialStep);
                    Hide();
                    _draggingTutorial.SetActive(false);
                }

                return;
            }
            if (_tutorialStep == 1)
            {
                var dicesCount = W.Query<All<Dice.Dice>>().EntitiesCount();

                if (_dicesInitialCount != -1 && dicesCount == 0)
                {
                    return;
                }
                
                if (dicesCount > _dicesInitialCount)
                {
                    _dicesInitialCount = dicesCount;
                }
                
                if (_dicesInitialCount > 0)
                {
                    if (dicesCount < _dicesInitialCount)
                    {
                        _tutorialStep = 2;
                        PlayerPrefs.SetInt(TutorialStep, _tutorialStep);
                        PlayerPrefs.Save();
                        Hide();
                        _dragDiceToEnemyTutorial.SetActive(false);
                    }
                    else
                    {
                        Show();
                        _toolboxFade.SetRect(_dragOnEnemiesRect.anchoredPosition, _dragOnEnemiesRect.sizeDelta);
                        _dragDiceToEnemyTutorial.SetActive(true);
                    }
                }

                return;
            }
        }
        private void Hide(bool force = false)
        {
            _canvasGroup.blocksRaycasts = false;
            if (force)
            {
                _canvasGroup.alpha = 0;
                return;
            }          
            
            if (_shown)
            {
                _canvasGroup.DOFade(0, 0.5f);
            }
            _shown = false;
        }
        private void Show()
        {
            _canvasGroup.blocksRaycasts = true;
            if (!_shown)
            {
                _canvasGroup.DOFade(1, 0.5f);
            }
            _shown = true;
        }
    }
}