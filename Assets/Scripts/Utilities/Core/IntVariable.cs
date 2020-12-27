using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "State/IntVariable", order = 2)]
    public class IntVariable : GenericVariable<int>
    {
        public float Add(int extraValue)
        {
            var newValue = CurrentValue + extraValue;
            SetValue(newValue);
            return newValue;
        }
    }
}
