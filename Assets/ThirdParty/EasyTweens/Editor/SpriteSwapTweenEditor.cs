using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class FrameDataView : VisualElement
    {
        public PropertyField TargetField;
        public FloatField ValueField;
        public Action OnValueChangedCallback;

        public FrameDataView(VisualTreeAsset visualTreeAsset)
        {
            visualTreeAsset.CloneTree(this);
            TargetField = this.Q<PropertyField>("Target");
            TargetField.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var children = TargetField.Children();
                foreach (var child in children)
                {
                    child.style.flexGrow = 1;
                }
                TargetField.Q<Label>().style.maxWidth = 80;
            });

            ValueField = this.Q<FloatField>("Value");
            ValueField.RegisterValueChangedCallback(evt => OnValueChangedCallback?.Invoke());
        }
    }

    [CustomTweenEditor(typeof(TweenSpriteSwap))]
    public class SpriteSwapTweenEditor : TweenEditor
    {
        public VisualTreeAsset FrameDataTemplate;
        TweenSpriteSwap _tween;
        DragnDropFramesManipulator _dragFramesManipulator;

        public SpriteSwapTweenEditor(TweenAnimationEditor animationEditor, TweenBase t, VisualTreeAsset visualTreeAsset, StyleSheet styleSheet) : base(animationEditor, t,
            visualTreeAsset, styleSheet)
        {
            _tween = t as TweenSpriteSwap;
            _tween.startValue = 0;
            _tween.endValue = 1;
            _dragFramesManipulator = new DragnDropFramesManipulator(this, animationEditor.Animation);

            SetupFramesList();
        }
        private void SetupFramesList()
        {
            var framesList = this.Q<ListView>("FramesList");
            framesList.itemIndexChanged += ListReordered;
            framesList.itemsAdded += FramesCountChanged;
            framesList.itemsRemoved += FramesCountChanged;
            framesList.makeItem = () =>
            {
                var newView = new FrameDataView(FrameDataTemplate);
                newView.ValueField.value = 1;
                newView.OnValueChangedCallback = () =>
                {
                    EditorApplication.delayCall += DelayedUpdate;
                };
                return newView;
            };
            framesList.bindItem = (element, i) =>
            {
                var frameDataView = (FrameDataView)element;
                frameDataView.TargetField.BindProperty(GetProperty(_serializedObject, $"frames.Array.data[{i}].sprite"));
                frameDataView.ValueField.BindProperty(GetProperty(_serializedObject, $"frames.Array.data[{i}].relativeDuration"));
                frameDataView.TargetField.label = $"{i}";
            };
        }

        public override void OnBindingsUpdated()
        {
            SetupTemplate();
            this.Q<FloatField>("Duration").RegisterValueChangedCallback(evt => UpdateFrameVisuals());

            var framesList = this.Q<ListView>("FramesList");
            framesList.BindProperty(GetProperty(_serializedObject, "frames"));
            UpdateFrameVisuals();
        }
        private void FramesCountChanged(IEnumerable<int> obj)
        {
            EditorApplication.delayCall += DelayedUpdate;
        }
        void DelayedUpdate()
        {
            EditorApplication.delayCall -= DelayedUpdate;
            UpdateFrameVisuals();
        }
        private void ListReordered(int arg1, int arg2)
        {
            Debug.Log($"List reordered: {arg1} -> {arg2}");
        }
        private void AddFrame()
        {
            _tween.frames.Add(new FrameData
            {
                sprite = null,
                relativeDuration = 1
            });

            UpdateFrameVisuals();
        }

        public void UpdateFrameVisuals()
        {
            if (_serializedObject != null)
                _serializedObject.UpdateIfRequiredOrScript();

            var relativeDuration = _tween.frames.Sum(data => data.relativeDuration);
            var framesList = this.Q<ListView>("FramesList");
            framesList.headerTitle = $"Frames({_tween.frames.Count})";
            if (relativeDuration > 0)
            {
                var msPerNormalizedTime = _tween.Duration / relativeDuration * 1000f;
                var root = framesList.Q<VisualElement>("unity-content-container");
                int index = 0;
                foreach (var child in root.Children())
                {
                    var frameData = _tween.frames[index];
                    var frameDataView = child.Q<FrameDataView>();
                    frameDataView.TargetField.label = $"{frameData.relativeDuration * msPerNormalizedTime:F0}ms";
                    index++;
                }
            }
            else
            {
                framesList.headerTitle = _tween.frames.Count == 0 ? " <color=#FF1111>No Frames</color>" :" <color=#FF1111>Frame timings not set</color>";
            }
        }

        private void SetupTemplate()
        {
            var result = AssetDatabase.FindAssets("t:VisualTreeAsset FrameData");
            if (result.Length > 0)
            {
                FrameDataTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(result[0]));
            }
            else
            {
                Debug.LogError("FrameData template not found");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            var listView = this.Q<ListView>("FramesList");
            listView.itemsAdded -= FramesCountChanged;
            listView.itemsRemoved -= FramesCountChanged;
            listView.itemIndexChanged -= ListReordered;

            if (_dragFramesManipulator != null)
            {
                _dragFramesManipulator.target.RemoveManipulator(_dragFramesManipulator);
                _dragFramesManipulator = null;
            }
        }
    }
    [CustomTweenEditor(typeof(TweenImageSpriteSwap))]
    public class ImageSpriteSwapTweenEditor : TweenEditor
    {
        public VisualTreeAsset FrameDataTemplate;
        TweenImageSpriteSwap _tween;
        DragnDropImageFramesManipulator _dragFramesManipulator;

        public ImageSpriteSwapTweenEditor(TweenAnimationEditor animationEditor, TweenBase t, VisualTreeAsset visualTreeAsset, StyleSheet styleSheet) : base(animationEditor, t,
            visualTreeAsset, styleSheet)
        {
            _tween = t as TweenImageSpriteSwap;
            _tween.startValue = 0;
            _tween.endValue = 1;
            _dragFramesManipulator = new DragnDropImageFramesManipulator(this, animationEditor.Animation);

            SetupFramesList();
        }
        private void SetupFramesList()
        {
            var framesList = this.Q<ListView>("FramesList");
            framesList.itemIndexChanged += ListReordered;
            framesList.itemsAdded += FramesCountChanged;
            framesList.itemsRemoved += FramesCountChanged;
            framesList.makeItem = () =>
            {
                var newView = new FrameDataView(FrameDataTemplate);
                newView.ValueField.value = 1;
                newView.OnValueChangedCallback = () =>
                {
                    EditorApplication.delayCall += DelayedUpdate;
                };
                return newView;
            };
            framesList.bindItem = (element, i) =>
            {
                var frameDataView = (FrameDataView)element;
                frameDataView.TargetField.BindProperty(GetProperty(_serializedObject, $"frames.Array.data[{i}].sprite"));
                frameDataView.ValueField.BindProperty(GetProperty(_serializedObject, $"frames.Array.data[{i}].relativeDuration"));
                frameDataView.TargetField.label = $"{i}";
            };
        }

        public override void OnBindingsUpdated()
        {
            SetupTemplate();
            this.Q<FloatField>("Duration").RegisterValueChangedCallback(evt => UpdateFrameVisuals());

            var framesList = this.Q<ListView>("FramesList");
            framesList.BindProperty(GetProperty(_serializedObject, "frames"));
            UpdateFrameVisuals();
        }
        private void FramesCountChanged(IEnumerable<int> obj)
        {
            EditorApplication.delayCall += DelayedUpdate;
        }
        void DelayedUpdate()
        {
            EditorApplication.delayCall -= DelayedUpdate;
            UpdateFrameVisuals();
        }
        private void ListReordered(int arg1, int arg2)
        {
            Debug.Log($"List reordered: {arg1} -> {arg2}");
        }
        private void AddFrame()
        {
            _tween.frames.Add(new FrameData
            {
                sprite = null,
                relativeDuration = 1
            });

            UpdateFrameVisuals();
        }

        public void UpdateFrameVisuals()
        {
            if (_serializedObject != null)
                _serializedObject.UpdateIfRequiredOrScript();

            var relativeDuration = _tween.frames.Sum(data => data.relativeDuration);
            var framesList = this.Q<ListView>("FramesList");
            framesList.headerTitle = $"Frames({_tween.frames.Count})";
            if (relativeDuration > 0)
            {
                var msPerNormalizedTime = _tween.Duration / relativeDuration * 1000f;
                var root = framesList.Q<VisualElement>("unity-content-container");
                int index = 0;
                foreach (var child in root.Children())
                {
                    var frameData = _tween.frames[index];
                    var frameDataView = child.Q<FrameDataView>();
                    frameDataView.TargetField.label = $"{frameData.relativeDuration * msPerNormalizedTime:F0}ms";
                    index++;
                }
            }
            else
            {
                framesList.headerTitle = _tween.frames.Count == 0 ? " <color=#FF1111>No Frames</color>" :" <color=#FF1111>Frame timings not set</color>";
            }
        }

        private void SetupTemplate()
        {
            var result = AssetDatabase.FindAssets("t:VisualTreeAsset FrameData");
            if (result.Length > 0)
            {
                FrameDataTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(result[0]));
            }
            else
            {
                Debug.LogError("FrameData template not found");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            var listView = this.Q<ListView>("FramesList");
            listView.itemsAdded -= FramesCountChanged;
            listView.itemsRemoved -= FramesCountChanged;
            listView.itemIndexChanged -= ListReordered;

            if (_dragFramesManipulator != null)
            {
                _dragFramesManipulator.target.RemoveManipulator(_dragFramesManipulator);
                _dragFramesManipulator = null;
            }
        }
    }
}