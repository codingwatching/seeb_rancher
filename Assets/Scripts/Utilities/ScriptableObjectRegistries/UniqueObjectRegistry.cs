using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utilities.ScriptableObjectRegistries
{
    [Serializable]
    public class IDableSavedReference
    {
        int referenceID;
        public IDableSavedReference(IDableObject target)
        {
            referenceID = target.myId;
        }

        public T GetObject<T>() where T: IDableObject
        {
            var registry = RegistryRegistry.GetObjectRegistry<T>();
            if(registry == null)
            {
                return null;
            }
            return registry.GetUniqueObjectFromID(referenceID);
        }
    }
    [Serializable]
    public abstract class IDableObject : ScriptableObject
    {
        public int myId;
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
                uniqueObject.myId = i;
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
