using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    // Adds a custom <invert>...</invert> rich text tag to TextMeshPro (UGUI).
    // The span is drawn as a solid rectangle in the text face color with the glyphs
    // recolored to the background color, so the letters read as "cut out" against a
    // flat surface.
    //
    // Implementation:
    //  - <invert> is rewritten into <link="invert"> via ITextPreprocessor (no layout impact).
    //  - After the mesh is generated (TEXT_CHANGED) we read textInfo.linkInfo, recolor the
    //    span's glyphs, and drive a TMPInvertBackground sibling that renders the face-color
    //    rectangle BEHIND the glyphs. All background updates go through CanvasRenderer.SetMesh
    //    so they are safe inside TMP's graphic-rebuild loop.
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMPInvertTag : MonoBehaviour, ITextPreprocessor
    {
        const string OpenTag = "<invert>";
        const string CloseTag = "</invert>";
        const string LinkId = "invert";
        const string BackgroundNamePrefix = "InvertBackground(temp)_";

        [SerializeField] TMP_Text _text;

        // Must be unique per component: multiple invert labels often share a parent, and a
        // shared background means a sibling with no <invert> span will Clear() ours.
        string BackgroundName => BackgroundNamePrefix + GetEntityId();

        [Tooltip("Color the glyphs are painted inside an <invert> span (the flat surface color the letters sit on).")]
        [SerializeField] Color _backgroundColor = Color.black;

        [Tooltip("When on, the rectangle uses the current text color as its face color. When off, Face Color Override is used.")]
        [SerializeField] bool _useTextColorAsFace = true;

        [SerializeField] Color _faceColorOverride = Color.white;

        readonly List<Rect> _rects = new();
        TMPInvertBackground _background;
        bool _isApplying;

        void Reset() => _text = GetComponent<TMP_Text>();

        void OnEnable()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            _text.textPreprocessor = this;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            EnsureBackground();
            _text.ForceMeshUpdate();
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            if (_text != null && ReferenceEquals(_text.textPreprocessor, this))
                _text.textPreprocessor = null;

            if (_background != null)
                _background.Clear();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            // Defer: creating/dirtying objects directly inside OnValidate is not allowed.
            UnityEditor.EditorApplication.delayCall += EditorReapply;
        }

        void EditorReapply()
        {
            if (this == null || _text == null || !isActiveAndEnabled)
                return;

            _text.textPreprocessor = this;
            EnsureBackground();
            _text.ForceMeshUpdate();
        }
#endif

        public string PreprocessText(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(OpenTag, StringComparison.Ordinal) < 0)
                return text;

            return text
                .Replace(OpenTag, $"<link=\"{LinkId}\">")
                .Replace(CloseTag, "</link>");
        }

        void OnTextChanged(UnityEngine.Object obj)
        {
            if (_isApplying || !ReferenceEquals(obj, _text))
                return;

            _isApplying = true;
            try
            {
                ApplyInvert();
            }
            finally
            {
                _isApplying = false;
            }
        }

        void ApplyInvert()
        {
            var info = _text.textInfo;
            if (info == null)
            {
                _background?.Clear();
                return;
            }

            var faceColor = _useTextColorAsFace ? _text.color : _faceColorOverride;
            faceColor.a = 1f;

            var glyphColor = _backgroundColor;
            glyphColor.a = 1f;
            Color32 glyphColor32 = glyphColor;

            // Only draw behind characters currently revealed by the typewriter / paging.
            // isVisible ignores these, so clamp explicitly.
            var firstVisible = _text.firstVisibleCharacter;
            var maxVisible = _text.maxVisibleCharacters;

            _rects.Clear();
            var recolored = false;

            for (var l = 0; l < info.linkCount; l++)
            {
                var link = info.linkInfo[l];
                if (!link.GetLinkID().Equals(LinkId, StringComparison.Ordinal))
                    continue;

                var first = link.linkTextfirstCharacterIndex;
                var last = first + link.linkTextLength - 1;

                var runLine = -1;
                float xMin = 0f, xMax = 0f;

                for (var c = first; c <= last && c < info.characterCount; c++)
                {
                    if (c < firstVisible || c >= maxVisible)
                        continue;

                    ref readonly var ch = ref info.characterInfo[c];

                    if (ch.isVisible)
                    {
                        var colors = info.meshInfo[ch.materialReferenceIndex].colors32;
                        var vi = ch.vertexIndex;
                        colors[vi + 0] = glyphColor32;
                        colors[vi + 1] = glyphColor32;
                        colors[vi + 2] = glyphColor32;
                        colors[vi + 3] = glyphColor32;
                        recolored = true;
                    }

                    if (ch.lineNumber != runLine)
                    {
                        if (runLine >= 0)
                            AddRect(info, runLine, xMin, xMax);

                        runLine = ch.lineNumber;
                        xMin = ch.bottomLeft.x;
                        xMax = ch.topRight.x;
                    }
                    else
                    {
                        xMin = Mathf.Min(xMin, ch.bottomLeft.x);
                        xMax = Mathf.Max(xMax, ch.topRight.x);
                    }
                }

                if (runLine >= 0)
                    AddRect(info, runLine, xMin, xMax);
            }

            if (recolored)
                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (_rects.Count == 0)
            {
                _background?.Clear();
                return;
            }

            if (!EnsureBackground())
                return;

            if (!CanvasUpdateRegistry.IsRebuildingGraphics())
                SyncBackgroundTransform();

            _background.SetRects(_rects, faceColor);
        }

        void AddRect(TMP_TextInfo info, int lineNumber, float xMin, float xMax)
        {
            var line = info.lineInfo[lineNumber];
            _rects.Add(Rect.MinMaxRect(xMin, line.descender, xMax, line.ascender));
        }

        bool EnsureBackground()
        {
            if (_background != null)
                return true;

            var parent = _text.transform.parent != null ? _text.transform.parent : _text.transform;
            var backgroundName = BackgroundName;

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == backgroundName && child.TryGetComponent(out TMPInvertBackground existing))
                {
                    _background = existing;
                    return true;
                }
            }

            // Creating a graphic mid-rebuild would itself request a rebuild → skip this pass.
            if (CanvasUpdateRegistry.IsRebuildingGraphics())
                return false;

            var go = new GameObject(backgroundName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            _background = go.AddComponent<TMPInvertBackground>();
            _background.raycastTarget = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
#endif
            SyncBackgroundTransform();
            return true;
        }

        void SyncBackgroundTransform()
        {
            if (_background == null)
                return;

            var src = _text.rectTransform;
            var dst = _background.rectTransform;

            if (dst.anchorMin != src.anchorMin) dst.anchorMin = src.anchorMin;
            if (dst.anchorMax != src.anchorMax) dst.anchorMax = src.anchorMax;
            if (dst.pivot != src.pivot) dst.pivot = src.pivot;
            if (dst.sizeDelta != src.sizeDelta) dst.sizeDelta = src.sizeDelta;
            if (dst.anchoredPosition3D != src.anchoredPosition3D) dst.anchoredPosition3D = src.anchoredPosition3D;
            if (dst.localScale != src.localScale) dst.localScale = src.localScale;
            if (dst.localRotation != src.localRotation) dst.localRotation = src.localRotation;

            var textIndex = _text.transform.GetSiblingIndex();
            if (dst.GetSiblingIndex() >= textIndex)
                dst.SetSiblingIndex(textIndex);
        }
    }
}