using System.Collections.Generic;

namespace SRDebugger.UI.Other
{
    using SRF;
    using UnityEngine;
    using UnityEngine.UI;

    public class CategoryGroup : SRMonoBehaviourEx
    {
        [RequiredField] public RectTransform Container;
        [RequiredField] public Text Header;
        [RequiredField] public GameObject Background;
        [RequiredField] public Toggle SelectionToggle;
        [SerializeField] private Button _toggleVisibility;
        [SerializeField] private RectTransform expandIndicator;
        
        private bool Expanded;

        public List<GameObject> childControls = new List<GameObject>(); 
        
        public GameObject[] EnabledDuringSelectionMode = new GameObject[0];

        private bool _selectionModeEnabled = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            _toggleVisibility.onClick.AddListener(ToggleControls);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _toggleVisibility.onClick.RemoveListener(ToggleControls);
        }

        private void ToggleControls()
        {
            Expanded = !Expanded;

            SetControlsState(Expanded);
            
        }

        public void SetControlsState(bool visible)
        {
            foreach (var control in childControls)
            {
                control.SetActive(visible);
            }
            
            expandIndicator.localRotation = Quaternion.Euler(0,0, visible ? 180 : -90);
        }

        public bool IsSelected
        {
            get
            {
                return SelectionToggle.isOn;
            }
            set
            {
                SelectionToggle.isOn = value;

                if (SelectionToggle.graphic != null)
                {
                    SelectionToggle.graphic.CrossFadeAlpha(value ? _selectionModeEnabled ? 1.0f : 0.2f : 0f, 0, true);
                }
            }
        }

        public bool SelectionModeEnabled
        {
            get { return _selectionModeEnabled; }

            set
            {
                if (value == _selectionModeEnabled)
                {
                    return;
                }

                _selectionModeEnabled = value;

                for (var i = 0; i < EnabledDuringSelectionMode.Length; i++)
                {
                    EnabledDuringSelectionMode[i].SetActive(_selectionModeEnabled);
                }
            }
        }

    }
}
