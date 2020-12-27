using UnityEngine;

namespace Assets.Scripts.Utilities.Core.VariableOperators
{
    public class BooleanToggle : MonoBehaviour
    {
        public BooleanReference variableToToggle;

        public void Toggle()
        {
            variableToToggle.SetValue(!variableToToggle.CurrentValue);
        }
    }
}
