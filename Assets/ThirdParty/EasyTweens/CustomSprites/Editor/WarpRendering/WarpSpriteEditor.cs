#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace EasyTweens.Editor
{
    [CustomEditor(typeof(WarpSprite))]
    [CanEditMultipleObjects]
    public sealed class WarpSpriteEditor : UnityEditor.Editor
    {
        private SerializedProperty _gridX;
        private SerializedProperty _gridY;
        private SerializedProperty _resolution;
        private SerializedProperty _interpolation;

        private void OnEnable()
        {
            _gridX = serializedObject.FindProperty("_gridX");
            _gridY = serializedObject.FindProperty("_gridY");
            _resolution = serializedObject.FindProperty("_resolution");
            _interpolation = serializedObject.FindProperty("_interpolation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_gridX, new GUIContent("Grid X"));
            EditorGUILayout.PropertyField(_gridY, new GUIContent("Grid Y"));
            EditorGUILayout.PropertyField(_resolution, new GUIContent(
                "Resolution",
                "Grid cells per axis the sprite is tessellated into. A curved warp needs "
                + "enough cells to bend smoothly; a flat cage costs nothing either way."));
            EditorGUILayout.PropertyField(_interpolation);

            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (WarpSprite sprite in targets)
                    sprite.Rebuild();
            }

            DrawActions();
            DrawMeshCost();
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var editing = ToolManager.activeToolType == typeof(WarpSpriteTool);
                if (GUILayout.Button(editing ? "Stop Editing Grid" : "Edit Grid In Scene"))
                {
                    if (editing)
                        ToolManager.RestorePreviousTool();
                    else
                        ToolManager.SetActiveTool<WarpSpriteTool>();
                }

                if (!GUILayout.Button("Reset Grid"))
                    return;
            }

            foreach (WarpSprite sprite in targets)
            {
                Undo.RecordObject(sprite, "Reset Warp Grid");
                sprite.ResetGrid();
                EditorUtility.SetDirty(sprite);
            }
        }

        private void DrawMeshCost()
        {
            if (targets.Length != 1 || target is not WarpSprite sprite)
                return;

            if (sprite.LastVertexCount == 0)
            {
                EditorGUILayout.LabelField(
                    "Cage is flat — the original sprite is drawn untouched.",
                    EditorStyles.miniLabel);
                return;
            }

            var message = $"Mesh: {sprite.LastVertexCount} verts, {sprite.LastTriangleCount} tris";
            if (sprite.LastVertexCount > 2000)
                EditorGUILayout.HelpBox($"{message}. Lower Resolution to reduce it.", MessageType.Warning);
            else
                EditorGUILayout.LabelField(message, EditorStyles.miniLabel);
        }
    }
}
#endif
