using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    // Draws a set of solid-color rectangles behind an <invert> span. Managed entirely
    // by TMPInvertTag. Geometry is pushed straight to the CanvasRenderer via SetMesh
    // (never SetVerticesDirty / color / RectTransform changes), so it can be updated
    // safely from inside TMP's TEXT_CHANGED callback, which fires during the canvas
    // graphic-rebuild loop where requesting another rebuild throws.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TMPInvertBackground : MaskableGraphic
    {
        readonly List<Rect> _rects = new();
        Color _rectColor = Color.white;
        Mesh _mesh;
        VertexHelper _vh;

        public void SetRects(List<Rect> rects, Color rectColor)
        {
            _rects.Clear();
            if (rects != null)
                _rects.AddRange(rects);

            _rectColor = rectColor;
            RenderImmediate();
        }

        public void Clear()
        {
            if (_rects.Count == 0)
                return;

            _rects.Clear();
            RenderImmediate();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // Fallback path when Unity rebuilds this graphic on its own (enable, canvas
            // resize, masking change). Uses the cached rects/color.
            Populate(vh);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_mesh != null)
                DestroyImmediate(_mesh);
            _vh?.Dispose();
        }

        void Populate(VertexHelper vh)
        {
            vh.Clear();

            var col = (Color32)_rectColor;
            for (var i = 0; i < _rects.Count; i++)
            {
                var r = _rects[i];
                var idx = vh.currentVertCount;

                vh.AddVert(new Vector3(r.xMin, r.yMin), col, Vector2.zero);
                vh.AddVert(new Vector3(r.xMin, r.yMax), col, Vector2.zero);
                vh.AddVert(new Vector3(r.xMax, r.yMax), col, Vector2.zero);
                vh.AddVert(new Vector3(r.xMax, r.yMin), col, Vector2.zero);

                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
                vh.AddTriangle(idx + 0, idx + 2, idx + 3);
            }
        }

        void RenderImmediate()
        {
            if (canvasRenderer == null)
                return;

            _mesh ??= new Mesh { name = "InvertBackground", hideFlags = HideFlags.HideAndDontSave };
            _vh ??= new VertexHelper();

            Populate(_vh);
            _vh.FillMesh(_mesh);

            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            canvasRenderer.SetTexture(mainTexture);
            canvasRenderer.SetMesh(_mesh);
        }
    }
}
