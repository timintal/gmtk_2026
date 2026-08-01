#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UI;
using UnityEngine;

namespace EasyTweens.Editor
{
    [CustomEditor(typeof(WarpImage))]
    [CanEditMultipleObjects]
    public sealed class WarpImageEditor : ImageEditor
    {
        private SerializedProperty _gridX;
        private SerializedProperty _gridY;
        private SerializedProperty _subdivisions;
        private SerializedProperty _interpolation;

        protected override void OnEnable()
        {
            base.OnEnable();
            _gridX = serializedObject.FindProperty("_gridX");
            _gridY = serializedObject.FindProperty("_gridY");
            _subdivisions = serializedObject.FindProperty("_subdivisions");
            _interpolation = serializedObject.FindProperty("_interpolation");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Warp", EditorStyles.boldLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(_gridX, new GUIContent("Grid X"));
            EditorGUILayout.PropertyField(_gridY, new GUIContent("Grid Y"));
            EditorGUILayout.PropertyField(_subdivisions, new GUIContent(
                "Subdivisions",
                "Splits applied to each edge of every triangle the base Image produces. "
                + "Simple images start from 2 triangles; sliced and tiled ones start from many more."));
            EditorGUILayout.PropertyField(_interpolation);

            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (WarpImage image in targets)
                    image.SetVerticesDirty();
            }

            DrawActions();
            DrawMeshCost();
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var editing = ToolManager.activeToolType == typeof(WarpImageTool);
                if (GUILayout.Button(editing ? "Stop Editing Grid" : "Edit Grid In Scene"))
                {
                    if (editing)
                        ToolManager.RestorePreviousTool();
                    else
                        ToolManager.SetActiveTool<WarpImageTool>();
                }

                if (!GUILayout.Button("Reset Grid"))
                    return;
            }

            foreach (WarpImage image in targets)
            {
                Undo.RecordObject(image, "Reset Warp Grid");
                image.ResetGrid();
                EditorUtility.SetDirty(image);
            }
        }

        private void DrawMeshCost()
        {
            if (targets.Length != 1 || target is not WarpImage image)
                return;

            var message = $"Mesh: {image.LastVertexCount} verts, {image.LastTriangleCount} tris";
            if (image.LastVertexCount > 2000)
                EditorGUILayout.HelpBox($"{message}. Lower Subdivisions to reduce it.", MessageType.Warning);
            else
                EditorGUILayout.LabelField(message, EditorStyles.miniLabel);
        }
    }
}
#endif
