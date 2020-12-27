using UnityEngine;

namespace Assets
{
    public static class MyUtilities
    {
        public static Vector2 GetMousePos2D()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(new Vector3(0, 0, 1), 0);

            if (plane.Raycast(ray, out var enter))
            {
                return ray.GetPoint(enter);
            }
            return default;
        }
    }
    public static class VectorExt
    {
        public static Vector2 InverseScale(this Vector2 source, Vector2 scale)
        {
            return new Vector2(source.x / scale.x, source.y / scale.y);
        }
    }
    public static class ComponentExtensions
    {
        public static void DestroyAllChildren(this GameObject gameObject)
        {
            for (int i = gameObject.transform.childCount; i > 0; --i)
            {
                Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            }
        }
    }
}
