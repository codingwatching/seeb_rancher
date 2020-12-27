using System;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    [CreateAssetMenu(fileName = "EventGroup", menuName = "State/EventGroup", order = 10)]
    public class EventGroup: ScriptableObject
    {
        public event Action OnEvent;

        public void TriggerEvent()
        {
            OnEvent?.Invoke();
        }
    }
}
