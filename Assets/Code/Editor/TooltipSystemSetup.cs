#if UNITY_EDITOR
using System;
using System.IO;
using Code.Common.View.UI;
using Code.Configs;
using Code.Features.Tooltip;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Editor
{
    public static class TooltipSystemSetup
    {
        private const string PrefabPath = "Assets/_Game/Prefabs/UI/SimpleTextTooltip.prefab";
        private const string VisualConfigPath = "Assets/Code/Configs/VisualConfig.asset";

        [MenuItem("Static Proto/Tooltip/Create Simple Text Tooltip Prefab")]
        public static void CreateSimpleTextTooltipPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath)!);

            var root = new GameObject(
                "SimpleTextTooltip",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(Image),
                typeof(ContentSizeFitter),
                typeof(SimpleTextTooltipView));

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);

            var background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            background.raycastTarget = false;

            var fitter = root.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement));
            textGo.transform.SetParent(root.transform, false);

            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (textType == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                EditorUtility.DisplayDialog(
                    "Tooltip Prefab",
                    "TextMeshProUGUI type was not found. Is Unity.TextMeshPro installed?",
                    "OK");
                return;
            }

            var textComponent = textGo.AddComponent(textType);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);

            var text = textGo.GetComponent(textType);
            var serializedText = new SerializedObject(text);
            serializedText.FindProperty("m_text").stringValue = "Tooltip";
            serializedText.FindProperty("m_fontSize").floatValue = 18f;
            serializedText.FindProperty("m_richText").boolValue = true;
            serializedText.FindProperty("m_enableWordWrapping").boolValue = true;
            var colorProp = serializedText.FindProperty("m_fontColor");
            colorProp.colorValue = Color.white;
            var alignmentProp = serializedText.FindProperty("m_HorizontalAlignment");
            if (alignmentProp != null)
            {
                alignmentProp.intValue = 1; // Left
            }

            serializedText.FindProperty("m_RaycastTarget").boolValue = false;
            serializedText.ApplyModifiedPropertiesWithoutUndo();

            var layoutElement = textGo.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 240f;

            var view = root.GetComponent<SimpleTextTooltipView>();
            var canvasGroup = root.GetComponent<CanvasGroup>();
            SerializedObject serializedView = new(view);
            serializedView.FindProperty("_text").objectReferenceValue = text;
            serializedView.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            AssignPrefabToVisualConfig(prefab.GetComponent<SimpleTextTooltipView>());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Tooltip Prefab Created",
                $"Saved prefab to:\n{PrefabPath}\n\nVisualConfig was updated.",
                "OK");
        }

        [MenuItem("Static Proto/Tooltip/Setup Tooltip Canvas In Active Scene")]
        public static void SetupTooltipCanvasInActiveScene()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<TooltipCanvas>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorUtility.DisplayDialog("Tooltip Canvas", "TooltipCanvas already exists in the scene.", "OK");
                return;
            }

            var canvasGo = new GameObject(
                "TooltipCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(TooltipCanvas));

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Tooltip Canvas");
            Selection.activeGameObject = canvasGo;
            EditorSceneManager.MarkSceneDirty(canvasGo.scene);

            var registry = UnityEngine.Object.FindFirstObjectByType<Code.Common.View.SceneResourceRegistry>();
            if (registry != null)
            {
                var serializedRegistry = new SerializedObject(registry);
                var resources = serializedRegistry.FindProperty("_resources");
                resources.InsertArrayElementAtIndex(resources.arraySize);
                resources.GetArrayElementAtIndex(resources.arraySize - 1).objectReferenceValue =
                    canvasGo.GetComponent<TooltipCanvas>();
                serializedRegistry.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.DisplayDialog(
                "Tooltip Canvas Created",
                "TooltipCanvas was added to the active scene.\nIf SceneResourceRegistry exists, it was registered automatically.",
                "OK");
        }

        private static void AssignPrefabToVisualConfig(SimpleTextTooltipView prefab)
        {
            var visualConfig = AssetDatabase.LoadAssetAtPath<VisualConfig>(VisualConfigPath);
            if (visualConfig == null)
            {
                Debug.LogWarning($"VisualConfig not found at {VisualConfigPath}");
                return;
            }

            var serializedConfig = new SerializedObject(visualConfig);
            var prefabs = serializedConfig.FindProperty("TooltipPrefabs");
            prefabs.arraySize = 1;
            var entry = prefabs.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("Type").enumValueIndex = (int)TooltipType.Text;
            entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(visualConfig);
        }
    }
}
#endif
