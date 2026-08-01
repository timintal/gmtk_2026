#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens.Editor
{
    /// Slider across the target's control points, so an index can be picked by dragging and
    /// watching which point lights up in the scene view instead of by counting grid rows.
    [CustomPropertyDrawer(typeof(WarpPointIndexAttribute))]
    public sealed class WarpPointIndexDrawer : PropertyDrawer
    {
        /// Largest index any cage can hold. Stands in while the target is unknown, so that
        /// clearing the target field does not clamp a perfectly good index down to zero.
        private const int UnboundedMax = WarpCage.MaxGridSize * WarpCage.MaxGridSize - 1;

        private const long GridPollInterval = 250;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var cage = ResolveTarget(property);

            var slider = new SliderInt(property.displayName, 0, MaxIndex(cage))
            {
                bindingPath = property.propertyPath,
                showInputField = true,
            };
            slider.AddToClassList("customLabel");

            void RefreshRange()
            {
                // Never tighter than the stored index: shrinking the grid would otherwise
                // rewrite a value the user chose, silently and without an undo step to
                // notice. Dragging back into range pulls the limit down to the real maximum.
                var max = Mathf.Max(MaxIndex(cage), slider.value);
                if (slider.highValue != max)
                    slider.highValue = max;
            }

            var targetProperty = FindTargetProperty(property);
            if (targetProperty != null)
            {
                slider.TrackPropertyValue(targetProperty, changed =>
                {
                    cage = changed.objectReferenceValue as IWarpCage;
                    RefreshRange();
                });
            }

            // A grid resize changes the range without touching this serialized object, so the
            // only way to notice is to look. The scheduler stops when the row leaves the panel.
            slider.schedule.Execute(RefreshRange).Every(GridPollInterval);

            // Tracking the property rather than the field means the initial bind stays quiet
            // and undo or an external edit pings just the same as dragging does.
            slider.TrackPropertyValue(property, changed =>
            {
                RefreshRange();
                WarpPointPing.Ping(cage, changed.intValue);
            });

            return slider;
        }

        private static int MaxIndex(IWarpCage cage) =>
            cage == null ? UnboundedMax : Mathf.Max(0, cage.ControlPointCount - 1);

        private static IWarpCage ResolveTarget(SerializedProperty property) =>
            FindTargetProperty(property)?.objectReferenceValue as IWarpCage;

        /// The tween stores its target in a sibling field of the same list element, so the
        /// target is one path segment away from the index being drawn.
        private static SerializedProperty FindTargetProperty(SerializedProperty property)
        {
            var path = property.propertyPath;
            var lastSeparator = path.LastIndexOf('.');
            if (lastSeparator < 0)
                return null;

            return property.serializedObject.FindProperty(path[..(lastSeparator + 1)] + "target");
        }
    }
}
#endif
