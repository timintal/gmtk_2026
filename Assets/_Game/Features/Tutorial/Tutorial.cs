using _Game.Features.Blessings;
using _Game.Features.Blessings.Views;
using _Game.Features.Dice;
using DG.Tweening;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Features.Tutorial
{
    public class Tutorial : MonoBehaviour
    {
        public const string TutorialStep = "tutorStep";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _draggingTutorial;
        [SerializeField] private GameObject _dragDiceToEnemyTutorial;
        [SerializeField] private GameObject _blessingsTutorial;
        [SerializeField] private GameObject _turnTutorial;
        [SerializeField] private RectFade _toolboxFade;
        [SerializeField] private RectTransform _toolboxAnchor;
        [SerializeField] private RectTransform _dragOnEnemiesRect;
        [SerializeField] private RectTransform _diceBlessingAnchor;
        [SerializeField] private RectTransform _turnTutorialAnchor;


        bool _shown;
        int _tutorialStep;
        bool _isDragTutorialShown;

        private int _dicesInitialCount = -1;
        private int _blessingsInitialCount = -1;
        private float _step2ShowTime = -1;
        private int _step2Clicks = -1;

        private void Awake()
        {
            _tutorialStep = PlayerPrefs.GetInt(TutorialStep, 0);

            if (_tutorialStep >= 4)
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

            if (_tutorialStep == 2 && W.HasResource<PlayerState>())
            {
                var currentLevel = W.GetResource<PlayerState>().CurrentLevel;
                if (currentLevel == 2)
                {
                    if (_step2Clicks == -1)
                    {
                        _toolboxFade.SetRect(_turnTutorialAnchor.anchoredPosition, _turnTutorialAnchor.sizeDelta);

                        _turnTutorial.SetActive(true);
                        Show();
                        _step2Clicks = 0;
                        _step2ShowTime = Time.time;
                    }
                    else if (_step2Clicks >=0 && 
                             Pointer.current.press.wasPressedThisFrame &&
                             Time.time - _step2ShowTime > 2)
                    {
                        _step2Clicks++;
                    }
                }

                if (currentLevel > 2 || _step2Clicks > 0 && Time.time - _step2ShowTime > 2)
                {
                    _tutorialStep = 3;
                    PlayerPrefs.SetInt(TutorialStep, _tutorialStep);
                    PlayerPrefs.Save();
                    Hide();
                    _turnTutorial.SetActive(false);
                }

            }
            if (_tutorialStep == 3)
            {
                var blessingsCount = W.Query<All<DiceBlessing, Hand>>().EntitiesCount();

                var hoveredView = W.GetResource<BlessingsContainerView>().Hovered;
                if (hoveredView != null && hoveredView.Entity.Has<DiceBlessing>())
                {
                    _blessingsInitialCount = blessingsCount;
                    Show();
                    _blessingsTutorial.SetActive(true);
                    _toolboxFade.SetRect(_diceBlessingAnchor.anchoredPosition, _diceBlessingAnchor.sizeDelta);
                }
                else
                {
                    Hide();
                    _blessingsTutorial.SetActive(false);
                    if (blessingsCount < _blessingsInitialCount)
                    {
                        _tutorialStep = 4;
                        PlayerPrefs.SetInt(TutorialStep, _tutorialStep);
                        PlayerPrefs.Save();
                    }
                }


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