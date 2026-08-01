using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyTweens.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace EasyTweens
{
    public class AlternativeEditor
    {
        public Type tweenType;
        public VisualTreeAsset tweenAsset;
    }

    [CustomEditor(typeof(TweenAnimation))]
    public class TweenAnimationEditor : Editor
    {
        public VisualTreeAsset MainVisualTreeAsset;
        public StyleSheet MainStylesheet;
        public VisualTreeAsset TweenVisualTreeAsset;
        public StyleSheet TweenStylesheet;

        private VisualElement rootElement;
        private TweenAnimation animation;

        public TweenAnimation Animation => animation;

        private List<TweenEditor> tweenEditors = new List<TweenEditor>();
        private List<TweenEditor> selectedTweenEditors = new List<TweenEditor>();

        private Action<Vector2> factorUpdateAction;
        private VisualElement _selectionActionsRoot;

        private Button playForwardSelectedButton;
        private Button playBackwardSelectedButton;
        private Button returnFromWindowedModeButton;

        List<ReorderDragAndDropManipulator> _manipulators = new List<ReorderDragAndDropManipulator>();
        DropFromHierarchyAndCreateNewTween _dropFromHierarchyAndCreateNewTweenManipulator;
        private Button _showAllButton;
        private SerializedProperty _currentTimeProperty;
        private SerializedProperty _playOnAwakeProperty;
        private SerializedProperty _restartOnEnableProperty;
        private SerializedProperty _ignoreTimeScaleProp;
        private SerializedProperty _allowCustomDurationProp;
        private SerializedProperty _timeSpeedProperty;
        private SerializedProperty _animDurationProperty;
        private Button _stopButton;
        private Button _resumeButton;

        float lastResumeTime = -1;
        int lastResumeDirectionMultiplier;
        private bool _needRefreshDelayDurationVisualization;
        private Button _resetFilterButton;

        public List<TweenEditor> TweenEditors => tweenEditors;

        public bool IsInWindow { get; set; }

        public void OnEnable()
        {
            if (target == null) return;

            EditorApplication.update += Update;

            animation = (TweenAnimation)target;
            if (animation.serializationVersion != SerializationVersion.Version)
            {
                Updater.Update(animation);
            }

            animation.editorSubscriptionRetainCount = 0;
            animation.SubscribeToEditorUpdates();
            if (rootElement == null)
                rootElement = new VisualElement();

            // Load in UXML template and USS styles, then apply them to the root element.
            MainVisualTreeAsset.CloneTree(rootElement);

            rootElement.styleSheets.Add(MainStylesheet);

            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void Update()
        {
            if (animation.HasDirtyTweens() || _needRefreshDelayDurationVisualization)
            {
                RefreshDelayDurationsHandles();
            }
        }

        public void OnDisable()
        {
            EditorApplication.update -= Update;

            if (animation == null) return;
            animation.UnsubscribeFromEditorUpdates();
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            ClearManipulators();
            DisposeEditors();

            rootElement.Clear();

            _currentTimeProperty?.Dispose();
            _playOnAwakeProperty?.Dispose();
            _ignoreTimeScaleProp?.Dispose();
            _timeSpeedProperty?.Dispose();
            _animDurationProperty?.Dispose();
            _allowCustomDurationProp?.Dispose();
        }

        private void ClearManipulators()
        {
            foreach (ReorderDragAndDropManipulator manipulator in _manipulators)
            {
                manipulator.target.RemoveManipulator(manipulator);
            }

            _manipulators.Clear();
        }

        private void UndoRedoPerformed()
        {
            if (animation == null)
            {
                Undo.undoRedoPerformed -= UndoRedoPerformed;
            }

            serializedObject.UpdateIfRequiredOrScript();
            if (tweenEditors.Count != animation.tweens.Count)
            {
                FillTweenEditors();
            }
            else
            {
                for (int i = 0; i < tweenEditors.Count; i++)
                {
                    tweenEditors[i].SyncFields();
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (animation == null || CheckIfEditorAlreadyOpenedInWindow()) return rootElement;

            AddPlayButtons();

            SetupCommonTweenParams();

            _selectionActionsRoot = rootElement.Q<VisualElement>("SelectionActions");
            _selectionActionsRoot.style.display = DisplayStyle.None;
            FillTweenEditors();
            SetupSelectedTweensActions();
            UpdateSelectedTweensActions();

            FillTweenClassesForAddButton();

            var dropFromHierarchyTarget = rootElement.Q<VisualElement>("DropNewTweenTarget");
            _dropFromHierarchyAndCreateNewTweenManipulator =
                new DropFromHierarchyAndCreateNewTween(dropFromHierarchyTarget, this);

            rootElement.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                foreach (var tweenEditor in tweenEditors)
                {
                    tweenEditor.UpdateDelayDurationVisualization();
                    tweenEditor.UpdateCurrentActivePhasePosition(animation.currentTime);
                }
            });

            return rootElement;
        }

        private void SetupCommonTweenParams()
        {
            _restartOnEnableProperty = serializedObject.FindProperty("restartOnEnable");
            Toggle restartOnEnableToggle = rootElement.Q<Toggle>("RestartOnEnable");
            restartOnEnableToggle.BindProperty(_restartOnEnableProperty);
            restartOnEnableToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue != animation.restartOnEnable)
                {
                    Undo.RecordObject(animation, "Restart On Enable");
                    EditorUtility.SetDirty(animation);
                }
                
                var animationObj = (MonoBehaviour)serializedObject.targetObject;
                var restartComponent = animationObj.GetComponent<RestartAnimationOnEnabled>();
                if (evt.newValue && !restartComponent)
                {
                    animationObj.gameObject.AddComponent<RestartAnimationOnEnabled>();
                }
                else if (!evt.newValue && restartComponent)
                {
                    Undo.DestroyObjectImmediate(restartComponent);
                }
            });
            
            _playOnAwakeProperty = serializedObject.FindProperty("playOnAwake");
            Toggle playOnAwakeToggle = rootElement.Q<Toggle>("PlayOnAwake");
            playOnAwakeToggle.BindProperty(_playOnAwakeProperty);
            playOnAwakeToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue != animation.playOnAwake)
                {
                    Undo.RecordObject(animation, "Play On Awake");
                    EditorUtility.SetDirty(animation);
                }
                restartOnEnableToggle.SetEnabled(evt.newValue);
            });
            
            _ignoreTimeScaleProp = serializedObject.FindProperty("ignoreTimeScale");
            Toggle ignoreTimeScaleToggle = rootElement.Q<Toggle>("IgnoreTimeScale");
            ignoreTimeScaleToggle.BindProperty(_ignoreTimeScaleProp);
            ignoreTimeScaleToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue != animation.ignoreTimeScale)
                {
                    Undo.RecordObject(animation, "Ignore Time Scale");
                    EditorUtility.SetDirty(animation);
                }
            });

            _timeSpeedProperty = serializedObject.FindProperty("timeSpeedMultiplier");
            FloatField timeSpeedField = rootElement.Q<FloatField>("DurationScale");
            timeSpeedField.isReadOnly = false;
            timeSpeedField.BindProperty(_timeSpeedProperty);

            _animDurationProperty = serializedObject.FindProperty("duration");
            FloatField durationField = rootElement.Q<FloatField>("Duration");
            durationField.BindProperty(_animDurationProperty);
            durationField.isDelayed = true;
            durationField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                RefreshDelayDurationsHandles();
                if (!Mathf.Approximately(evt.newValue, animation.duration))
                {
                    Undo.RecordObject(animation, "Duration");
                    EditorUtility.SetDirty(animation);
                }
            });

            _allowCustomDurationProp = serializedObject.FindProperty("allowCustomAnimationDuration");
            Toggle allowCustomDuration = rootElement.Q<Toggle>("AllowCustomDuration");
            allowCustomDuration.BindProperty(_allowCustomDurationProp);
            allowCustomDuration.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue != animation.allowCustomAnimationDuration)
                {
                    Undo.RecordObject(animation, "Allow Custom Duration");
                    EditorUtility.SetDirty(animation);
                }

                if (animation.allowCustomAnimationDuration)
                {
                    durationField.isReadOnly = false;
                    durationField.style.opacity = 1;
                    durationField.focusable = true;
                }
                else
                {
                    durationField.isReadOnly = true;
                    durationField.style.opacity = 0.5f;
                    durationField.focusable = false;
                    animation.duration = 0;
                    animation.EnsureDuration();
                }
            });

            SetupProgressSlider();

            EnumField loopEnum = rootElement.Q<EnumField>("LoopType");
            loopEnum.Init(animation.lootType);
            loopEnum.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                if ((LoopType)evt.newValue != animation.lootType)
                {
                    animation.lootType = (LoopType)evt.newValue;
                    EditorUtility.SetDirty(animation);
                }
            });

            _showAllButton = rootElement.Q<Button>("ShowAll");
            _showAllButton.clickable.clicked += () =>
            {
                for (int i = 0; i < animation.tweens.Count; i++)
                {
                    animation.tweens[i].isHidden = false;
                }
                var filterText = rootElement.Q<TextField>("FilterText");
                filterText.value = "";

                FillTweenEditors();
            };

            _resetFilterButton = rootElement.Q<Button>("ResetFilter");
            _resetFilterButton.clickable.clicked += () =>
            {
                var filterText = rootElement.Q<TextField>("FilterText");
                filterText.value = "";
            };
        }

        private bool CheckIfEditorAlreadyOpenedInWindow()
        {
            if (!IsInWindow && EditorWindow.HasOpenInstances<AnimationUtilityWindow>())
            {
                var window = AnimationUtilityWindow.GetWindow<AnimationUtilityWindow>();
                if (window.tweenAnimation == this.animation)
                {
                    EnterWindowedMode(window);
                    return true;
                }
            }

            return false;
        }

        private void EnterWindowedMode(AnimationUtilityWindow window)
        {
            OnDisable();

            returnFromWindowedModeButton = new Button(() =>
            {
                if (window.IsPinned)
                {
                    window.Clear();
                    ShowEditorContent();
                }
                else
                {
                    window.Close();
                }
            });

            returnFromWindowedModeButton.text = "Return from windowed mode";
            rootElement.Add(returnFromWindowedModeButton);

            window.OnDestroyEvent += ShowEditorContent;
        }

        private void ShowEditorContent(AnimationUtilityWindow window = null)
        {
            if (window != null)
            {
                window.OnDestroyEvent -= ShowEditorContent;
            }

            if (returnFromWindowedModeButton != null && rootElement.Contains(returnFromWindowedModeButton))
            {
                rootElement.Remove(returnFromWindowedModeButton);
            }

            returnFromWindowedModeButton = null;

            EditorApplication.delayCall += ReloadEditor;
        }

        private void ReloadEditor()
        {
            EditorApplication.delayCall -= ReloadEditor;
            OnEnable();
            CreateInspectorGUI();
        }

        public void RefreshDelayDurationsHandles()
        {
            _needRefreshDelayDurationVisualization = false;
            animation.RecalculateDelays();
            animation.EnsureDuration();
            for (int i = 0; i < tweenEditors.Count; i++)
            {
                bool success = tweenEditors[i].UpdateDelayDurationVisualization();
                _needRefreshDelayDurationVisualization |= !success;
            }

            if (!_needRefreshDelayDurationVisualization)
            {
                for (int i = 0; i < tweenEditors.Count; i++)
                {
                    tweenEditors[i].UpdateCurrentActivePhasePosition(animation.currentTime);
                }
            }
        }

        private void AddPlayButtons()
        {
            _stopButton = rootElement.Q<Button>("Stop");
            _stopButton.clickable.clicked += () => { animation.Pause(); };

            _resumeButton = rootElement.Q<Button>("Resume");
            _resumeButton.clickable.clicked += () =>
            {
                if (lastResumeTime > 0)
                {
                    animation.SetTime(lastResumeTime, lastResumeTime - animation.currentTime);
                    animation.currentTime = lastResumeTime;
                    animation.directionMultiplier = lastResumeDirectionMultiplier;
                }
                else
                {
                    lastResumeTime = animation.currentTime;
                    lastResumeDirectionMultiplier = animation.directionMultiplier;
                }

                animation.Resume();
            };

            Button playForward = rootElement.Q<Button>("PlayForward");
            playForward.clickable.clicked += () =>
            {
                lastResumeTime = -1;

                if (animation.editorSubscriptionRetainCount == 0)
                    animation.SubscribeToEditorUpdates();

                animation.Play();
            };

            Button playFastForward = rootElement.Q<Button>("PlayFastForward");
            playFastForward.clickable.clicked += () =>
            {
                EditorUtility.SetDirty(animation);
                lastResumeTime = -1;
                animation.Play(false);
            };

            Button playBackward = rootElement.Q<Button>("PlayBackward");
            playBackward.clickable.clicked += () =>
            {
                lastResumeTime = -1;
                animation.PlayBackward();
            };

            Button playFastBackward = rootElement.Q<Button>("PlayFastBackward");
            playFastBackward.clickable.clicked += () =>
            {
                EditorUtility.SetDirty(animation);
                lastResumeTime = -1;
                animation.PlayBackward(false);
            };

            Button separateWindowButton = rootElement.Q<Button>("OpenUtilityWindow");
            separateWindowButton.clickable.clicked += () =>
            {
                var window = AnimationUtilityWindow.ShowWindow(this, true);
                EnterWindowedMode(window);
            };

            Button separatePinableWindowButton = rootElement.Q<Button>("OpenUtilityWindowPinnable");
            separatePinableWindowButton.clickable.clicked += () =>
            {
                var window = AnimationUtilityWindow.ShowWindow(this);
                EnterWindowedMode(window);
            };

            playForwardSelectedButton = rootElement.Q<Button>("PlayForwardSelected");
            playForwardSelectedButton.style.display = DisplayStyle.None;

            playBackwardSelectedButton = rootElement.Q<Button>("PlayBackwardSelected");
            playBackwardSelectedButton.style.display = DisplayStyle.None;

            playForwardSelectedButton.clickable.clicked += () => { animation.Play(true, true); };

            playBackwardSelectedButton.clickable.clicked += () => { animation.PlayBackward(true, true); };
        }

        private void SetupProgressSlider()
        {
            Slider factorSlider = rootElement.Q<Slider>("FactorSlider");
            FloatField currentTime = rootElement.Q<FloatField>("HiddenCurrentTime");
            _currentTimeProperty = serializedObject.FindProperty("currentTime");
            currentTime.BindProperty(_currentTimeProperty);

            currentTime.RegisterValueChangedCallback(evt =>
            {
                factorSlider.value = evt.newValue / animation.duration;
                foreach (var te in tweenEditors)
                {
                    te.UpdateCurrentActivePhasePosition(factorSlider.value * animation.duration);
                }
            });

            factorSlider.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                if (factorSlider.focusController.focusedElement == factorSlider)
                {
                    lastResumeTime = -1;
                    float factor = evt.newValue;

                    var deltaTime = factor * animation.duration - animation.currentTime;
                    animation.directionMultiplier = (int)Mathf.Sign(deltaTime);
                    animation.SetTime(factor * animation.duration, deltaTime);
                    animation.currentTime = factor * animation.duration;
                }
            });
        }

        private void FillTweenEditors()
        {
            DisposeEditors();

            VisualElement tweensContainer = rootElement.Q<VisualElement>("TweensContainer");
            tweensContainer.Clear();
            ClearManipulators();
            _needRefreshDelayDurationVisualization = true;

            for (int i = 0; i < animation.tweens.Count; i++)
            {
                try
                {
                    if (animation.tweens[i].isHidden) continue;
                    TweenEditor te = null;
                    if (animation.tweens[i].GetType().IsDefined(typeof(UseCustomEditorAttribute), true))
                    {
                        var customAttribute = (UseCustomEditorAttribute)animation.tweens[i].GetType()
                            .GetCustomAttribute(typeof(UseCustomEditorAttribute), true);

                        var visualAsset = TweenVisualTreeAsset;
                        var alternativeVisuals = TweenEditorTypeCache.AlternativeEditors
                            .FirstOrDefault(editor => editor.tweenType == animation.tweens[i].GetType())
                            ?.tweenAsset;
                        if (alternativeVisuals != null)
                        {
                            visualAsset = alternativeVisuals;
                        }
                        te = (TweenEditor)Activator.CreateInstance(Type.GetType("EasyTweens." + customAttribute.Type),
                            this, animation.tweens[i], visualAsset, TweenStylesheet);
                    }

                    if (te == null)
                        te = new TweenEditor(this, animation.tweens[i], TweenVisualTreeAsset, TweenStylesheet);

                    te.BindProperties();
                    tweensContainer.Add(te);
                    tweenEditors.Add(te);
                    ReorderDragAndDropManipulator manipulator = new ReorderDragAndDropManipulator(te, this);
                    _manipulators.Add(manipulator);
                    if (te.Tween.isSelected)
                    {
                        selectedTweenEditors.Add(te);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    //show editor window with error
                    if (EditorUtility.DisplayDialog("TweenAnimation: Error Loading Tween Editor",
                            "Error loading editor for tween at index " +
                            i +
                            "\n\n" +
                            e.Message +
                            "\n\n Do you want to remove broken tween?",
                            "Yes", "No"))
                    {
                        animation.tweens.RemoveAt(i);
                        i--;
                    }
                }
            }

            UpdateSelectedTweensActions();
        }

        private void DisposeEditors()
        {
            foreach (var editor in tweenEditors)
            {
                editor.Dispose();
            }

            tweenEditors.Clear();
            selectedTweenEditors.Clear();
        }

        void FillTweenClassesForAddButton()
        {
            var tweenTypes = TweenEditorTypeCache.TweenTypes;
            var availableTweenNames = TweenEditorTypeCache.AvailableTweenNames;

            var addButton = rootElement.Q<Button>("AddTween");
            addButton.RegisterCallback<ClickEvent>(evt =>
            {
                GenericMenu menu = new GenericMenu();
                for (var i = 0; i < availableTweenNames.Count; i++)
                {
                    var choice = availableTweenNames[i];
                    int index = i;
                    menu.AddItem(new GUIContent(choice), false, () => { AddTween(tweenTypes[index]); });
                }

                menu.ShowAsContext();
            });
        }

        public TweenBase AddTween(Type tweenType)
        {
            Undo.RecordObjects(new Object[]
            {
                animation,
                this
            }, "Add Tween");
            TweenBase tween = (TweenBase)Activator.CreateInstance(tweenType);
            tween.curve = AnimationCurve.Linear(0, 0, 1, 1);
            animation.tweens.Add(tween);
            serializedObject.UpdateIfRequiredOrScript();
            FillTweenEditors();
            return tween;
        }

        public void RemoveTween(TweenEditor te)
        {
            Undo.RecordObjects(new Object[]
            {
                animation,
                this
            }, "Remove Tween");
            animation.tweens.Remove(te.Tween);
            serializedObject.UpdateIfRequiredOrScript();
            FillTweenEditors();
        }

        public void DuplicateTween(TweenEditor te)
        {
            Undo.RecordObjects(new Object[]
            {
                animation,
                this
            }, "Duplicate Tween");
            TweenBase copy = (TweenBase)Activator.CreateInstance(te.Tween.GetType());

            var json = JsonUtility.ToJson(te.Tween);
            JsonUtility.FromJsonOverwrite(json, copy);

            copy.ResetGuid();

            animation.tweens.Insert(animation.tweens.IndexOf(te.Tween) + 1, copy);
            if (!string.IsNullOrEmpty(te.Tween.NameOverride))
            {
                copy.NameOverride = te.Tween.NameOverride + " (Copy)";
            }
            serializedObject.UpdateIfRequiredOrScript();
            FillTweenEditors();
        }

        public void MoveTweenUp(TweenEditor te)
        {
            var tweenIndex = animation.tweens.IndexOf(te.Tween);
            if (tweenIndex > 0)
            {
                while (tweenIndex > 0 && animation.tweens[tweenIndex - 1].isHidden)
                {
                    (animation.tweens[tweenIndex], animation.tweens[tweenIndex - 1]) =
                        (animation.tweens[tweenIndex - 1], animation.tweens[tweenIndex]);
                    tweenIndex--;
                }

                if (tweenIndex > 0)
                {
                    (animation.tweens[tweenIndex], animation.tweens[tweenIndex - 1]) =
                        (animation.tweens[tweenIndex - 1], animation.tweens[tweenIndex]);
                }
            }
        }

        public void MoveTweenDown(TweenEditor te)
        {
            var tweenIndex = animation.tweens.IndexOf(te.Tween);
            if (tweenIndex < animation.tweens.Count - 1)
            {
                while (tweenIndex < animation.tweens.Count - 1 && animation.tweens[tweenIndex + 1].isHidden)
                {
                    (animation.tweens[tweenIndex], animation.tweens[tweenIndex + 1]) =
                        (animation.tweens[tweenIndex + 1], animation.tweens[tweenIndex]);
                    tweenIndex++;
                }

                if (tweenIndex < animation.tweens.Count - 1)
                {
                    (animation.tweens[tweenIndex], animation.tweens[tweenIndex + 1]) =
                        (animation.tweens[tweenIndex + 1], animation.tweens[tweenIndex]);
                }
            }
        }

        void RemoveSelectedTweens()
        {
            Undo.RecordObjects(new Object[]
            {
                animation,
                this
            }, "Remove Selected Tweens");
            VisualElement tweensContainer = rootElement.Q<VisualElement>("TweensContainer");
            foreach (var tweenEditor in selectedTweenEditors)
            {
                tweensContainer.Remove(tweenEditor);
                tweenEditors.Remove(tweenEditor);
                animation.tweens.Remove(tweenEditor.Tween);
            }

            rootElement.Q<Toggle>("SelectAll").value = false;
            rootElement.Q<TextField>("FilterText").value = "";
            selectedTweenEditors.Clear();
            UpdateSelectedTweensActions();
        }

        void SetupSelectedTweensActions()
        {
            var selectedDelayField = rootElement.Q<FloatField>("SelectedDelay");
            var delayButton = rootElement.Q<Button>("SelectedDelayButton");
            delayButton.clickable.clicked += () =>
            {
                foreach (var tweenEditor in selectedTweenEditors)
                {
                    tweenEditor.Tween.Delay += selectedDelayField.value;
                }
            };

            var selectedDurationField = rootElement.Q<FloatField>("SelectedDuration");
            var durationButton = rootElement.Q<Button>("SelectedDurationButton");
            durationButton.clickable.clicked += () =>
            {
                foreach (var tweenEditor in selectedTweenEditors)
                {
                    tweenEditor.Tween.Duration += selectedDurationField.value;
                }
            };

            var curveField = rootElement.Q<CurveField>("SelectedCurveField");
            var curveButton = rootElement.Q<Button>("SetCurveButton");
            curveButton.clickable.clicked += () =>
            {
                foreach (var tweenEditor in selectedTweenEditors)
                {
                    tweenEditor.Tween.curve = curveField.value;
                }
            };

            var hideButton = rootElement.Q<Button>("HideButton");
            hideButton.clickable.clicked += () =>
            {
                foreach (var tweenEditor in selectedTweenEditors)
                {
                    tweenEditor.Tween.isHidden = true;
                    tweenEditor.Tween.isSelected = false;
                }

                FillTweenEditors();
            };

            var selectAllToggle = rootElement.Q<Toggle>("SelectAll");
            selectAllToggle.RegisterValueChangedCallback(evt =>
            {
                foreach (var tweenEditor in tweenEditors)
                {
                    tweenEditor.Tween.isSelected = evt.newValue;
                }
            });

            var filterText = rootElement.Q<TextField>("FilterText");
            filterText.RegisterValueChangedCallback(evt =>
            {
                foreach (var tween in animation.tweens)
                {
                    tween.isHidden = !string.IsNullOrEmpty(evt.newValue);
                    if (tween.NameOverride != null)
                    {
                        if (tween.NameOverride.ToLower().Contains(evt.newValue.ToLower()))
                            tween.isHidden = false;
                    }
                    if (tween.GetType().Name.ToLower().Contains(evt.newValue.ToLower()))
                    {
                        tween.isHidden = false;
                    }
                }
                FillTweenEditors();
            });

            // Button clearButton = rootElement.Q<Button>("ClearTweens");
            // clearButton.clickable.clicked += () => { RemoveSelectedTweens(); };
        }

        void UpdateSelectedTweensActions()
        {
            if (selectedTweenEditors.Count > 0)
            {
                _selectionActionsRoot.style.display = DisplayStyle.Flex;
                playBackwardSelectedButton.style.display = DisplayStyle.Flex;
                playForwardSelectedButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _selectionActionsRoot.style.display = DisplayStyle.None;
                playBackwardSelectedButton.style.display = DisplayStyle.None;
                playForwardSelectedButton.style.display = DisplayStyle.None;
            }

            _showAllButton.visible = animation.tweens.Any(tween => tween.isHidden);
        }

        public void TweenSelectionChanged(TweenEditor tweenEditor, bool selected)
        {
            if (selected && !selectedTweenEditors.Contains(tweenEditor))
            {
                selectedTweenEditors.Add(tweenEditor);
            }
            else if (!selected && selectedTweenEditors.Contains(tweenEditor))
            {
                selectedTweenEditors.Remove(tweenEditor);
            }

            UpdateSelectedTweensActions();
        }

        public void UpdateDragging(TweenEditor tweenEditor, int positionChange)
        {
            var currentIndex = tweenEditors.IndexOf(tweenEditor);
            var newIndex = currentIndex + positionChange;
            if (newIndex < 0)
            {
                newIndex = 0;
            }
            else if (newIndex >= tweenEditors.Count)
            {
                newIndex = tweenEditors.Count - 1;
            }

            var height = tweenEditor.layout.height;
            var step = (int)Mathf.Sign(newIndex - currentIndex);

            for (int i = 0; i < tweenEditors.Count; i++)
            {
                if (i > currentIndex && i <= newIndex || i < currentIndex && i >= newIndex)
                {
                    var translate = tweenEditors[i].resolvedStyle.translate;
                    tweenEditors[i].style.translate = new Translate(
                        translate.x,
                        -height * step,
                        translate.z);
                }
                else if (i != currentIndex)
                {
                    tweenEditors[i].style.translate = new Translate(0, 0, 0);
                }
            }
        }

        public void StopDragging(TweenEditor tweenEditor, int positionChange)
        {
            int sign = (int)Mathf.Sign(positionChange);

            while (Mathf.Abs(positionChange) > 0)
            {
                if (sign < 0)
                {
                    MoveTweenUp(tweenEditor);
                }
                else
                {
                    MoveTweenDown(tweenEditor);
                }

                positionChange -= sign;
            }

            serializedObject.UpdateIfRequiredOrScript();
            EditorUtility.SetDirty(animation);
            FillTweenEditors();
            _needRefreshDelayDurationVisualization = true;
        }
    }
}