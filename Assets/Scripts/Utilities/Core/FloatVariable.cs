using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "State/FloatVariable", order = 2)]
    public class FloatVariable : GenericVariable<float>
    {
        public float Add(float extraValue)
        {
            var newValue = CurrentValue + extraValue;
            SetValue(newValue);
            return newValue;
        }

    }
}
