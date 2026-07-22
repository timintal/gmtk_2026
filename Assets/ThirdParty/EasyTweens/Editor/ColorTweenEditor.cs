using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class ColorTweenEditor : TweenEditor
    {
        private VisualElement startField;
        private VisualElement endField;
        private PropertyField gradientField;

        private PropertyField _useGradientField;
        private PropertyField _useHdrField;
        private GradientFieldManipulator _gradientFieldManipulator;

        private SerializedProperty _useHDRProp;

        public ColorTweenEditor(TweenAnimationEditor animationEditor, TweenBase t, VisualTreeAsset visualTreeAsset, StyleSheet styleSheet) : base(animationEditor, t, visualTreeAsset, styleSheet)
        {
        }

        public override void OnBindingsUpdated()
        {
            if (startField == null)
            {
                startField = this.Q<VisualElement>("StartValueRoot");
            }

            if (endField == null)
            {
                endField = this.Q<VisualElement>("EndValueRoot");
                // optionalFieldsParent.Remove(endField);
            }

            gradientField = this.Q<PropertyField>("gradient");

            if (_useGradientField != null)
            {
                _useGradientField.UnregisterCallback<SerializedPropertyChangeEvent>(UpdateFieldsVisibility);
                _useGradientField.Unbind();
            }

            _useGradientField = this.Q<PropertyField>("useGradient");
            _useHdrField = this.Q<PropertyField>("useHDRColor");

            _useGradientField.RegisterValueChangeCallback(UpdateFieldsVisibility);
            _useHDRProp = GetProperty(_serializedObject, "useHDRColor");
            _useHdrField.BindProperty(_useHDRProp);
            _additionalProperties.Add(_useHDRProp);
            _useHdrField.RegisterValueChangeCallback(HandleHDR);
            HandleHDR(null);
#if  !UNITY_2021_1_OR_NEWER
            _useGradientField.style.display = DisplayStyle.None;
#endif
        }

        private void UpdateFieldsVisibility(SerializedPropertyChangeEvent evt)
        {
#if  UNITY_2021_1_OR_NEWER
            if (evt.changedProperty.boolValue)
            {
                HandleHDR(null);

                startField.style.display = DisplayStyle.None;
                endField.style.display = DisplayStyle.None;
                gradientField.style.display = DisplayStyle.Flex;
            }
            else
            {
#endif
                startField.style.display = DisplayStyle.Flex;
                endField.style.display = DisplayStyle.Flex;
                gradientField.style.display = DisplayStyle.None;
#if  UNITY_2021_1_OR_NEWER
            }
#endif
        }

        void HandleHDR(SerializedPropertyChangeEvent evt)
        {
            var gradientGeneratedField = gradientField.Q<GradientField>();
            if (_gradientFieldManipulator != null)
            {
                _gradientFieldManipulator.target.RemoveManipulator(_gradientFieldManipulator);
            }

            var needHDR = (bool)Tween.GetType().GetField("useHDRColor").GetValue(Tween);
            if (gradientGeneratedField != null)
            {
                gradientGeneratedField.focusable = false;
                _gradientFieldManipulator = new GradientFieldManipulator(gradientGeneratedField, () =>
                {
                    ShowGradientPicker(needHDR, gradientGeneratedField);
                });
            }

            var startColorField = startField.Q<ColorField>();
            var endColorField = endField.Q<ColorField>();
            if (startColorField != null && endColorField != null)
            {
                startColorField.hdr = endColorField.hdr = needHDR;
                startColorField.MarkDirtyRepaint();
                endColorField.MarkDirtyRepaint();
            }
        }

        void ShowGradientPicker(bool needHDR, GradientField gradientGeneratedField)
        {
            var gradientField = Tween.GetType().GetField("gradient");
            var gradient = (Gradient)gradientField.GetValue(Tween);

            var gradientPickerType = typeof(Editor).Assembly.GetType("UnityEditor.GradientPicker");

            var showMethod = gradientPickerType.GetMethod("Show", BindingFlags.NonPublic | BindingFlags.Static);

            Action<Gradient> action = (Gradient g) =>
            {
                gradientField.SetValue(Tween, g);
                gradientGeneratedField.SetValueWithoutNotify(g);
                gradientGeneratedField.MarkDirtyRepaint();
            };
            showMethod.Invoke( null,new object[] {gradient, needHDR, gradientGeneratedField.colorSpace, action, null});
        }
    }

}
