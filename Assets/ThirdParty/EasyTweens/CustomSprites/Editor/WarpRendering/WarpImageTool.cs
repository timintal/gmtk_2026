#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace EasyTweens.Editor
{
    /// Drag-the-cage scene view editing, shared by every warped renderer. EditorTool filters
    /// its targets by concrete component type, so each warpable type needs its own subclass.
    public abstract class WarpCageTool : EditorTool
    {
        private const float HandleScreenSize = 0.05f;

        private GUIContent _toolbarIcon;

        public override GUIContent toolbarIcon => _toolbarIcon ??= new GUIContent(
            EditorGUIUtility.IconContent("MoveTool").image,
            "Warp Grid — drag the control points to deform the image");

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView)
                return;

            foreach (var candidate in targets)
            {
                if (candidate is not IWarpCage cage)
                    continue;

                if (candidate is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                    continue;

                WarpCageGizmos.DrawCage(cage, WarpCageGizmos.CageColor);
                DrawControlPoints(cage, (Object)candidate);
            }
        }

        private static void DrawControlPoints(IWarpCage cage, Object undoTarget)
        {
            var previousMatrix = Handles.matrix;
            var previousColor = Handles.color;

            // Working inside the target's own space lets us hand local rect coordinates
            // straight to the handles, whatever the canvas render mode or sprite pivot is.
            Handles.matrix = cage.Transform.localToWorldMatrix;

            for (var i = 0; i < cage.ControlPointCount; i++)
            {
                WarpPointPing.TryGetPointColor(cage, i, out var pointColor);
                Handles.color = pointColor;

                var local = (Vector3)cage.NormalizedToLocal(cage.GetControlPoint(i));

                // GetHandleSize already reports in the space of Handles.matrix, so this stays
                // a constant on-screen size whatever the object scale is.
                var size = HandleUtility.GetHandleSize(local) * HandleScreenSize;

                EditorGUI.BeginChangeCheck();
                var moved = Handles.Slider2D(
                    local,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    size,
                    Handles.DotHandleCap,
                    Vector2.zero);

                if (!EditorGUI.EndChangeCheck())
                    continue;

                Undo.RecordObject(undoTarget, "Edit Warp Grid");
                cage.SetControlPoint(i, cage.LocalToNormalized(moved));
                EditorUtility.SetDirty(undoTarget);
            }

            Handles.color = previousColor;
            Handles.matrix = previousMatrix;
        }
    }

    [EditorTool("Warp Grid", typeof(WarpImage))]
    public sealed class WarpImageTool : WarpCageTool
    {
    }

    [EditorTool("Warp Grid", typeof(WarpSprite))]
    public sealed class WarpSpriteTool : WarpCageTool
    {
    }
}
#endif
