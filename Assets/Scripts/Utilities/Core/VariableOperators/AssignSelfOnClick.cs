using UnityEngine;

namespace Assets.Scripts.Utilities.Core.VariableOperators
{
    [RequireComponent(typeof(Collider2D))]
    public class AssignSelfOnClick : MonoBehaviour
    {
        public GameObjectVariable variableToSet;

        private void SetToVariable()
        {
            variableToSet.SetValue(gameObject);
        }
        private void OnMouseDown()
        {
            Debug.Log($"Clicked: {gameObject.name}");
            SetToVariable();
        }
    }
}
