using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace EasyTweens
{
    /// Deforms a SpriteRenderer through a control-point cage, the sprite counterpart of
    /// WarpImage. A curved warp cannot be expressed by moving four corners, so the sprite is
    /// re-tessellated into a grid and every vertex is displaced by the cage.
    ///
    /// The renderer keeps drawing through a real SpriteRenderer, so sorting, masking, 2D
    /// lights and materials all behave exactly as they did before. The tessellated geometry
    /// lives on a private clone of the sprite: writing it into the shared asset would deform
    /// every other renderer using the same sprite.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Rendering/Warp Sprite")]
    public sealed class WarpSprite : MonoBehaviour, IWarpCage
    {
        public const int MinResolution = 1;
        public const int MaxResolution = 64;

        [SerializeField, Range(WarpCage.MinGridSize, WarpCage.MaxGridSize)] private int _gridX = 3;
        [SerializeField, Range(WarpCage.MinGridSize, WarpCage.MaxGridSize)] private int _gridY = 3;
        [SerializeField, Range(MinResolution, MaxResolution)] private int _resolution = 12;
        [SerializeField] private WarpInterpolation _interpolation = WarpInterpolation.Bezier;

        [SerializeField, HideInInspector] private Vector2[] _controlPoints;
        [SerializeField, HideInInspector] private int _bakedGridX;
        [SerializeField, HideInInspector] private int _bakedGridY;

        private SpriteRenderer _renderer;

        /// Sprite the user assigned. Kept so the original can be handed back whenever the
        /// cage is flat and when the component is disabled.
        private Sprite _source;

        /// Our tessellated copy. Owned by this component and destroyed with it.
        private Sprite _generated;
        private Sprite _generatedFrom;
        private int _generatedResolution;

        private Vector3[] _positions;
        private Vector2[] _uvs;
        private ushort[] _indices;

        public int GridX => _gridX;
        public int GridY => _gridY;
        public Transform Transform => transform;

        public int LastVertexCount { get; private set; }
        public int LastTriangleCount { get; private set; }

        /// The undeformed sprite in local units, pivot included. Plays the part the
        /// RectTransform's rect plays for WarpImage.
        public Rect LocalRect
        {
            get
            {
                var sprite = Source;
                if (sprite == null || sprite.rect.width <= 0f || sprite.rect.height <= 0f)
                    return new Rect(-0.5f, -0.5f, 1f, 1f);

                var size = sprite.rect.size / sprite.pixelsPerUnit;
                var pivot = new Vector2(
                    sprite.pivot.x / sprite.rect.width,
                    sprite.pivot.y / sprite.rect.height);

                return new Rect(-Vector2.Scale(pivot, size), size);
            }
        }

        public int ControlPointCount
        {
            get
            {
                EnsureGrid();
                return _controlPoints.Length;
            }
        }

        private Sprite Source
        {
            get
            {
                SyncSource();
                return _source;
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
            Rebuild();
        }

        /// Snapshot of the whole cage, safe to keep: it is a copy, not the live buffer.
        public Vector2[] GetControlPoints()
        {
            EnsureGrid();
            var snapshot = new Vector2[_controlPoints.Length];
            Array.Copy(_controlPoints, snapshot, snapshot.Length);
            return snapshot;
        }

        /// Applies a whole cage at once, rebuilding only if something moved. A snapshot taken
        /// at a different grid resolution is rejected, since the points carry no dimensions
        /// of their own and would land in the wrong rows.
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
                Rebuild();
        }

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

            Rebuild();
        }

        public Vector2 NormalizedToLocal(Vector2 normalized) =>
            WarpCage.NormalizedToLocal(LocalRect, normalized);

        public Vector2 LocalToNormalized(Vector2 local) =>
            WarpCage.LocalToNormalized(LocalRect, local);

        public Vector2 EvaluateLocal(Vector2 t)
        {
            EnsureGrid();
            return NormalizedToLocal(WarpCage.Evaluate(_controlPoints, _gridX, _gridY, _interpolation, t));
        }

        public void Rebuild()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            SyncSource();
            EnsureGrid();

            if (_source == null || _source.texture == null)
            {
                ReleaseGenerated();
                return;
            }

            // A flat cage costs nothing: hand the untouched asset back and drop our copy, so
            // an idle WarpSprite renders exactly like a plain SpriteRenderer.
            if (!WarpCage.IsWarped(_controlPoints, _gridX, _gridY))
            {
                ReleaseGenerated();
                return;
            }

            if (_generated == null || _generatedFrom != _source || _generatedResolution != _resolution)
                CreateGenerated();

            WritePositions();

            if (_renderer.sprite != _generated)
                _renderer.sprite = _generated;
        }

        private void OnEnable()
        {
            _renderer = GetComponent<SpriteRenderer>();
            Rebuild();
        }

        private void OnDisable() => ReleaseGenerated();

        private void OnDestroy() => ReleaseGenerated();

        /// Catches sprite swaps made by animation or gameplay after the cage was built.
        private void LateUpdate()
        {
            if (_renderer == null)
                return;

            var current = _renderer.sprite;
            if (current != _generated && current != _source)
                Rebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _gridX = Mathf.Clamp(_gridX, WarpCage.MinGridSize, WarpCage.MaxGridSize);
            _gridY = Mathf.Clamp(_gridY, WarpCage.MinGridSize, WarpCage.MaxGridSize);
            _resolution = Mathf.Clamp(_resolution, MinResolution, MaxResolution);

            // Deferred: OnValidate runs during deserialization, where creating objects and
            // touching the renderer is not allowed.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && isActiveAndEnabled)
                    Rebuild();
            };
        }
#endif

        /// Notices whichever sprite the user, an animation or a tween last assigned, ignoring
        /// the one we generated ourselves.
        private void SyncSource()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            if (_renderer == null)
                return;

            var current = _renderer.sprite;
            if (current != _generated)
                _source = current;
        }

        private void EnsureGrid() =>
            WarpCage.EnsureGrid(
                ref _controlPoints,
                ref _gridX,
                ref _gridY,
                ref _bakedGridX,
                ref _bakedGridY,
                _interpolation);

        /// Builds the private sprite copy and its fixed topology. UVs and indices depend only
        /// on the resolution, so they are uploaded once and only positions move afterwards.
        private void CreateGenerated()
        {
            ReleaseGenerated();

            var pivot = new Vector2(
                _source.rect.width > 0f ? _source.pivot.x / _source.rect.width : 0.5f,
                _source.rect.height > 0f ? _source.pivot.y / _source.rect.height : 0.5f);

            var secondaryCount = _source.GetSecondaryTextureCount();
            if (secondaryCount > 0)
            {
                var secondary = new SecondarySpriteTexture[secondaryCount];
                _source.GetSecondaryTextures(secondary);
                _generated = Sprite.Create(
                    _source.texture, _source.textureRect, pivot, _source.pixelsPerUnit,
                    0, SpriteMeshType.FullRect, _source.border, false, secondary);
            }
            else
            {
                _generated = Sprite.Create(
                    _source.texture, _source.textureRect, pivot, _source.pixelsPerUnit,
                    0, SpriteMeshType.FullRect, _source.border);
            }

            _generated.name = _source.name + " (Warped)";
            _generated.hideFlags = HideFlags.HideAndDontSave;
            _generatedFrom = _source;
            _generatedResolution = _resolution;

            BuildTopology();
        }

        private void BuildTopology()
        {
            var cells = Mathf.Max(MinResolution, _resolution);
            var side = cells + 1;
            var vertexCount = side * side;

            _positions = new Vector3[vertexCount];
            _uvs = new Vector2[vertexCount];
            _indices = new ushort[cells * cells * 6];

            var texture = _source.texture;
            var textureRect = _source.textureRect;
            var uvMin = new Vector2(textureRect.xMin / texture.width, textureRect.yMin / texture.height);
            var uvMax = new Vector2(textureRect.xMax / texture.width, textureRect.yMax / texture.height);

            for (var y = 0; y < side; y++)
            {
                var v = y / (float)cells;
                for (var x = 0; x < side; x++)
                {
                    var u = x / (float)cells;
                    _uvs[y * side + x] = new Vector2(
                        Mathf.Lerp(uvMin.x, uvMax.x, u),
                        Mathf.Lerp(uvMin.y, uvMax.y, v));
                }
            }

            var index = 0;
            for (var y = 0; y < cells; y++)
            {
                for (var x = 0; x < cells; x++)
                {
                    var bottomLeft = (ushort)(y * side + x);
                    var bottomRight = (ushort)(bottomLeft + 1);
                    var topLeft = (ushort)(bottomLeft + side);
                    var topRight = (ushort)(topLeft + 1);

                    _indices[index++] = bottomLeft;
                    _indices[index++] = topLeft;
                    _indices[index++] = bottomRight;
                    _indices[index++] = bottomRight;
                    _indices[index++] = topLeft;
                    _indices[index++] = topRight;
                }
            }

            _generated.SetVertexCount(vertexCount);
            Upload(_generated, VertexAttribute.TexCoord0, _uvs);

            var indices = new NativeArray<ushort>(_indices, Allocator.Temp);
            _generated.SetIndices(indices);
            indices.Dispose();

            LastVertexCount = vertexCount;
            LastTriangleCount = _indices.Length / 3;
        }

        private void WritePositions()
        {
            var cells = Mathf.Max(MinResolution, _resolution);
            var side = cells + 1;
            var rect = LocalRect;

            for (var y = 0; y < side; y++)
            {
                var v = y / (float)cells;
                for (var x = 0; x < side; x++)
                {
                    var t = new Vector2(x / (float)cells, v);
                    var warped = WarpCage.Evaluate(_controlPoints, _gridX, _gridY, _interpolation, t);
                    var local = WarpCage.NormalizedToLocal(rect, warped);
                    _positions[y * side + x] = new Vector3(local.x, local.y, 0f);
                }
            }

            Upload(_generated, VertexAttribute.Position, _positions);
        }

        private static void Upload<T>(Sprite sprite, VertexAttribute attribute, T[] data) where T : struct
        {
            var native = new NativeArray<T>(data, Allocator.Temp);
            sprite.SetVertexAttribute(attribute, native);
            native.Dispose();
        }

        private void ReleaseGenerated()
        {
            if (_renderer != null && _generated != null && _renderer.sprite == _generated)
                _renderer.sprite = _source;

            if (_generated == null)
                return;

            if (Application.isPlaying)
                Destroy(_generated);
            else
                DestroyImmediate(_generated);

            _generated = null;
            _generatedFrom = null;
            LastVertexCount = 0;
            LastTriangleCount = 0;
        }
    }
}
