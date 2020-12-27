using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    [Serializable]
    public abstract class IDableObject : ScriptableObject
    {
        public abstract void AssignId(int myNewID);
    }

    public abstract class UniqueObjectRegistry : ScriptableObject
    {
        public abstract IDableObject[] AllObjects { get; }

        private void Awake()
        {
            OnObjectSetChanged();
        }
        public virtual void OnObjectSetChanged()
        {

        }
        public void AssignAllIDs()
        {
            for (var i = 0; i < AllObjects.Length; i++)
            {
                var uniqueObject = AllObjects[i];
                uniqueObject.AssignId(i);
                EditorUtility.SetDirty(uniqueObject);
            }
            OnObjectSetChanged();
        }
    }

    public abstract class UniqueObjectRegistryWithAccess<T> : UniqueObjectRegistry where T : IDableObject
    {
        public T[] allObjects;
        public override IDableObject[] AllObjects => allObjects;

        public T GetUniqueObjectFromID(int id)
        {
            return allObjects[id];
        }
    }
}
