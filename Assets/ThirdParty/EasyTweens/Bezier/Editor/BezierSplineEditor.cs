using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace EasyTweens
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineEditor : Editor
    {
        private const float handleSize = 0.04f;

        public VisualTreeAsset mainUXML;
        public VisualTreeAsset curveUXML;

        private readonly Color[] modeColors =
        {
            Color.white,
            new(1,0.6f,0.2f),
            Color.cyan
        };

        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;
        private List<int> selectedindexes = new List<int>();

        private List<BezierCurveEditor> _curveEditors = new List<BezierCurveEditor>();
        public override VisualElement CreateInspectorGUI()
        {
            spline = target as BezierSpline;

            _root = new VisualElement();
            if (mainUXML == null)
            {
                var path = AssetDatabase.FindAssets("BezierSplineEditor t:visualtreeasset")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .First();
                mainUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }
            
            if (curveUXML == null)
            {
                var path = AssetDatabase.FindAssets("BezierCurveEditor t:visualtreeasset")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .First();
                curveUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }

            mainUXML.CloneTree(_root);

            var toggle = _root.Q<Toggle>("LoopToggle");
            toggle.RegisterValueChangedCallback(evt =>
            {
                spline.Loop = evt.newValue;
                EditorUtility.SetDirty(spline);
            });
            toggle.value = spline.Loop;

            _curvesContainer = _root.Q<ScrollView>("CurvesContainer");
            _root.Q<Button>("LengthUpdate").clickable.clicked += () =>
            {
                spline.RecalculateCurveLengths();
                EditorUtility.SetDirty(spline);
            };
            _root.Q<FloatField>("Precision").RegisterValueChangedCallback(evt =>
            {
                spline.RecalculateCurveLengths();
                EditorUtility.SetDirty(spline);
            });
            
            
            SetupLockAxisToggles();
            
            AddCurves();

            return _root;
        }

        private void SetupLockAxisToggles()
        {
            var lockX = _root.Q<Toggle>("LockX");
            var lockY = _root.Q<Toggle>("LockY");
            var lockZ = _root.Q<Toggle>("LockZ");

            var resetAxisButton = _root.Q<Button>("ResetSelectedAxis");
            resetAxisButton.clickable.clicked += () =>
            {
                if (spline.lockXAxis)
                {
                    for (var i = 0; i < spline.Points.Length; i++)
                    {
                        spline.Points[i].x = 0;
                    }
                }
                else if (spline.lockYAxis)
                {
                    for (var i = 0; i < spline.Points.Length; i++)
                    {
                        spline.Points[i].y = 0;
                    }
                }
                else if (spline.lockZAxis)
                {
                    for (var i = 0; i < spline.Points.Length; i++)
                    {
                        spline.Points[i].z = 0;
                    }
                }
            };

            Action updateLocks = () =>
            {
                var spline = target as BezierSpline;
                bool needDisableOther = spline.lockXAxis || spline.lockYAxis || spline.lockZAxis;
                
                lockX.SetEnabled(!needDisableOther || this.spline.lockXAxis);
                lockY.SetEnabled(!needDisableOther || this.spline.lockYAxis);
                lockZ.SetEnabled(!needDisableOther || this.spline.lockZAxis);
                
                string selectedAxis = this.spline.lockXAxis ? "X" : this.spline.lockYAxis ? "Y" : this.spline.lockZAxis ? "Z" : "";
                resetAxisButton.style.visibility = needDisableOther ? Visibility.Visible : Visibility.Hidden;
                resetAxisButton.text = $"Reset {selectedAxis} Axis";

                foreach (var curveEditor in _curveEditors)
                {
                    curveEditor.UpdateLockedAxis();
                }
            };
            
            lockX.RegisterValueChangedCallback(evt => { updateLocks(); });
            lockY.RegisterValueChangedCallback(evt => { updateLocks(); });
            lockZ.RegisterValueChangedCallback(evt => { updateLocks(); });
        }

        private void AddCurves()
        {
            if (_curvesContainer == null) return;
            
            for (int i = 1; i < spline.ControlPointCount; i += 3)
            {
                spline.EnforceMode(i);
                var bezierCurveEditor = new BezierCurveEditor(i - 1, serializedObject, curveUXML, RefreshCurves);
                _curveEditors.Add(bezierCurveEditor);
                var index = i;
                bezierCurveEditor.RegisterCallback<FocusInEvent>(evt =>
                {
                    selectedindexes.Clear();
                    selectedindexes.Add(index - 1);
                    selectedindexes.Add(index);
                    selectedindexes.Add(index + 1);
                    selectedindexes.Add(index + 2);
                    EditorUtility.SetDirty(spline);
                });
                bezierCurveEditor.RegisterCallback<FocusOutEvent>(evt =>
                {
                    selectedindexes.Remove(index - 1);
                    selectedindexes.Remove(index);
                    selectedindexes.Remove(index + 1);
                    selectedindexes.Remove(index + 2);
                    EditorUtility.SetDirty(spline);
                });
                
                bezierCurveEditor.UpdateLockedAxis();
                _curvesContainer.Add(bezierCurveEditor);
            }
        }

        void RefreshCurves()
        {
            foreach (var curveEditor in _curveEditors)
            {
                _curvesContainer.Remove(curveEditor);
            }
            
            _curveEditors.Clear();
            AddCurves();
        }
        

        private void OnSceneGUI()
        {
            spline = target as BezierSpline;

            CheckInput();

            handleTransform = spline.transform;

            handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? handleTransform.rotation
                : Quaternion.identity;


            for (int i = 1; i < spline.ControlPointCount; i += 3)
            {
                Vector3 p0 = ShowPoint(i - 1);
                Vector3 p1 = ShowPoint(i);
                Vector3 p2 = ShowPoint(i + 1);
                Vector3 p3 = ShowPoint(i + 2);

                Handles.color = new Color(0.2f, 0.2f, 0.2f);
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
            }
        }


        List<Vector3> changedPoints = new List<Vector3>();
        private VisualElement _root;
        private ScrollView _curvesContainer;

        private Vector3 ShowPoint(int index)
        {
            // if(spline.Points.Length < index) return Vector3.zero;
            
            Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point) * handleSize;

            if (index % 3 == 0)
            {
                size *= 1.5f;
                Handles.color = new Color(0f, 0.7f, 1);
            }
            else
            {
                Handles.color = modeColors[(int) spline.GetControlPointMode(index)];
                size *= 0.9f;
            }

            if (selectedindexes.Contains(index))
            {
                Handles.color = Color.red;
            }

            EditorGUI.BeginChangeCheck();

            if (Event.current.type == EventType.KeyDown && Event.current.button == 0)
            {
                if (GUIUtility.hotControl ==  spline.GetHashCode() + index)
                {
                    if (Event.current.keyCode == KeyCode.M)
                    {
                        spline.SetControlPointMode(index, BezierControlPointMode.Mirrored);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.A)
                    {
                        spline.SetControlPointMode(index, BezierControlPointMode.Aligned);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.F)
                    {
                        spline.SetControlPointMode(index, BezierControlPointMode.Free);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.D && index % 3 == 0)
                    {
                        spline.RemoveCurvePoint(index);
                        Event.current.Use();
                        serializedObject.UpdateIfRequiredOrScript();
                        RefreshCurves();
                        return Vector3.zero;
                    }
                }
            }
            
            Vector3 p = handleTransform.TransformPoint(spline.GetControlPoint(index));

            Vector3 newPos =
                #if UNITY_2022_3_OR_NEWER
                Handles.FreeMoveHandle(spline.GetHashCode() + index,p, size, Vector3.zero,
                    Handles.DotHandleCap);
 #else
                Handles.FreeMoveHandle(spline.GetHashCode() + index,p, Quaternion.identity,  size, Vector3.zero,
                    Handles.DotHandleCap);
                #endif

            Vector3 diff = newPos - p;
            
            if (spline.lockXAxis) diff.x = 0;
            if (spline.lockYAxis) diff.y = 0;
            if (spline.lockZAxis) diff.z = 0;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);

                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(p + diff));
            }
            
            if (diff.sqrMagnitude > float.Epsilon)
            {
                spline.RecalculateCurveLengths();
            }

            return point;
        }

        void CheckInput()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                Undo.RecordObject(spline, "Add Curve Point");
                
                float distanceFromCamera = Vector3.Distance(spline.transform.position, Camera.current.transform.position);

                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                Vector3 hitPoint = Vector3.zero;
                
                if (spline.lockXAxis)
                {
                    float t = (spline.transform.position.x - ray.origin.x) / ray.direction.x;
                    hitPoint = ray.GetPoint(t);
                }
                else if (spline.lockYAxis)
                {
                    float t = (spline.transform.position.y - ray.origin.y) / ray.direction.y;
                    hitPoint = ray.GetPoint(t);
                }
                else if (spline.lockZAxis)
                {
                    float t = (spline.transform.position.z - ray.origin.z) / ray.direction.z;
                    hitPoint = ray.GetPoint(t);
                }
                else
                {
                    GameObject foundObject = HandleUtility.PickGameObject(e.mousePosition, false);

                    if (foundObject != null)
                    {
                        MeshCollider addedComponent = null;
                        var component = foundObject.GetComponent<Collider>();
                        if (component == null)
                        {
                            addedComponent = foundObject.AddComponent<MeshCollider>();
                        }
                        
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, distanceFromCamera * 2))
                        {
                            hitPoint = hit.point;
                        }
                        
                        DestroyImmediate(addedComponent);
                    }
                    else
                    {
                        hitPoint = ray.GetPoint(distanceFromCamera);
                    }
                }
                
                var middle = spline.Points[spline.Points.Length - 1] + (spline.transform.InverseTransformPoint(hitPoint) - spline.Points[spline.Points.Length - 1]) * 0.5f ;
                
                spline.AddCurve(middle, 
                    middle,
                    spline.transform.InverseTransformPoint(hitPoint));
                
                serializedObject.UpdateIfRequiredOrScript();
                RefreshCurves();
            }
        }
    }
}