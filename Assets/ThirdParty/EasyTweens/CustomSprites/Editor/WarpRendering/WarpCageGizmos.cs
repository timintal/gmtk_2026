#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EasyTweens.Editor
{
    /// Scene view drawing shared by the warp grid tool and the control point ping, so the
    /// cage looks the same whichever of them puts it on screen.
    internal static class WarpCageGizmos
    {
        public static readonly Color CageColor = new(0.30f, 0.72f, 1f, 0.9f);
        public static readonly Color PointColor = new(1f, 0.76f, 0.20f, 1f);

        private const int CurveSamples = 24;
        private static readonly Vector3[] CurveBuffer = new Vector3[CurveSamples + 1];

        /// Drawn in GUI space rather than as world geometry: the cage is coplanar with the
        /// thing it deforms, and a world-space line would be painted over by it.
        public static void DrawCage(IWarpCage cage, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.BeginGUI();
            var previousMatrix = Handles.matrix;
            var previousColor = Handles.color;
            Handles.matrix = Matrix4x4.identity;
            Handles.color = color;

            for (var y = 0; y < cage.GridY; y++)
            {
                var v = y / (float)(cage.GridY - 1);
                for (var s = 0; s <= CurveSamples; s++)
                    CurveBuffer[s] = SurfaceToGUI(cage, new Vector2(s / (float)CurveSamples, v));

                Handles.DrawAAPolyLine(2f, CurveBuffer.Length, CurveBuffer);
            }

            for (var x = 0; x < cage.GridX; x++)
            {
                var u = x / (float)(cage.GridX - 1);
                for (var s = 0; s <= CurveSamples; s++)
                    CurveBuffer[s] = SurfaceToGUI(cage, new Vector2(u, s / (float)CurveSamples));

                Handles.DrawAAPolyLine(2f, CurveBuffer.Length, CurveBuffer);
            }

            Handles.color = previousColor;
            Handles.matrix = previousMatrix;
            Handles.EndGUI();
        }

        public static Vector3 ControlPointToGUI(IWarpCage cage, int index) =>
            HandleUtility.WorldToGUIPoint(cage.Transform.TransformPoint(
                cage.NormalizedToLocal(cage.GetControlPoint(index))));

        private static Vector3 SurfaceToGUI(IWarpCage cage, Vector2 t) =>
            HandleUtility.WorldToGUIPoint(cage.Transform.TransformPoint(cage.EvaluateLocal(t)));
    }
}
#endif
