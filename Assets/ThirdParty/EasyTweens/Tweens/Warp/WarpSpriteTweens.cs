using System;
using EasyTweens;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Warp
{
    /// Morphs the whole deformation cage between two poses. Sculpt the grid in the scene
    /// view with the Warp Grid tool, press the tween's "set current as start", sculpt the
    /// second pose, then "set current as end".
    ///
    /// Poses are stored in normalized sprite space, so they survive the sprite being swapped
    /// for one of another size, but not the grid being resized: change Grid X or Grid Y and
    /// both ends need recapturing.
    [Serializable, TweenCategoryOverride("Sprite")]
    public class WarpSpriteGridTween : TargetedTween<WarpSprite, Vector2[]>
    {
        [NonSerialized] private Vector2[] _lerped;

        protected override Vector2[] Property
        {
            get => target == null ? null : target.GetControlPoints();
            set
            {
                if (target == null)
                    return;

                target.SetControlPoints(value);
#if UNITY_EDITOR
                EditorUtility.SetDirty(target);
#endif
            }
        }

        protected override Vector2[] Lerp(float factor)
        {
            if (startValue == null || endValue == null || startValue.Length != endValue.Length)
                return null;

            // Reused across frames: this runs every update and the cage is a fixed size.
            if (_lerped == null || _lerped.Length != startValue.Length)
                _lerped = new Vector2[startValue.Length];

            for (var i = 0; i < _lerped.Length; i++)
                _lerped[i] = Vector2.LerpUnclamped(startValue[i], endValue[i], factor);

            return _lerped;
        }
    }

    /// Moves a single control point of the cage. Several of these with staggered delays give
    /// ripple and wobble effects that a single pose morph cannot express.
    [Serializable, TweenCategoryOverride("Sprite")]
    public class WarpSpritePointTween : Vector2Tween<WarpSprite>
    {
        /// Row-major index into the cage: point (x, y) is at y * Grid X + x.
        [WarpPointIndex]
        [ExposeInEditor(tooltip: "Row-major index into the grid: y * Grid X + x.")]
        public int pointIndex;

        private bool HasPoint => target != null && pointIndex >= 0 && pointIndex < target.ControlPointCount;

        protected override Vector2 Property
        {
            get => HasPoint ? target.GetControlPoint(pointIndex) : Vector2.zero;
            set
            {
                if (!HasPoint)
                    return;

                target.SetControlPoint(pointIndex, value);
#if UNITY_EDITOR
                EditorUtility.SetDirty(target);
#endif
            }
        }
    }
}
