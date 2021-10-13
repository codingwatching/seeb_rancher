using UnityEngine;

namespace UI.PlantData
{
    public class Turntable : MonoBehaviour
    {
        public float rotationSpeed;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
