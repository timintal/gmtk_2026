using System;
using System.Collections.Generic;
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

        private BlessingView _hovered;

        private List<BlessingView> _blessingViews = new();
        private List<W.Entity> _blessingEntities = new();

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(SkipSelection);
        }
        private void SkipSelection()
        {
            for (var i = 0; i < _blessingViews.Count; i++)
            {
                var viewEntity = _blessingEntities[i];
                viewEntity.Set<Destroyed>();
            }
            CloseScreen();
        }

        [Button]
        public void ShowRewards(int count)
        {
            gameObject.SetActive(true);
            var blessingsLibrary = W.GetResource<BlessingsLibrary>();
            var visualConfig = W.GetResource<VisualConfig>();
            var playerState = W.GetResource<PlayerState>();

            _blessingViews.Clear();
            _blessingEntities.Clear();
            _hovered = null;
        
            List<string> excludeIds = new();
            for (int i = 0; i < count; i++)
            {
                var blessingConfig = blessingsLibrary.GetRandomBlessingConfig(playerState.CurrentLevel, excludeIds);
                var blessingEntity = blessingsLibrary.CreateBlessing(blessingConfig);
                blessingEntity.Set<RewardScreen>();
                var entityView = Instantiate(visualConfig.BlessingCardPrefab, _root.transform);
                entityView.Bind(blessingEntity);
                blessingEntity.Set(new ViewLink() { View = entityView });
                var blessingView = entityView.GetComponent<BlessingView>();
                _blessingViews.Add(blessingView);
                _blessingEntities.Add(blessingEntity);
                blessingView.OverrideSortingLayers(SortingLayers.Popups, SortingLayers.Overlay);
                excludeIds.Add(blessingConfig.BlessingId);
            }
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
        private void SelectHovered()
        {
            if (_hovered != null)
            {
                for (var i = 0; i < _blessingViews.Count; i++)
                {
                    var view = _blessingViews[i];
                    var viewEntity = _blessingEntities[i];
                    if (_hovered == view)
                    {
                        viewEntity.Delete<RewardScreen>();
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
                CloseScreen();
            }
        }
        private void CloseScreen()
        {
            _blessingViews.Clear();
            _blessingEntities.Clear();
            gameObject.SetActive(false);
            W.NewEntity<Default>().Set<StartNewLevelRequest>();
        }


    }
}