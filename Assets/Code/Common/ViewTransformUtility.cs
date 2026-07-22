using UnityEngine;

namespace Code.Common
{
    internal static class ViewTransformUtility
    {

        public static Vector2 ReadPosition(Transform transform)
        {
            return transform is RectTransform rectTransform
                ? rectTransform.localPosition
                : new Vector2(transform.position.x, transform.position.y);
        }

        public static void WritePosition(Transform transform, Vector2 position, float preservedZ)
        {
            if (transform is RectTransform rectTransform)
            {
                rectTransform.localPosition = new Vector3(position.x, position.y, preservedZ);
                return;
            }

            transform.position = new Vector3(position.x, position.y, preservedZ);
        }

        public static float ReadPreservedZ(Transform transform)
        {
            return transform is RectTransform rectTransform
                ? rectTransform.localPosition.z
                : transform.position.z;
        }
    }
}
