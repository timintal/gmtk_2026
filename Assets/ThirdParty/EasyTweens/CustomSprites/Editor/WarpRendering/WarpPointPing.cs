#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EasyTweens.Editor
{
    /// Flashes one control point in the scene view so you can see which one an index refers
    /// to. Draws itself, rather than leaving it to the warp grid tool, so the point is
    /// identifiable even when the tool is inactive or the target is not the selected object.
    internal static class WarpPointPing
    {
        private const double Duration = 0.9;
        private const float DotRadius = 5f;
        private const float RingStartRadius = 7f;
        private const float RingEndRadius = 30f;

        private static readonly Color FlashColor = new(1f, 0.23f, 0.19f, 1f);

        private static IWarpCage _cage;
        private static int _index;
        private static double _startTime;

        /// The cage is held through an interface, so a destroyed component would slip past a
        /// plain null check and only Unity's own operator can see that it has gone.
        private static bool CageAlive => _cage != null && _cage is Object obj && obj != null;

        public static void Ping(IWarpCage cage, int index)
        {
            if (cage == null || cage is not Object obj || obj == null || index < 0 || index >= cage.ControlPointCount)
            {
                Stop();
                return;
            }

            _cage = cage;
            _index = index;
            _startTime = EditorApplication.timeSinceStartup;

            SceneView.duringSceneGui -= Draw;
            SceneView.duringSceneGui += Draw;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;

            SceneView.RepaintAll();
        }

        /// Colour the warp grid tool should give one of its handles, so the point the user
        /// is pointing at reads as flashing rather than as a second dot drawn over the first.
        public static bool TryGetPointColor(IWarpCage cage, int index, out Color color)
        {
            color = WarpCageGizmos.PointColor;
            if (!ReferenceEquals(_cage, cage) || _index != index)
                return false;

            var progress = Progress();
            if (progress >= 1f)
                return false;

            color = Color.Lerp(FlashColor, WarpCageGizmos.PointColor, Mathf.SmoothStep(0f, 1f, progress));
            return true;
        }

        private static void Tick()
        {
            if (!CageAlive || _index >= _cage.ControlPointCount || Progress() >= 1f)
                Stop();

            SceneView.RepaintAll();
        }

        private static void Draw(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (!CageAlive || _index >= _cage.ControlPointCount)
            {
                Stop();
                return;
            }

            var progress = Progress();
            if (progress >= 1f)
            {
                Stop();
                return;
            }

            var fade = 1f - progress;
            var cage = WarpCageGizmos.CageColor;
            WarpCageGizmos.DrawCage(_cage, new Color(cage.r, cage.g, cage.b, cage.a * fade * 0.7f));

            var center = WarpCageGizmos.ControlPointToGUI(_cage, _index);

            Handles.BeginGUI();
            var previousMatrix = Handles.matrix;
            var previousColor = Handles.color;
            Handles.matrix = Matrix4x4.identity;

            Handles.color = new Color(FlashColor.r, FlashColor.g, FlashColor.b, fade * 0.8f);
            Handles.DrawWireDisc(center, Vector3.forward, Mathf.Lerp(RingStartRadius, RingEndRadius, progress));

            TryGetPointColor(_cage, _index, out var dotColor);
            Handles.color = dotColor;
            Handles.DrawSolidDisc(center, Vector3.forward, DotRadius);

            Handles.color = previousColor;
            Handles.matrix = previousMatrix;
            Handles.EndGUI();
        }

        private static float Progress() =>
            Mathf.Clamp01((float)((EditorApplication.timeSinceStartup - _startTime) / Duration));

        private static void Stop()
        {
            _cage = null;
            SceneView.duringSceneGui -= Draw;
            EditorApplication.update -= Tick;
        }
    }
}
#endif
