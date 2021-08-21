using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MyUtilities
{
    public static class SerializationInfoExtensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }
    }

    public static class ListExtensions
    {
        private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class VectorUtils
    {
        public static Vector3 VectorMax(params Vector3[] vectors)
        {
            if (vectors.Length <= 0)
            {
                return default;
            }
            var maxVector = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vect = vectors[i];
                maxVector.x = Mathf.Max(maxVector.x, vect.x);
                maxVector.y = Mathf.Max(maxVector.y, vect.y);
                maxVector.z = Mathf.Max(maxVector.z, vect.z);
            }
            return maxVector;
        }
        public static Vector3 VectorMin(params Vector3[] vectors)
        {
            if (vectors.Length <= 0)
            {
                return default;
            }
            var minVector = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vect = vectors[i];
                minVector.x = Mathf.Min(minVector.x, vect.x);
                minVector.y = Mathf.Min(minVector.y, vect.y);
                minVector.z = Mathf.Min(minVector.z, vect.z);
            }
            return minVector;
        }
        public static Vector3 Average(params Vector3[] vectors)
        {
            if (vectors.Length <= 0)
            {
                return default;
            }
            var sumVector = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                sumVector += vectors[i];
            }
            return sumVector / vectors.Length;
        }
    }

    public static class DrawArrow
    {
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}
