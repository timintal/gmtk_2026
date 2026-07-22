using UnityEngine;

namespace Code.Utils
{
    public static class VectorUtils
    {
        public static Vector3 XY0(this Vector2 v) => new Vector3(v.x, v.y, 0);
    }
}