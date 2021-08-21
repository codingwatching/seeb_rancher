using UnityEngine;
using UnityEngine.Events;

namespace MyUtilities
{
    public class InitializationEvents : MonoBehaviour
    {
        public UnityEvent onAwake;
        public UnityEvent onStart;

        private void Awake()
        {
            onAwake?.Invoke();
        }

        void Start()
        {
            onStart?.Invoke();
        }
    }
}