using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public abstract class LiveObjectSet<T> : ScriptableObject
    {
        public T[] InitialObjectsForInspector;

        private void OnEnable()
        {
            ItemSet = new HashSet<T>(InitialObjectsForInspector);
            OnItemSetChanged?.Invoke();
        }

        private ISet<T> ItemSet = new HashSet<T>();

        public event Action OnItemSetChanged;
        public bool AddItem(T item)
        {
            if (ItemSet.Add(item))
            {
                OnItemSetChanged?.Invoke();
                return true;
            }
            return false;
        }
        public bool RemoveItem(T item)
        {
            if (ItemSet.Remove(item))
            {
                OnItemSetChanged?.Invoke();
                return true;
            }
            return false;
        }

        public IEnumerable<T> GetAll()
        {
            return ItemSet;
        }
    }
}
