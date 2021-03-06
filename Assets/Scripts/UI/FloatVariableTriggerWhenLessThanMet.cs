using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class FloatVariableTriggerWhenLessThanMet : MonoBehaviour
    {
        public FloatVariable variable;
        public float boundaryTrigger;

        public UnityEvent lessThanEqualTrigger;
        public UnityEvent greaterThanTrigger;

        private void Awake()
        {
            variable.Value.TakeUntilDestroy(this)
                .Subscribe(nextValue =>
                {
                    if (nextValue <= boundaryTrigger)
                    {
                        lessThanEqualTrigger?.Invoke();
                    }
                    else
                    {
                        greaterThanTrigger?.Invoke();
                    }
                }).AddTo(this);
        }
    }
}