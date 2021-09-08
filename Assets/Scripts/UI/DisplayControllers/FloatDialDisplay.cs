using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace UI.DisplayControllers
{
    public class FloatDialDisplay : MonoBehaviour
    {
        public FloatReference reference;
        public float totalSpanLength;
        public bool clockwiseRotation = true;
        public float angleOffset = 90;

        public RectTransform hand;


        private void Awake()
        {
            reference.ValueChanges.TakeUntilDestroy(this)
                .StartWith(reference.CurrentValue)
                .Subscribe(next =>
                {
                    var percentage = next / totalSpanLength;
                    hand.eulerAngles = new Vector3(0, 0, percentage * 360 * (clockwiseRotation ? -1 : 1) + angleOffset);
                }).AddTo(this);
        }
    }
}