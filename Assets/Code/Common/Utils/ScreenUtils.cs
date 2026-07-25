using UnityEngine;

namespace Code.Common.Utils
{
    public static class ScreenUtils
    {
        public static Vector2 ScreenToWorld2D(Camera camera, Vector2 screen)
        {
            if (camera.orthographic)
            {
                var world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
                return new Vector2(world.x, world.y);
            }

            var ray = camera.ScreenPointToRay(screen);
            var distance = Mathf.Abs(ray.direction.z) > 0.0001f ? -ray.origin.z / ray.direction.z : 0f;
            var pointOnPlane = ray.GetPoint(distance);
            return new Vector2(pointOnPlane.x, pointOnPlane.y);
        }
    }
}