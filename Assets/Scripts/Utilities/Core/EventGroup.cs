using System;
using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    [CreateAssetMenu(fileName = "EventGroup", menuName = "State/EventGroup", order = 10)]
    public class EventGroup : ScriptableObject
    {
        public event Action OnEvent;
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif


        public void TriggerEvent()
        {
            OnEvent?.Invoke();
        }
    }
}
