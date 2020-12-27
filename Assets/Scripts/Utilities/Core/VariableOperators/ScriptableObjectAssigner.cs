using UnityEngine;

namespace Assets.Scripts.Utilities.Core.VariableOperators
{
    public class ScriptableObjectAssigner : MonoBehaviour
    {
        public ScriptableObjectVariable variableToSet;
        public ScriptableObject objectToAssign;

        public bool AssignOnInit = false;

        private void Awake()
        {
            if (AssignOnInit)
            {
                SetToVariable();
            }
        }

        public void SetToVariable()
        {
            variableToSet.SetValue(objectToAssign);
        }
    }
}
