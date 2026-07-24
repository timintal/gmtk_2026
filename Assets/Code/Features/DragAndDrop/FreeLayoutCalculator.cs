using System.Collections.Generic;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public struct FreeLayoutItem
    {
        public Vector2 Center;
        public Vector2 HalfSize;
    }

    /// <summary>
    /// Pure AABB relaxation used by the free container layout. Items keep their positions and are
    /// only nudged apart when they overlap beyond <paramref name="overlapTolerance"/>. Every item is
    /// treated equally (the freshly dropped one included) so overlaps are shared instead of forcing a
    /// single item to bear the whole push. All items are kept inside <paramref name="bounds"/>.
    /// </summary>
    public static class FreeLayoutCalculator
    {
        public static void Resolve(
            Rect bounds,
            IList<FreeLayoutItem> items,
            float overlapTolerance,
            int iterations,
            FreeLayoutShape shape = FreeLayoutShape.Rectangle)
        {
            var count = items.Count;
            if (count == 0)
            {
                return;
            }

            var passes = Mathf.Max(1, iterations);
            for (var pass = 0; pass < passes; pass++)
            {
                if (Step(bounds, items, overlapTolerance, stepScale: 1f, maxStep: 0f, shape) <= 0f)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Runs a single separation pass and returns the largest distance any item moved this call.
        /// Use it once per frame to spread items apart gradually instead of resolving instantly.
        /// <paramref name="stepScale"/> (0..1) softens each push (easing); <paramref name="maxStep"/>
        /// caps how far an item may travel this step (0 = uncapped). A return value of 0 means the
        /// layout is settled.
        /// </summary>
        public static float Step(
            Rect bounds,
            IList<FreeLayoutItem> items,
            float overlapTolerance,
            float stepScale,
            float maxStep,
            FreeLayoutShape shape = FreeLayoutShape.Rectangle)
        {
            var count = items.Count;
            if (count == 0)
            {
                return 0f;
            }

            var maxMoved = 0f;

            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    maxMoved = Mathf.Max(maxMoved, Separate(items, i, j, overlapTolerance, stepScale, maxStep, shape));
                }
            }

            for (var i = 0; i < count; i++)
            {
                var item = items[i];
                var clampHalf = ClampHalfSize(item, shape);
                var clamped = ClampInside(bounds, item.Center, clampHalf);
                var moved = (clamped - item.Center).magnitude;
                if (moved > 0f)
                {
                    item.Center = clamped;
                    items[i] = item;
                    maxMoved = Mathf.Max(maxMoved, moved);
                }
            }

            return maxMoved;
        }

        private static float Separate(IList<FreeLayoutItem> items, int i, int j, float overlapTolerance, float stepScale, float maxStep, FreeLayoutShape shape)
        {
            var push = shape == FreeLayoutShape.Circle
                ? ResolveCirclePush(items, i, j, overlapTolerance)
                : ResolveRectanglePush(items, i, j, overlapTolerance);

            if (push == Vector2.zero)
            {
                return 0f;
            }

            var half = push * 0.5f * stepScale;
            if (maxStep > 0f)
            {
                var magnitude = half.magnitude;
                if (magnitude > maxStep)
                {
                    half *= maxStep / magnitude;
                }
            }

            var a = items[i];
            var b = items[j];
            a.Center -= half;
            b.Center += half;
            items[i] = a;
            items[j] = b;

            return half.magnitude;
        }

        private static Vector2 ResolveRectanglePush(IList<FreeLayoutItem> items, int i, int j, float overlapTolerance)
        {
            var a = items[i];
            var b = items[j];

            var delta = b.Center - a.Center;
            var combined = a.HalfSize + b.HalfSize;
            var requiredX = Mathf.Max(0f, combined.x - overlapTolerance);
            var requiredY = Mathf.Max(0f, combined.y - overlapTolerance);

            var penetrationX = requiredX - Mathf.Abs(delta.x);
            var penetrationY = requiredY - Mathf.Abs(delta.y);

            // AABBs must overlap on both axes (beyond tolerance) to count as "overlapping too much".
            if (penetrationX <= 0f || penetrationY <= 0f)
            {
                return Vector2.zero;
            }

            var coincident = Mathf.Approximately(delta.x, 0f) && Mathf.Approximately(delta.y, 0f);
            if (penetrationX <= penetrationY)
            {
                var dir = coincident ? DeterministicDir(i, j) : Mathf.Sign(delta.x == 0f ? 1f : delta.x);
                return new Vector2(penetrationX * dir, 0f);
            }

            var dirY = coincident ? DeterministicDir(i, j) : Mathf.Sign(delta.y == 0f ? 1f : delta.y);
            return new Vector2(0f, penetrationY * dirY);
        }

        private static Vector2 ResolveCirclePush(IList<FreeLayoutItem> items, int i, int j, float overlapTolerance)
        {
            var a = items[i];
            var b = items[j];

            var delta = b.Center - a.Center;
            var required = Mathf.Max(0f, Radius(a) + Radius(b) - overlapTolerance);
            var distance = delta.magnitude;
            var penetration = required - distance;
            if (penetration <= 0f)
            {
                return Vector2.zero;
            }

            var dir = distance > 1e-5f ? delta / distance : new Vector2(DeterministicDir(i, j), 0f);
            return dir * penetration;
        }

        private static Vector2 ClampHalfSize(in FreeLayoutItem item, FreeLayoutShape shape)
        {
            if (shape != FreeLayoutShape.Circle)
            {
                return item.HalfSize;
            }

            var radius = Radius(item);
            return new Vector2(radius, radius);
        }

        private static float Radius(in FreeLayoutItem item)
        {
            return Mathf.Max(item.HalfSize.x, item.HalfSize.y);
        }

        private static float DeterministicDir(int i, int j)
        {
            return (i + j) % 2 == 0 ? 1f : -1f;
        }

        private static Vector2 ClampInside(Rect bounds, Vector2 center, Vector2 halfSize)
        {
            var minX = bounds.xMin + halfSize.x;
            var maxX = bounds.xMax - halfSize.x;
            var minY = bounds.yMin + halfSize.y;
            var maxY = bounds.yMax - halfSize.y;

            var x = minX <= maxX ? Mathf.Clamp(center.x, minX, maxX) : bounds.center.x;
            var y = minY <= maxY ? Mathf.Clamp(center.y, minY, maxY) : bounds.center.y;
            return new Vector2(x, y);
        }
    }
}
