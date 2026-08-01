using System;
using System.Collections.Generic;
using Code.Warp;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTweens
{
    /// Image that deforms its quad through a control-point cage, like a Blender lattice or
    /// a Photoshop warp. Unlike an affine transform, a curved warp cannot be expressed by
    /// moving the four corners of a quad, so the mesh is re-tessellated on rebuild.
    ///
    /// Control points are stored in normalized rect space where (0,0) is the rect's min
    /// corner and (1,1) its max, so a deformation keeps its shape when the rect resizes.
    [AddComponentMenu("UI/Warp Image")]
    public sealed class WarpImage : Image, IWarpCage
    {
        [SerializeField, Range(WarpCage.MinGridSize, WarpCage.MaxGridSize)] private int _gridX = 3;
        [SerializeField, Range(WarpCage.MinGridSize, WarpCage.MaxGridSize)] private int _gridY = 3;
        [SerializeField, Range(1, 32)] private int _subdivisions = 12;
        [SerializeField] private WarpInterpolation _interpolation = WarpInterpolation.Bezier;

        [SerializeField, HideInInspector] private Vector2[] _controlPoints;
        [SerializeField, HideInInspector] private int _bakedGridX;
        [SerializeField, HideInInspector] private int _bakedGridY;

        private static readonly List<UIVertex> SourceTriangles = new();

        public int GridX => _gridX;
        public int GridY => _gridY;
        public int Subdivisions => _subdivisions;

        public Transform Transform => rectTransform;

        /// Size of the mesh produced by the last rebuild. Reported by the inspector, since
        /// the cost depends on the base image type as much as on the subdivision count.
        public int LastVertexCount { get; private set; }

        public int LastTriangleCount { get; private set; }

        public int ControlPointCount
        {
            get
            {
                EnsureGrid();
                return _controlPoints.Length;
            }
        }

        public Vector2 GetControlPoint(int index)
        {
            EnsureGrid();
            return _controlPoints[index];
        }

        public void SetControlPoint(int index, Vector2 normalized)
        {
            EnsureGrid();
            if (_controlPoints[index] == normalized)
                return;

            _controlPoints[index] = normalized;
            SetVerticesDirty();
        }

        /// Snapshot of the whole cage, safe to keep: it is a copy, not the live buffer.
        public Vector2[] GetControlPoints()
        {
            EnsureGrid();
            var snapshot = new Vector2[_controlPoints.Length];
            Array.Copy(_controlPoints, snapshot, snapshot.Length);
            return snapshot;
        }

        /// Applies a whole cage at once, rebuilding the mesh only if something moved.
        /// A snapshot taken at a different grid resolution is rejected, since the points
        /// carry no dimensions of their own and would land in the wrong rows.
        public void SetControlPoints(Vector2[] points)
        {
            EnsureGrid();
            if (points == null || points.Length != _controlPoints.Length)
                return;

            var moved = false;
            for (var i = 0; i < points.Length; i++)
            {
                if (_controlPoints[i] == points[i])
                    continue;

                _controlPoints[i] = points[i];
                moved = true;
            }

            if (moved)
                SetVerticesDirty();
        }

        /// Where control point <paramref name="index"/> sits when the grid is undeformed.
        public Vector2 GetRestControlPoint(int index)
        {
            EnsureGrid();
            return WarpCage.RestPoint(index, _gridX, _gridY);
        }

        public void ResetGrid()
        {
            EnsureGrid();
            for (var i = 0; i < _controlPoints.Length; i++)
                _controlPoints[i] = WarpCage.RestPoint(i, _gridX, _gridY);

            SetVerticesDirty();
        }

        public Vector2 NormalizedToLocal(Vector2 normalized) =>
            WarpCage.NormalizedToLocal(rectTransform.rect, normalized);

        public Vector2 LocalToNormalized(Vector2 local) =>
            WarpCage.LocalToNormalized(rectTransform.rect, local);

        public Vector2 EvaluateLocal(Vector2 t)
        {
            EnsureGrid();
            return NormalizedToLocal(Evaluate(t));
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            EnsureGrid();
            if (WarpCage.IsWarped(_controlPoints, _gridX, _gridY))
            {
                // Subdividing whatever the base Image produced, rather than generating our own
                // quad, keeps every image type working: simple, sliced, tiled, filled and
                // sprite-mesh all arrive here as plain triangles.
                if (_subdivisions > 1)
                    SubdivideAndWarp(vh);
                else
                    WarpInPlace(vh);
            }

            LastVertexCount = vh.currentVertCount;
            LastTriangleCount = vh.currentIndexCount / 3;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            EnsureGrid();
            SetVerticesDirty();
        }
#endif

        private void SubdivideAndWarp(VertexHelper vh)
        {
            SourceTriangles.Clear();
            vh.GetUIVertexStream(SourceTriangles);
            vh.Clear();

            for (var i = 0; i + 2 < SourceTriangles.Count; i += 3)
            {
                AddSubdividedTriangle(
                    vh,
                    SourceTriangles[i],
                    SourceTriangles[i + 1],
                    SourceTriangles[i + 2]);
            }

            SourceTriangles.Clear();
        }

        /// Emits a barycentric lattice over the triangle, warping each generated vertex.
        /// Vertices are shared within the lattice; the seams between adjacent base triangles
        /// stay crack-free because the warp is a pure function of the undeformed position.
        private void AddSubdividedTriangle(VertexHelper vh, UIVertex a, UIVertex b, UIVertex c)
        {
            var steps = _subdivisions;
            var inverseSteps = 1f / steps;
            var firstIndex = vh.currentVertCount;

            for (var i = 0; i <= steps; i++)
            {
                var u = i * inverseSteps;
                for (var j = 0; j <= steps - i; j++)
                {
                    var v = j * inverseSteps;
                    vh.AddVert(Warp(Blend(a, b, c, 1f - u - v, u, v)));
                }
            }

            var rowStart = firstIndex;
            for (var i = 0; i < steps; i++)
            {
                var nextRowStart = rowStart + (steps - i + 1);
                for (var j = 0; j < steps - i; j++)
                {
                    vh.AddTriangle(rowStart + j, nextRowStart + j, rowStart + j + 1);
                    if (j < steps - i - 1)
                        vh.AddTriangle(rowStart + j + 1, nextRowStart + j, nextRowStart + j + 1);
                }

                rowStart = nextRowStart;
            }
        }

        private void WarpInPlace(VertexHelper vh)
        {
            var vertex = new UIVertex();
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vh.SetUIVertex(Warp(vertex), i);
            }
        }

        private UIVertex Warp(UIVertex vertex)
        {
            var rect = rectTransform.rect;
            var position = vertex.position;

            var t = LocalToNormalized(position);
            var displacement = Evaluate(t) - t;

            position.x += displacement.x * rect.width;
            position.y += displacement.y * rect.height;
            vertex.position = position;

            return vertex;
        }

        private static UIVertex Blend(UIVertex a, UIVertex b, UIVertex c, float wa, float wb, float wc)
        {
            var result = a;
            result.position = a.position * wa + b.position * wb + c.position * wc;
            result.uv0 = a.uv0 * wa + b.uv0 * wb + c.uv0 * wc;
            result.uv1 = a.uv1 * wa + b.uv1 * wb + c.uv1 * wc;
            result.color = (Color)a.color * wa + (Color)b.color * wb + (Color)c.color * wc;
            return result;
        }

        private Vector2 Evaluate(Vector2 t) =>
            WarpCage.Evaluate(_controlPoints, _gridX, _gridY, _interpolation, t);

        private void EnsureGrid() =>
            WarpCage.EnsureGrid(
                ref _controlPoints,
                ref _gridX,
                ref _gridY,
                ref _bakedGridX,
                ref _bakedGridY,
                _interpolation);
    }
}
