using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EasyTweens
{
    [Serializable]
    public class TweenCopy
    {
        public string TweenType;
        public string Json;
    }

    public class TweenEditor : VisualElement, IDisposable
    {
        private TweenBase tween;
        private Foldout mainFoldout;
        protected TweenAnimationEditor mainAnimationEditor;

        public TweenBase Tween => tween;

        private string DefaultFoldoutText =>
            _targetProperty == null || _targetProperty.objectReferenceValue == null
                ? ""
                : " \u279C " + _targetProperty.objectReferenceValue.name;

        private SerializedProperty _targetProperty;

        private SerializedProperty _startProp;

        private SerializedProperty _endProp;

        private SerializedProperty _curveProp;
        private SerializedProperty _curveRepeatsProp;

        private SerializedProperty _delayProp;

        private SerializedProperty _durationProp;

        private SerializedProperty _isUnfoldedProp;

        private SerializedProperty _nameOverrideProp;

        private DropFromHierarchyManipulator _fromHierarchyManipulator;

        private DragLinkManipulator _dragLinkManipulator;

        private VisualElement _delayDurationVisualization;

        private VisualElement _currentActivePhasePositionElement;

        private VisualElement _linkedDelayElement;

        private VisualElement _linkDragElement;

        protected SerializedObject _serializedObject;
        private TextField _overrideNameTextField;
        protected List<SerializedProperty> _additionalProperties = new List<SerializedProperty>();
        private SerializedProperty _selectedProperty;
        private IOrderedEnumerable<FieldInfo> _additionalParamsToExpose;

        private Label _indexLabel;
        private PropertyField _targetFiled;
        private PropertyField _startField;
        private PropertyField _endField;
        private CurveField _curveField;
        private VisualElement _curveRoot;
        private IntegerField _curveRepeatsField;
        private FloatField _delayField;
        private FloatField _durationField;

        public string FoldoutTitle => mainFoldout.text;

        public TweenEditor(TweenAnimationEditor animationEditor, TweenBase t, VisualTreeAsset visualTreeAsset,
            StyleSheet styleSheet)
        {
            tween = t;
            mainAnimationEditor = animationEditor;

            visualTreeAsset.CloneTree(this);

            this.styleSheets.Add(styleSheet);

            RegisterButtonCallbacks();

            _fromHierarchyManipulator = new DropFromHierarchyManipulator(this, mainAnimationEditor.Animation);
        }


        public void SyncFields()
        {
            _durationField.value = _durationProp.floatValue;
            _delayField.value = _delayProp.floatValue;
        }

        protected void RegisterButtonCallbacks()
        {
            Button setCurrentToStartButton = this.Q<Button>("SetCurrentToStart");
            if (setCurrentToStartButton != null)
            {
                setCurrentToStartButton.clickable.clicked += () =>
                {
                    Undo.RecordObject(mainAnimationEditor.Animation, "Set current value to start");
                    tween.SetCurrentAsStartValue();
                };
            }

            Button swapValues = this.Q<Button>("SwapValues");
            if (swapValues != null)
            {
                swapValues.clickable.clicked += () =>
                {
                    Undo.RecordObject(mainAnimationEditor.Animation, "Swap values");
                    tween.SwapValues();
                };
            }

            Button setCurrentToEndButton = this.Q<Button>("SetCurrentToEnd");
            if (setCurrentToEndButton != null)
            {
                setCurrentToEndButton.clickable.clicked += () =>
                {
                    Undo.RecordObject(mainAnimationEditor.Animation, "Set current value to end");
                    tween.SetCurrentAsEndValue();
                };
            }

            Button removeTween = this.Q<Button>("RemoveTween");
            removeTween.clickable.clicked += () => { mainAnimationEditor.RemoveTween(this); };

            Button duplicateTween = this.Q<Button>("DuplicateTween");
            duplicateTween.clickable.clicked += () => { mainAnimationEditor.DuplicateTween(this); };

            RegisterCallback<MouseEnterEvent>(evt =>
            {
                if ((evt.pressedButtons & (1 << (int)MouseButton.RightMouse)) > 0)
                {
                    _indexLabel.style.display = DisplayStyle.Flex;
                }
            }, TrickleDown.TrickleDown);

            RegisterCopyPastOnRightButton();
        }
        private void RegisterCopyPastOnRightButton()
        {
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if ((evt.pressedButtons & (1 << (int)MouseButton.RightMouse)) > 0 &&
                    ((VisualElement)evt.target).ClassListContains("unity-foldout__input"))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy"), false, () =>
                    {
                        var json = JsonUtility.ToJson(new TweenCopy
                        {
                            Json = JsonUtility.ToJson(tween),
                            TweenType = tween.GetType().Name
                        });
                        EditorGUIUtility.systemCopyBuffer = json;
                    });

                    menu.AddItem(new GUIContent("Paste"), false, () =>
                    {
                        var json = EditorGUIUtility.systemCopyBuffer;
                        var tweenCopy = JsonUtility.FromJson<TweenCopy>(json);
                        if (tweenCopy == null)
                        {
                            Debug.LogError("Invalid copy data");
                        }
                        else if (tweenCopy.TweenType != tween.GetType().Name)
                        {
                            Debug.LogError("Invalid copy data. Tween type mismatch. Destination: " +
                                tween.GetType().Name +
                                " Source: " + tweenCopy.TweenType);
                        }
                        else
                        {
                            JsonUtility.FromJsonOverwrite(tweenCopy.Json, tween);
                        }
                    });
                    menu.ShowAsContext();
                }
            }, TrickleDown.NoTrickleDown);
        }

        private void SetFoldoutText(string goName)
        {
            string title = goName;

            if (!string.IsNullOrEmpty(tween.NameOverride))
            {
                title = string.Format(tween.NameOverride, goName);
            }
            else
            {
                var tweenName = tween.GetType().Name;
                tweenName = tweenName.Replace("Tween", "");

                for (int i = 1; i < tweenName.Length; i++)
                {
                    if (char.IsUpper(tweenName[i]) && tweenName[i - 1] != ' ')
                    {
                        tweenName = tweenName.Insert(i, " ");
                    }
                }

                title = tweenName + goName;
            }

            if (_targetProperty != null && _targetProperty.objectReferenceValue == null)
            {
                title += " <color=#FF0000>(Missing Target)</color>";
            }

            mainFoldout.text = title;
        }

        public void BindProperties()
        {
            _targetFiled = this.Q<PropertyField>("TargetField");
            _startField = this.Q<PropertyField>("StartValue");
            _endField = this.Q<PropertyField>("EndValue");
            _curveField = this.Q<CurveField>("Curve");
            _curveRoot = this.Q<VisualElement>("CurveRoot");
            _curveRepeatsField = this.Q<IntegerField>("CurveRepeats");
            _delayField = this.Q<FloatField>("Delay");
            _durationField = this.Q<FloatField>("Duration");
            _delayDurationVisualization = this.Q<VisualElement>("DelayDurationVisualization");
            _currentActivePhasePositionElement = this.Q<VisualElement>("CurrentActivePhase");
            _linkedDelayElement = this.Q<VisualElement>("LinkedDelay");
            _linkDragElement = this.Q<VisualElement>("LinkDragger");
            mainFoldout = this.Q<Foldout>("MainFoldout");
            _indexLabel = this.Q<Label>("IndexLabel");
            if (_indexLabel != null)
            {
                _indexLabel.style.display = DisplayStyle.None;
                _indexLabel.text = (mainAnimationEditor.Animation.tweens.IndexOf(tween) + 1).ToString();
            }

            _serializedObject = mainAnimationEditor.serializedObject;

            _targetProperty = GetProperty(_serializedObject, "target");
            _startProp = GetProperty(_serializedObject, "startValue");
            _endProp = GetProperty(_serializedObject, "endValue");
            _curveProp = GetProperty(_serializedObject, "curve");
            _curveRepeatsProp = GetProperty(_serializedObject, "curveRepeats");
            _delayProp = GetProperty(_serializedObject, "delay");
            _durationProp = GetProperty(_serializedObject, "duration");
            _isUnfoldedProp = GetProperty(_serializedObject, "isUnfolded");
            _nameOverrideProp = GetProperty(_serializedObject, "NameOverride");

            mainFoldout.value = _isUnfoldedProp.boolValue;
            mainFoldout.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.target == mainFoldout)
                {
                    tween.isUnfolded = evt.newValue;
                    _isUnfoldedProp.boolValue = evt.newValue;
                }
            });

            if (_targetProperty != null)
            {
                _targetFiled.BindProperty(_targetProperty);
                _targetFiled.RegisterValueChangeCallback(evt =>
                {
                    if (evt.changedProperty.objectReferenceValue != null)
                    {
                        Undo.RecordObject(mainAnimationEditor.Animation, "Change Target");
                        BindAdditionalParams();
                        _fromHierarchyManipulator.ResetColor();
                    }
                });
                _curveField.BindProperty(_curveProp);
                _curveRepeatsField.BindProperty(_curveRepeatsProp);
                _curveRepeatsField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(mainAnimationEditor.Animation, "Change Curve Repeats");
                    if (evt.newValue < 1)
                    {
                        tween.curveRepeats = 1;
                    }
                });
            }
            else
            {
                _durationField.visible = false;
                _curveRoot.visible = false;
                _targetFiled.visible = false;
            }

            if (_startField != null)
            {
                SetupStartFieldCallbacks();
            }

            if (_endField != null)
            {
                SetupEndFieldCallbacks();
            }

            _durationField.SetValueWithoutNotify(tween.Duration);
            _durationField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                Undo.RecordObject(mainAnimationEditor.Animation, "Change Duration");

                if (evt.newValue < 0)
                    tween.Duration = 0;

                tween.Duration = evt.newValue;

                UpdateDelayDurationVisualization();
            });

            _delayField.SetValueWithoutNotify(tween.Delay);
            _delayField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                Undo.RecordObject(mainAnimationEditor.Animation, "Change Delay");

                if (-evt.newValue > tween.LinkedTweenDelay)
                {
                    tween.Delay = -tween.LinkedTweenDelay;
                    _delayField.SetValueWithoutNotify(-tween.LinkedTweenDelay);
                }
                else
                    tween.Delay = evt.newValue;

                UpdateDelayDurationVisualization();
            });

            Button setCurrentToEndButton = this.Q<Button>("SetCurrentToDelay");
            setCurrentToEndButton.clickable.clicked += () =>
            {
                Undo.RecordObject(mainAnimationEditor.Animation, "Set current time to delay");
                tween.Delay = mainAnimationEditor.Animation.currentTime - (tween.TotalDelay - tween.Delay);
                _delayField.SetValueWithoutNotify(tween.Delay);
            };

            _targetFiled.RegisterCallback<ChangeEvent<Object>>(ev =>
            {
                SetFoldoutText(ev != null && ev.newValue != null ? " \u279C " + ev.newValue.name : "");
            });

            Toggle tweenSelection = this.Q<Toggle>("TweenSelection");
            _selectedProperty = GetProperty(_serializedObject, "isSelected");
            tweenSelection.BindProperty(_selectedProperty);
            tweenSelection.RegisterValueChangedCallback(evt =>
            {
                mainAnimationEditor.TweenSelectionChanged(this, evt.newValue);
            });

            _overrideNameTextField = this.Q<TextField>("OverrideNameField");
            _overrideNameTextField.visible = false;
            _overrideNameTextField.BindProperty(_nameOverrideProp);
            _overrideNameTextField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(mainAnimationEditor.Animation, "Change Name Override");
                SetFoldoutText(DefaultFoldoutText);
            });
            _overrideNameTextField.RegisterCallback<FocusOutEvent>(evt => { _overrideNameTextField.visible = false; });

            var editNameButton = this.Q<Button>("EditNameButton");
            editNameButton.clickable.clicked += () =>
            {
                if (_overrideNameTextField.visible)
                {
                    _overrideNameTextField.visible = false;
                    SetFoldoutText(DefaultFoldoutText);
                }
                else
                {
                    _overrideNameTextField.visible = true;
                    EditorApplication.delayCall += FocusOnTextField;
                }
            };

            BindAdditionalParams();
        }

        public virtual void OnBaseEditorConstructed() { }

        private void SetupEndFieldCallbacks()
        {
            if (_endProp != null)
            {
                _endField.BindProperty(_endProp);
                _endField.RegisterValueChangeCallback(evt =>
                    Undo.RecordObject(mainAnimationEditor.Animation, "Change End Value"));
            }
            else
            {
                this.Q<Button>("SetCurrentToEnd").visible = false;
                _endField.visible = false;
            }
        }
        private void SetupStartFieldCallbacks()
        {
            if (_startProp != null)
            {
                _startField.BindProperty(_startProp);
                _startField.RegisterValueChangeCallback(evt =>
                    Undo.RecordObject(mainAnimationEditor.Animation, "Change Start Value"));
            }
            else
            {
                this.Q<Button>("SetCurrentToStart").visible = false;
                this.Q<Button>("SwapValues").visible = false;
                _startField.visible = false;
            }
        }

        private void FocusOnTextField()
        {
            EditorApplication.delayCall -= FocusOnTextField;
            _overrideNameTextField.Focus();
            _overrideNameTextField.SelectAll();
        }

        protected SerializedProperty GetProperty(SerializedObject so, string propName)
        {
            return so.FindProperty("tweens").GetArrayElementAtIndex(mainAnimationEditor.Animation.tweens.IndexOf(tween))
                .FindPropertyRelative(propName);
        }

        public bool UpdateDelayDurationVisualization()
        {
            if (float.IsNaN(_delayDurationVisualization.parent.layout.width)) return false;

            var animationDuration = mainAnimationEditor.Animation.duration;
            if (animationDuration == 0)
                animationDuration = 0.1f;

            var newMaxValue = (tween.TotalDelay + tween.Duration) / animationDuration;
            var newMinValue = tween.TotalDelay / animationDuration;

            _currentActivePhasePositionElement.style.left =
                _delayDurationVisualization.style.left = newMinValue * _delayDurationVisualization.parent.layout.width;

            var newWidth = (newMaxValue - newMinValue) * _delayDurationVisualization.parent.layout.width;
            newWidth = Mathf.Max(5, newWidth);
            _currentActivePhasePositionElement.style.width = _delayDurationVisualization.style.width = newWidth;

            var linkedDelayMaxVal = (tween.LinkedTweenDelay + Mathf.Min(0, tween.Delay)) / animationDuration;
            _linkedDelayElement.style.width = _delayDurationVisualization.parent.layout.width * linkedDelayMaxVal;

            _linkDragElement.style.backgroundColor = string.IsNullOrEmpty(tween.LinkedTweenGuid)
                ? new Color(0.64f, 0.64f, 0.64f)
                : new Color(0.9f, 0.5f, 0.2f);

            return true;
        }

        public void UpdateCurrentActivePhasePosition(float currTime)
        {
            if (tween.Duration == 0)
            {
                _currentActivePhasePositionElement.style.scale = new Scale(
                    (currTime > 0 && currTime >= tween.TotalDelay) ? Vector3.one : Vector3.zero);
                return;
            }

            var currFactor = Mathf.Clamp01((currTime - tween.TotalDelay) / tween.Duration);
            _currentActivePhasePositionElement.style.scale = new Scale(new Vector3(currFactor, 1, 1));
        }

        void BindAdditionalParams()
        {
            foreach (var additionalProperty in _additionalProperties)
            {
                additionalProperty.Dispose();
            }

            _additionalProperties.Clear();

            if (_additionalParamsToExpose == null)
                _additionalParamsToExpose = GetTweenAdditionalParamsToExpose();

            var additionalPropertiesRoot = this.Q<VisualElement>("AdditionalProperties");
            additionalPropertiesRoot.Clear();

            foreach (var field in _additionalParamsToExpose)
            {
                var serializedProperty = GetProperty(mainAnimationEditor.serializedObject, field.Name);
                _additionalProperties.Add(serializedProperty);
                var exposeAttribute = field.GetCustomAttribute(typeof(ExposeInEditorAttribute), false);
#if UNITY_2021_1_OR_NEWER

                var shaderAttribute =
                    (ShaderPropertyAttribute)field.GetCustomAttribute(typeof(ShaderPropertyAttribute), false);

                if (field.FieldType == typeof(string) && shaderAttribute != null)
                {
                    DropdownField dropdownField = new DropdownField(serializedProperty.displayName);
                    dropdownField.AddToClassList("customLabel");

                    List<string> paramNames = new List<string>();
                    if (_targetProperty.objectReferenceValue != null)
                    {
                        List<string> choices = new List<string>();
                        Material material = null;

                        if (_targetProperty.objectReferenceValue is Renderer renderer)
                        {
                            material = renderer.sharedMaterial;
                        }
                        else if (_targetProperty.objectReferenceValue is TMP_Text tmp)
                        {
                            material = tmp.fontSharedMaterial;
                        }
                        var propertyCount = material.shader.GetPropertyCount();
                        for (int i = 0; i < propertyCount; i++)
                        {
                            var shaderPropertyType = material.shader.GetPropertyType(i);
                            if (shaderAttribute.Types.Contains(shaderPropertyType))
                            {
                                var propertyAttributes = material.shader.GetPropertyAttributes(i);
                                var attributes = propertyAttributes.Length > 0
                                    ? "[" + string.Join(",", propertyAttributes) + "]  "
                                    : "";
                                var propertyDescription =
                                    attributes + material.shader.GetPropertyDescription(i);

                                var propertyName = material.shader.GetPropertyName(i);
                                if (shaderPropertyType == ShaderPropertyType.Texture)
                                {
                                    propertyName += "_ST";
                                }
                                else if (shaderPropertyType == ShaderPropertyType.Range)
                                {
                                    propertyDescription = propertyDescription + " " +
                                        material.shader.GetPropertyRangeLimits(i);
                                }

                                choices.Add(propertyName + " : " + propertyDescription);
                                paramNames.Add(propertyName);
                            }
                        }

                        if (choices.Count == 0)
                        {
                            choices.Add("No properties found");
                        }

                        dropdownField.choices = choices;
                        dropdownField.index = paramNames.IndexOf(serializedProperty.stringValue);
                        dropdownField.RegisterValueChangedCallback(evt =>
                        {
                            Undo.RecordObject(mainAnimationEditor.Animation, "Changed additional property");

                            var selectedName = paramNames[dropdownField.index];
                            for (int i = 0; i < propertyCount; i++)
                            {
                                if (material.shader.GetPropertyName(i) == selectedName)
                                {
                                    if (material.shader.GetPropertyType(i) == ShaderPropertyType.Color)
                                    {
                                        var fieldInfo = Tween.GetType().GetField("useHDRColor");

                                        var needHDRColor = material.shader.GetPropertyAttributes(i)
                                            .Contains("HDR");
                                        fieldInfo?.SetValue(Tween, needHDRColor);
                                    }

                                    break;
                                }
                            }

                            serializedProperty.stringValue = selectedName;
                            serializedProperty.serializedObject.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        dropdownField.choices = new List<string>
                        {
                            "Current target is null"
                        };
                    }

                    additionalPropertiesRoot.Add(dropdownField);
                }
                else
#endif
                {
                    PropertyField propertyField = new PropertyField(serializedProperty);
                    propertyField.BindProperty(serializedProperty);
                    propertyField.name = serializedProperty.name;

                    propertyField.AddToClassList("customLabel");
                    var exposeInEditorAttribute = (ExposeInEditorAttribute)exposeAttribute;
                    propertyField.style.fontSize = exposeInEditorAttribute != null
                        ? exposeInEditorAttribute.FontSize
                        : 12;

                    propertyField.tooltip = exposeInEditorAttribute != null
                        ? exposeInEditorAttribute.Tooltip
                        : "";

                    additionalPropertiesRoot.Add(propertyField);
                }
            }

            if (_dragLinkManipulator != null && _dragLinkManipulator.target != null)
            {
                _dragLinkManipulator.target.RemoveManipulator(_dragLinkManipulator);
            }
            _dragLinkManipulator = new DragLinkManipulator(_linkDragElement, this, mainAnimationEditor);
            OnBindingsUpdated();
        }

        private IOrderedEnumerable<FieldInfo> GetTweenAdditionalParamsToExpose()
        {
            return tween.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(info => info.IsDefined(typeof(ExposeInEditorAttribute), false))
                .OrderBy(info =>
                {
                    var customAttribute =
                        (ExposeInEditorAttribute)info.GetCustomAttribute(typeof(ExposeInEditorAttribute), false);
                    if (customAttribute == null)
                        return 0;
                    return customAttribute.Order;
                });
        }

        public virtual void OnBindingsUpdated() { }

        public virtual void Dispose()
        {
            _dragLinkManipulator.target.RemoveManipulator(_dragLinkManipulator);
            _fromHierarchyManipulator.target.RemoveManipulator(_fromHierarchyManipulator);
            _serializedObject = null;
            mainFoldout.Unbind();
            _targetProperty?.Dispose();
            _startProp?.Dispose();
            _endProp?.Dispose();
            _curveProp?.Dispose();
            _delayProp?.Dispose();
            _durationProp?.Dispose();
            _isUnfoldedProp?.Dispose();
            _nameOverrideProp?.Dispose();
            _selectedProperty?.Dispose();

            foreach (var additionalProperty in _additionalProperties)
            {
                additionalProperty?.Dispose();
            }

            _additionalProperties.Clear();
        }
    }
}