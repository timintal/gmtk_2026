using System;
using UnityEngine;

namespace EasyTweens
{
    public enum WarpInterpolation
    {
        /// Tensor-product Bezier patch. Smooth everywhere; the surface passes through the
        /// corner control points only, the rest pull the surface like Bezier handles.
        Bezier,

        /// Piecewise bilinear, like a Blender lattice on linear interpolation. The surface
        /// passes through every control point but creases at them.
        Bilinear,
    }

    /// Common surface of anything deformed by a control point cage, so the scene view tooling
    /// can drive a UI graphic and a sprite renderer without knowing which one it has.
    public interface IWarpCage
    {
        int GridX { get; }
        int GridY { get; }
        int ControlPointCount { get; }

        /// Space the handles are drawn in. A RectTransform for UI, the plain transform for
        /// sprites; both put local rect coordinates where the cage expects them.
        Transform Transform { get; }

        Vector2 GetControlPoint(int index);
        void SetControlPoint(int index, Vector2 normalized);
        Vector2 GetRestControlPoint(int index);
        void ResetGrid();

        Vector2 NormalizedToLocal(Vector2 normalized);
        Vector2 LocalToNormalized(Vector2 local);

        /// Warped position, in local space, of the point that would sit at normalized
        /// coordinate <paramref name="t"/> on the undeformed rect.
        Vector2 EvaluateLocal(Vector2 t);
    }

    /// Cage evaluation shared by every warped renderer. Control points are held in normalized
    /// space where (0,0) is the rect's min corner and (1,1) its max, so a deformation keeps
    /// its shape when the rect resizes.
    public static class WarpCage
    {
        public const int MinGridSize = 2;
        public const int MaxGridSize = 8;

        /// Where a control point sits when the grid is undeformed.
        public static Vector2 RestPoint(int x, int y, int gridX, int gridY) =>
            new(x / (float)(gridX - 1), y / (float)(gridY - 1));

        public static Vector2 RestPoint(int index, int gridX, int gridY) =>
            RestPoint(index % gridX, index / gridX, gridX, gridY);

        public static bool IsWarped(Vector2[] points, int gridX, int gridY)
        {
            if (points == null)
                return false;

            for (var i = 0; i < points.Length; i++)
            {
                if ((points[i] - RestPoint(i, gridX, gridY)).sqrMagnitude > 1e-10f)
                    return true;
            }

            return false;
        }

        public static Vector2 NormalizedToLocal(Rect rect, Vector2 normalized) =>
            new(rect.xMin + normalized.x * rect.width, rect.yMin + normalized.y * rect.height);

        public static Vector2 LocalToNormalized(Rect rect, Vector2 local) =>
            new(
                rect.width > 0f ? (local.x - rect.xMin) / rect.width : 0f,
                rect.height > 0f ? (local.y - rect.yMin) / rect.height : 0f);

        public static Vector2 Evaluate(
            Vector2[] points,
            int gridX,
            int gridY,
            WarpInterpolation interpolation,
            Vector2 t) =>
            interpolation == WarpInterpolation.Bezier
                ? EvaluateBezier(points, gridX, gridY, t)
                : EvaluateBilinear(points, gridX, gridY, t);

        /// Rebuilds the control point array when the grid dimensions change, resampling the
        /// existing deformation onto the new resolution instead of discarding it.
        public static void EnsureGrid(
            ref Vector2[] points,
            ref int gridX,
            ref int gridY,
            ref int bakedGridX,
            ref int bakedGridY,
            WarpInterpolation interpolation)
        {
            gridX = Mathf.Clamp(gridX, MinGridSize, MaxGridSize);
            gridY = Mathf.Clamp(gridY, MinGridSize, MaxGridSize);

            var bakedValid = bakedGridX >= MinGridSize
                             && bakedGridY >= MinGridSize
                             && points != null
                             && points.Length == bakedGridX * bakedGridY;

            if (bakedValid && bakedGridX == gridX && bakedGridY == gridY)
                return;

            var resampled = new Vector2[gridX * gridY];
            for (var y = 0; y < gridY; y++)
            {
                for (var x = 0; x < gridX; x++)
                {
                    var t = RestPoint(x, y, gridX, gridY);
                    resampled[y * gridX + x] = bakedValid
                        ? Evaluate(points, bakedGridX, bakedGridY, interpolation, t)
                        : t;
                }
            }

            points = resampled;
            bakedGridX = gridX;
            bakedGridY = gridY;
        }

        private static Vector2 EvaluateBezier(Vector2[] points, int gridX, int gridY, Vector2 t)
        {
            Span<float> basisX = stackalloc float[MaxGridSize];
            Span<float> basisY = stackalloc float[MaxGridSize];
            Bernstein(t.x, gridX, basisX);
            Bernstein(t.y, gridY, basisY);

            var result = Vector2.zero;
            for (var y = 0; y < gridY; y++)
            {
                var weightY = basisY[y];
                if (weightY == 0f)
                    continue;

                var row = y * gridX;
                for (var x = 0; x < gridX; x++)
                    result += points[row + x] * (basisX[x] * weightY);
            }

            return result;
        }

        private static Vector2 EvaluateBilinear(Vector2[] points, int gridX, int gridY, Vector2 t)
        {
            var cellsX = gridX - 1;
            var cellsY = gridY - 1;

            var fx = Mathf.Clamp01(t.x) * cellsX;
            var fy = Mathf.Clamp01(t.y) * cellsY;
            var x0 = Mathf.Clamp((int)fx, 0, cellsX - 1);
            var y0 = Mathf.Clamp((int)fy, 0, cellsY - 1);

            var localX = fx - x0;
            var localY = fy - y0;
            var bottom = y0 * gridX + x0;
            var top = bottom + gridX;

            return Vector2.LerpUnclamped(
                Vector2.LerpUnclamped(points[bottom], points[bottom + 1], localX),
                Vector2.LerpUnclamped(points[top], points[top + 1], localX),
                localY);
        }

        /// Builds the Bernstein basis of degree count-1 by the triangular recurrence, which
        /// avoids binomial coefficients and stays stable at high degrees.
        private static void Bernstein(float t, int count, Span<float> basis)
        {
            var degree = count - 1;
            var inverseT = 1f - t;

            basis[0] = 1f;
            for (var i = 1; i <= degree; i++)
                basis[i] = 0f;

            for (var i = 1; i <= degree; i++)
            {
                basis[i] = basis[i - 1] * t;
                for (var j = i - 1; j > 0; j--)
                    basis[j] = basis[j] * inverseT + basis[j - 1] * t;

                basis[0] *= inverseT;
            }
        }
    }

    /// Marks an int as an index into the cage of the tween's target, which makes the editor
    /// flash that point in the scene view whenever the value changes.
    public sealed class WarpPointIndexAttribute : PropertyAttribute
    {
    }
}
