using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class BezierCurveEditor : VisualElement
    {
        private readonly int _startIndex;
        private readonly SerializedObject _splineSo;
        private Vector3Field _p0;
        private Vector3Field _p1;
        private Vector3Field _p2;
        private Vector3Field _p3;

        public BezierCurveEditor(int startIndex, SerializedObject splineSO, VisualTreeAsset visualTreeAsset, Action onDirty = null)
        {
            _startIndex = startIndex;
            _splineSo = splineSO;
            visualTreeAsset.CloneTree(this);
            _p0 = this.Q<Vector3Field>("Point0");
            _p0.BindProperty(GetPointsProperty(0));
            _p1 = this.Q<Vector3Field>("Point1");
            _p1.BindProperty(GetPointsProperty(1));
            _p2 = this.Q<Vector3Field>("Point2");
            _p2.BindProperty(GetPointsProperty(2));
            _p3 = this.Q<Vector3Field>("Point3");
            _p3.BindProperty(GetPointsProperty(3));

            int modeIndex = (startIndex + 1) / 3;
            var controlMode0 = this.Q<EnumField>("ControlPointMode0");
            controlMode0.RegisterValueChangedCallback(evt =>
            {
                var spline = splineSO.targetObject as BezierSpline;
                spline.EnforceMode(startIndex);
            });
            controlMode0.BindProperty(GetModesProperty(modeIndex));
            
            modeIndex = (startIndex + 2) / 3;
            var controlMode1 = this.Q<EnumField>("ControlPointMode1");
            controlMode1.BindProperty(GetModesProperty(modeIndex));
            controlMode1.RegisterValueChangedCallback(evt =>
            {
                var spline = splineSO.targetObject as BezierSpline;
                spline.EnforceMode(startIndex + 1);
            });
            
            modeIndex = (startIndex + 3) / 3;
            var controlMode2 = this.Q<EnumField>("ControlPointMode2");
            controlMode2.BindProperty(GetModesProperty(modeIndex));
            controlMode2.RegisterValueChangedCallback(evt =>
            {
                var spline = splineSO.targetObject as BezierSpline;
                spline.EnforceMode(startIndex + 2);
            });
            
            modeIndex = (startIndex + 4) / 3;
            var controlMode3 = this.Q<EnumField>("ControlPointMode3");
            controlMode3.BindProperty(GetModesProperty(modeIndex));
            controlMode3.RegisterValueChangedCallback(evt =>
            {
                var spline = splineSO.targetObject as BezierSpline;
                spline.EnforceMode(startIndex + 3);
            });
            
            this.Q<Foldout>("MainFoldout").text = $"Curve {(startIndex ) / 3}";
            
            var button = this.Q<Button>("RemoveCurve");
            button.clickable.clicked += () =>
            {
                var spline = splineSO.targetObject as BezierSpline;
                Undo.RecordObject(spline, "Remove Curve");
                spline.RemoveCurve((startIndex - 1) / 3);
                EditorUtility.SetDirty(spline);
                if (onDirty != null) onDirty();
            };
        }

        public void UpdateLockedAxis()
        {
            var spline = _splineSo.targetObject as BezierSpline;
            _p0.Q<FloatField>("unity-x-input").style.display = spline.lockXAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p0.Q<FloatField>("unity-y-input").style.display = spline.lockYAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p0.Q<FloatField>("unity-z-input").style.display = spline.lockZAxis ? DisplayStyle.None : DisplayStyle.Flex;
            
            _p1.Q<FloatField>("unity-x-input").style.display = spline.lockXAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p1.Q<FloatField>("unity-y-input").style.display = spline.lockYAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p1.Q<FloatField>("unity-z-input").style.display = spline.lockZAxis ? DisplayStyle.None : DisplayStyle.Flex;
            
            _p2.Q<FloatField>("unity-x-input").style.display = spline.lockXAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p2.Q<FloatField>("unity-y-input").style.display = spline.lockYAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p2.Q<FloatField>("unity-z-input").style.display = spline.lockZAxis ? DisplayStyle.None : DisplayStyle.Flex;
            
            _p3.Q<FloatField>("unity-x-input").style.display = spline.lockXAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p3.Q<FloatField>("unity-y-input").style.display = spline.lockYAxis ? DisplayStyle.None : DisplayStyle.Flex;
            _p3.Q<FloatField>("unity-z-input").style.display = spline.lockZAxis ? DisplayStyle.None : DisplayStyle.Flex;
            
        }
    
        public SerializedProperty GetPointsProperty(int index)
        {
            return _splineSo.FindProperty("points").GetArrayElementAtIndex(_startIndex + index);
        }
        public SerializedProperty GetModesProperty(int index)
        {
            return _splineSo.FindProperty("modes").GetArrayElementAtIndex(index);
        }
        
        
    }
}