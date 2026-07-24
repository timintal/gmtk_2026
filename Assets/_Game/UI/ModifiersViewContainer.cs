using System.Collections.Generic;
using _Game.Features.Enemies;
using Code.Common.View;
using UnityEngine;

namespace _Game.UI
{
    public partial class ModifiersViewContainer : EntityChildView
    {
        [SerializeField] ModifierView _modifierViewPrefab;

        List<ModifierView> _modifierViews = new();
        int usedModifierViews = 0;

        void Awake()
        {
            _modifierViewPrefab.gameObject.SetActive(false);
        }
        
        protected override void PostBind()
        {
            RefreshModifiers();
        }

        public void RefreshModifiers()
        {
            var allModifierEntities = Entity.GetAllModifierEntities();

            ClearModifiers();
            foreach (var modifierEntity in allModifierEntities)
            {
                if (modifierEntity.Has<CountdownModifier>())
                {
                    var modifierType = modifierEntity.Read<CountdownModifier>();
                    var modifierView = GetModifierView();
                    modifierView.SetModifier(modifierType.Type, modifierEntity);
                }
            }
        }

        void ClearModifiers()
        {
            foreach (var modifierView in _modifierViews)
            {
                modifierView.gameObject.SetActive(false);
            }
            usedModifierViews = 0;
        }

        private ModifierView GetModifierView()
        {
            ModifierView modifierView = null;
            if (usedModifierViews < _modifierViews.Count)
            {
                modifierView = _modifierViews[usedModifierViews++];
            }
            else
            {
                modifierView = Instantiate(_modifierViewPrefab, _modifierViewPrefab.transform.parent);
                _modifierViews.Add(modifierView);
            }
            modifierView.gameObject.SetActive(true);
            usedModifierViews++;
            return modifierView;
        }
    }
}