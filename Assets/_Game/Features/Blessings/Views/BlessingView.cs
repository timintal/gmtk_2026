using System.Collections.Generic;
using Code.Common;
using Code.Common.View;
using EasyTweens;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace _Game.Features.Blessings.Views
{
    public partial class BlessingView : EntityChildView
    {
        [SerializeField] private TMP_Text _Title;
        [SerializeField] private TMP_Text _Description;
        [SerializeField] SpriteRenderer _background;
 
        [SerializeField] SortingGroup _sortingGroup;
        [SerializeField] Collider2D _collider;
        [SerializeField] string _hoverSortingLayer;
        [SerializeField] TweenAnimation _hoverTweenAnimation;
        [SerializeField] LineRenderer _lineRenderer;
        [SerializeField] private Transform _topArrow;

        [SerializeField] Transform _modifiersContainer;
        [SerializeField] private BlessingsModifierView _blessingsModifierViewPrefab;
        [SerializeField] TweenAnimation _modifierTweenAnimation;

        List<BlessingsModifierView> _modifierViews = new();

        string _originalSortingLayer;
        bool _isHovered;

        public Collider2D Collider => _collider;
        public bool IsHovered => _isHovered;

        void Awake()
        {
            _originalSortingLayer = _sortingGroup.sortingLayerName;
            _lineRenderer.gameObject.SetActive(false);
            _topArrow.gameObject.SetActive(false);
            _blessingsModifierViewPrefab.gameObject.SetActive(false);
            _hoverTweenAnimation.PlayBackward(false);
            _modifierTweenAnimation.PlayBackward(false);
        }

        public void OverrideSortingLayers(string original, string hover)
        {
            _originalSortingLayer = original;
            _hoverSortingLayer = hover;
            _sortingGroup.sortingLayerName = _isHovered ? _hoverSortingLayer : _originalSortingLayer;
        }

        protected override void PostBind()
        {
            UpdateVisuals(Entity);
        }
        private void UpdateVisuals(W.Entity entity)
        {
            CreateModifiers();
            var blessingsLibrary = W.GetResource<BlessingsLibrary>();
            var blessingId = entity.Read<BlessingId>();
            var blessingValue = entity.Read<BlessingValue>();
            var blessingsConfig = blessingsLibrary.GetBlessingConfig(blessingId.Value);
            _Title.text = string.Format(blessingsConfig.Title, blessingValue.Value.ToString("F0"));
            _Description.text = string.Format(blessingsConfig.Description, blessingValue.Value.ToString("F0"));
            _background.color = blessingsConfig.CardBackColor;
        }
        private void CreateModifiers()
        {
            if (Entity.Has<EvenBlessing>())
            {
                var modifierView = Instantiate(_blessingsModifierViewPrefab, _modifiersContainer);
                _modifierViews.Add(modifierView);
                modifierView.gameObject.SetActive(true);
                modifierView.SetModifier("Only affecting even dice");
            }
            if (Entity.Has<OddBlessing>())
            {
                var modifierView = Instantiate(_blessingsModifierViewPrefab, _modifiersContainer);
                _modifierViews.Add(modifierView);
                modifierView.gameObject.SetActive(true);
                modifierView.SetModifier("Only affecting odd dice");
            }
            if (Entity.Has<AffectAllBlessing>())
            {
                var modifierView = Instantiate(_blessingsModifierViewPrefab, _modifiersContainer);
                _modifierViews.Add(modifierView);
                modifierView.gameObject.SetActive(true);
                modifierView.SetModifier("Affecting all dice");
            }
            if (Entity.Has<AffectSameValueDicesBlessing>())
            {
                var modifierView = Instantiate(_blessingsModifierViewPrefab, _modifiersContainer);
                _modifierViews.Add(modifierView);
                modifierView.gameObject.SetActive(true);
                modifierView.SetModifier("Affecting dice with the same value");
            }
        }

        public void SetHovered(bool isHovered)
        {
            if (_isHovered == isHovered)
                return;

            _isHovered = isHovered;
            _sortingGroup.sortingLayerName = isHovered ? _hoverSortingLayer : _originalSortingLayer;
            if (isHovered)
            {
                _hoverTweenAnimation.Play();
                _modifierTweenAnimation.Play();
            }
            else
            {
                _hoverTweenAnimation.PlayBackward();
                _modifierTweenAnimation.PlayBackward();
            }
        }

        private void Update()
        {
            if (_isHovered && Mouse.current.leftButton.isPressed)
            {
                var camera = W.GetResource<MainCamera>().Value;
                var tipPosition = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                tipPosition.z = transform.position.z;
                _topArrow.position = tipPosition;
                var diff = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1) - _lineRenderer.GetPosition(_lineRenderer.positionCount - 2);
                var signedAngle = Vector2.SignedAngle(Vector2.up, diff);
                _topArrow.transform.rotation = Quaternion.Euler(0, 0, signedAngle);

                var diffMagnitude = diff.magnitude;
                diff /= diffMagnitude;
                var endPosition = tipPosition - diff * 0.7f;

                _lineRenderer.SetPosition(1, endPosition);
            }
        }


        public void OnClick()
        {
            _lineRenderer.gameObject.SetActive(true);
            _topArrow.gameObject.SetActive(true);

            _lineRenderer.SetPosition(0, transform.position);
            var camera = W.GetResource<MainCamera>().Value;
            var tipPosition = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _lineRenderer.SetPosition(1, tipPosition);
        }

        public void ResetDrag()
        {
            _lineRenderer.gameObject.SetActive(false);
            _topArrow.gameObject.SetActive(false);
        }
        
        public void OnRelease()
        {
            SetHovered(false);
            _lineRenderer.gameObject.SetActive(false);
            _topArrow.gameObject.SetActive(false);
            Entity.Set(new Position
            {
                Value = _topArrow.position
            });
            Entity.Set<Activated>();
        }
    }
}