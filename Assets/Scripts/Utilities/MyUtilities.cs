using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        public static bool RaycastToObject(LayerMask mask, out RaycastHit hit)
        {
            hit = default;
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var innerHit, 100, mask))
                {
                    hit = innerHit;
                    return true;
                }
            }
            return false;
        }

        public static RaycastHit[] RaycastAllToObject(LayerMask mask)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.RaycastAll(ray, 100, mask);
        }
    }
    public static class ArrayExt
    {
        public static T[][] Pivot<T>(this T[][] source)
        {
            if(source.Length <= 0 || source[0].Length <= 0)
            {
                throw new System.Exception("invalid pivot");
            }
            var result = new T[source[0].Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new T[source.Length];
                for (int j = 0; j < result[i].Length; j++)
                {
                    result[i][j] = source[j][i];
                }
            }
            return result;
        }

        /// <summary>
        /// return a list of indexes of length <paramref name="numberOfIndexes"/> varying from 0 to <paramref name="sizeOfIndexedSpace"/>
        ///     such that no number is repeated twice
        /// </summary>
        /// <param name="numberOfIndexes"></param>
        /// <param name="sizeOfIndexedSpace"></param>
        /// <returns></returns>
        public static int[] SelectIndexSources(int numberOfIndexes, int sizeOfIndexedSpace)
        {
            if (numberOfIndexes > sizeOfIndexedSpace)
            {
                throw new System.Exception("must have at least as many parents as chromosome copies");
            }
            var selectedParents = new HashSet<int>();
            var resultSelectedParentsPerDuplicate = new int[numberOfIndexes];
            for (int i = 0; i < resultSelectedParentsPerDuplicate.Length; i++)
            {
                int nextSelectedParent;
                do
                {
                    nextSelectedParent = Random.Range(0, sizeOfIndexedSpace);
                } while (selectedParents.Contains(nextSelectedParent));
                selectedParents.Add(nextSelectedParent);
                resultSelectedParentsPerDuplicate[i] = nextSelectedParent;
            }
            return resultSelectedParentsPerDuplicate;
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
        public static void DestroyAllChildren(this GameObject gameObject, System.Func<GameObject, bool> predicate)
        {
            for (int i = gameObject.transform.childCount; i > 0; --i)
            {
                var child = gameObject.transform.GetChild(0).gameObject;
                if (predicate(child))
                {
                    Object.DestroyImmediate(child);
                }
            }
        }
    }
}
