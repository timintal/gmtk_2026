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
        [SerializeField] Button _removeButton;
        [SerializeField] private GameObject _addTitle;
        [SerializeField] private GameObject _removeTitle;

        private BlessingView _hovered;

        private List<BlessingView> _blessingViews = new();
        private List<W.Entity> _blessingEntities = new();

        private bool _isAddMode;

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(SkipSelection);
            _removeButton.onClick.AddListener(RemoveMode);
        }
        
        private void OnDisable()
        {
            _skipButton.onClick.RemoveListener(SkipSelection);
            _removeButton.onClick.RemoveListener(RemoveMode);
        }
        [Button]
        public void ShowRewards(int count)
        {
            gameObject.SetActive(true);
            _removeButton.gameObject.SetActive(true);
            _skipButton.gameObject.SetActive(true);
            _addTitle.SetActive(true);
            _removeTitle.SetActive(false);
            
            var blessingsLibrary = W.GetResource<BlessingsLibrary>();
            var visualConfig = W.GetResource<VisualConfig>();
            var playerState = W.GetResource<PlayerState>();

            _blessingViews.Clear();
            _blessingEntities.Clear();
            _hovered = null;
            _isAddMode = true;
        
            List<string> excludeIds = new();
            for (int i = 0; i < count; i++)
            {
                var blessingConfig = blessingsLibrary.GetRandomBlessingConfig(playerState.CurrentLevel, excludeIds);
                var blessingEntity = blessingsLibrary.CreateBlessing(blessingConfig);
                CreateCardPreview(blessingEntity, visualConfig);
                excludeIds.Add(blessingConfig.BlessingId);
            }
        }
        private void RemoveMode()
        {
            _isAddMode = false;
            
            _removeButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(true);
            _addTitle.SetActive(false);
            _removeTitle.SetActive(true);
            RemoveAllCurrentBlessings();
            
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
            blessingEntity.Set(new ViewLink() { View = entityView });
            var blessingView = entityView.GetComponent<BlessingView>();
            _blessingViews.Add(blessingView);
            _blessingEntities.Add(blessingEntity);
            blessingView.OverrideSortingLayers(SortingLayers.Popups, SortingLayers.Overlay);
        }

        private void Update()
        {
            var mouse = Mouse.current;
            var cam = W.GetResource<MainCamera>().Value;
            if (mouse == null)
                return;

            var point = ScreenUtils.ScreenToWorld2D(cam, mouse.position.ReadValue());

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
        
            if (mouse.leftButton.wasReleasedThisFrame && _hovered != null)
            {
                SelectHovered();
            }
        }

        void RemoveAllCurrentBlessings()
        {
            for (var i = 0; i < _blessingViews.Count; i++)
            {
                var viewEntity = _blessingEntities[i];
                viewEntity.Set<Destroyed>();
            }
            _blessingViews.Clear();
            _blessingEntities.Clear();
        }
        
        private void SelectHovered()
        {
            if (_hovered != null)
            {
                for (var i = 0; i < _blessingViews.Count; i++)
                {
                    var view = _blessingViews[i];
                    var viewEntity = _blessingEntities[i];
                    viewEntity.Delete<RewardScreen>();
                    if (_hovered == view && _isAddMode)
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
                    else if (_hovered == view && !_isAddMode)
                    {
                        viewEntity.Set<Destroyed>();
                    }
                    else
                    {
                        if (_isAddMode)
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
            _blessingViews.Clear();
            _blessingEntities.Clear();
            gameObject.SetActive(false);
            
            var newLevelRequest = W.NewEntity<Default>();
            newLevelRequest.Set<StartNewLevelRequest>();
            newLevelRequest.Set(new Delay() { Value = 0.4f });
        }


    }
}