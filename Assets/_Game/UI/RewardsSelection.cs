using System.Collections.Generic;
using System.Linq;
using _Game.Features.Blessings;
using _Game.Features.Blessings.Views;
using _Game.Features.Dice;
using _Game.Features.Run;
using _Game.Features.Visuals;
using Code.Common;
using Code.Common.Utils;
using Code.Common.View;
using Code.Configs;
using Code.Generated;
using DG.Tweening;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Game.UI
{
    public class RewardsSelection : ResourceMonoBehaviour<RewardsSelection>
    {
        [SerializeField] private SimpleLineLayout _root;
        [SerializeField] Button _skipButton;
        [SerializeField] Button _backButton;
        [SerializeField] Button _removeButton;
        [SerializeField] Button _previewDeckButton;
        [SerializeField] private GameObject _addTitle;
        [SerializeField] private GameObject _removeTitle;
        [SerializeField] private GameObject _yourDeckTitle;

        private BlessingView _hovered;

        private List<BlessingView> _blessingViews = new();
        private List<W.Entity> _blessingEntities = new();
        private List<BlessingsConfig> _generatedConfigs = new();

        private bool _isAddMode;
        private bool _deckPreviewMode;

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(SkipSelection);
            _removeButton.onClick.AddListener(RemoveMode);
            _backButton.onClick.AddListener(BackToRewards);
            _previewDeckButton.onClick.AddListener(PreviewDeck);
        }
        private void OnDisable()
        {
            _skipButton.onClick.RemoveListener(SkipSelection);
            _removeButton.onClick.RemoveListener(RemoveMode);
            _backButton.onClick.RemoveListener(BackToRewards);
            _previewDeckButton.onClick.RemoveListener(PreviewDeck);
        }

        private void PreviewDeck()
        {
            _deckPreviewMode = true;
            _isAddMode = false;

            _addTitle.SetActive(false);
            _removeTitle.SetActive(false);
            _yourDeckTitle.SetActive(true);

            _backButton.gameObject.SetActive(true);
            _removeButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(false);
            _previewDeckButton.gameObject.SetActive(false);

            DestroyAllGeneratedBlessings();

            //add views to active blessings
            foreach (var e in W.Query<All<Blessing, DrawPile>>().Entities())
            {
                var visualConfig = W.GetResource<VisualConfig>();
                CreateCardPreview(e, visualConfig);
            }
        }
        private void BackToRewards()
        {
            _isAddMode = true;
            _deckPreviewMode = false;

            for (var i = 0; i < _blessingViews.Count; i++)
            {
                var viewEntity = _blessingEntities[i];
                viewEntity.Delete<RewardScreen>();
                viewEntity.Set<NeedCleanupView>();
                viewEntity.PutBlessingInDrawPile();
            }

            ShowRewards(_generatedConfigs.Count, false);
        }

        [Button]
        public void ShowRewards(int count, bool regenerate = true)
        {
            gameObject.SetActive(true);
            _removeButton.gameObject.SetActive(true);
            _skipButton.gameObject.SetActive(true);
            _previewDeckButton.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(false);

            _addTitle.SetActive(true);
            _removeTitle.SetActive(false);
            _yourDeckTitle.SetActive(false);

            var blessingsLibrary = W.GetResource<BlessingsLibrary>();
            var visualConfig = W.GetResource<VisualConfig>();
            var playerState = W.GetResource<PlayerState>();

            _blessingViews.Clear();
            _blessingEntities.Clear();
            _hovered = null;
            _isAddMode = true;
            _deckPreviewMode = false;

            if (regenerate)
            {
                _generatedConfigs.Clear();
                List<string> excludeIds = new();
                for (int i = 0; i < count; i++)
                {
                    var blessingConfig = blessingsLibrary.GetRandomBlessingConfig(playerState.CurrentLevel, excludeIds);
                    var blessingEntity = blessingsLibrary.CreateBlessing(blessingConfig);
                    CreateCardPreview(blessingEntity, visualConfig);
                    excludeIds.Add(blessingConfig.BlessingId);
                    _generatedConfigs.Add(blessingConfig);
                }
            }
            else
            {
                foreach (var blessingConfig in _generatedConfigs)
                {
                    var blessingEntity = blessingsLibrary.CreateBlessing(blessingConfig);
                    CreateCardPreview(blessingEntity, visualConfig);
                }
            }
        }
        private void RemoveMode()
        {
            _isAddMode = false;

            _removeButton.gameObject.SetActive(false);
            _backButton.gameObject.SetActive(true);
            _skipButton.gameObject.SetActive(false);
            _previewDeckButton.gameObject.SetActive(false);

            _addTitle.SetActive(false);
            _removeTitle.SetActive(true);
            _yourDeckTitle.SetActive(false);

            DestroyAllGeneratedBlessings();

            foreach (var e in W.Query<All<Blessing, DrawPile>>().Entities())
            {
                var visualConfig = W.GetResource<VisualConfig>();
                CreateCardPreview(e, visualConfig);
            }
        }
        private void SkipSelection()
        {
            for (var i = 0; i < _blessingViews.Count; i++)
            {
                if (_isAddMode)
                {
                    var viewEntity = _blessingEntities[i];
                    viewEntity.Set<Destroyed>();
                }
                else
                {
                    var viewEntity = _blessingEntities[i];
                    viewEntity.Delete<RewardScreen>();
                    viewEntity.PutBlessingInDrawPile();
                }
            }
            CloseScreen();
        }

        private void CreateCardPreview(World<WT>.Entity blessingEntity, VisualConfig visualConfig)
        {
            blessingEntity.Set<RewardScreen>();
            var entityView = Instantiate(visualConfig.BlessingCardPrefab, _root.transform);
            entityView.Bind(blessingEntity);
            blessingEntity.Set(new ViewLink()
            {
                View = entityView
            });
            var blessingView = entityView.GetComponent<BlessingView>();
            _blessingViews.Add(blessingView);
            _blessingEntities.Add(blessingEntity);
            blessingView.OverrideSortingLayers(SortingLayers.Popups, SortingLayers.Overlay);
        }

        private void Update()
        {
            // Pointer unifies mouse and touch: press == left button / primary touch.
            var pointer = Pointer.current;
            var cam = W.GetResource<MainCamera>().Value;
            if (pointer == null)
                return;

            var point = ScreenUtils.ScreenToWorld2D(cam, pointer.position.ReadValue());

            BlessingView best = null;
            for (var i = 0; i < _blessingViews.Count; i++)
            {
                var view = _blessingViews[i];
                if (view.Collider == null)
                    continue;

                if (view.Collider.OverlapPoint(point))
                    best = view; // ascending scan => last overlapping child (top-most) wins
            }

            if (best != _hovered)
            {
                if (_hovered != null)
                {
                    _hovered.SetHovered(false);
                }
                _hovered = best;
                if (_hovered != null)
                {
                    _hovered.SetHovered(true);
                }
            }

            if (pointer.press.wasReleasedThisFrame && _hovered != null)
            {
                SelectHovered();
            }
        }

        void DestroyAllGeneratedBlessings()
        {
            foreach (var e in W.Query<All<Blessing, RewardScreen>, None<DrawPile>>().Entities())
            {
                e.Set<Destroyed>();
            }

            _blessingViews.Clear();
            _blessingEntities.Clear();
        }

        private void SelectHovered()
        {
            if (_deckPreviewMode)
            {
                return;
            }

            if (_hovered != null)
            {
                for (var i = 0; i < _blessingViews.Count; i++)
                {
                    var view = _blessingViews[i];
                    var viewEntity = _blessingEntities[i];
                    viewEntity.Delete<RewardScreen>();

                    if (_isAddMode)
                    {
                        if (_hovered == view)
                        {
                            viewEntity.PutBlessingInDrawPile();
                            viewEntity.Delete<ViewLink>();

                            view.transform.SetParent(null);
                            view.transform.DOMove(new Vector3(0, -10, 0), 0.8f).OnComplete(() =>
                            {
                                Destroy(view.gameObject);
                            }).SetEase(Ease.InOutElastic);
                            view.transform.DOScale(Vector3.zero, 0.3f).SetDelay(0.4f);
                        }
                        else
                        {
                            viewEntity.Set<Destroyed>();
                        }
                    }
                    else if (!_isAddMode)
                    {
                        if (_hovered == view)
                        {
                            viewEntity.Set<Destroyed>();
                        }
                        else
                        {
                            viewEntity.Set<NeedCleanupView>();
                            viewEntity.PutBlessingInDrawPile();
                        }
                    }
                }
                CloseScreen();
            }
        }
        private void CloseScreen()
        {
            DestroyAllGeneratedBlessings();
            _blessingViews.Clear();
            _blessingEntities.Clear();
            _generatedConfigs.Clear();
            gameObject.SetActive(false);

            var newLevelRequest = W.NewEntity<Default>();
            newLevelRequest.Set<StartNewLevelRequest>();
            newLevelRequest.Set(new Delay()
            {
                Value = 0.4f
            });
        }


    }
}