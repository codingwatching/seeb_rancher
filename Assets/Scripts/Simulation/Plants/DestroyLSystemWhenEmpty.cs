using Dman.LSystem.UnityObjects;
using UnityEngine;

namespace Simulation.Plants
{
    [RequireComponent(typeof(LSystemBehavior))]
    [RequireComponent(typeof(MeshFilter))]
    public class DestroyLSystemWhenEmpty : MonoBehaviour
    {
        public GameObject prefabRoot;

        public int maximumConsecutiveEmptyUpdates = 10;

        private int numberOfEmptyUpdates = 0;

        private void Awake()
        {
            this.GetComponent<LSystemBehavior>().OnSystemStateUpdated += SystemWasUpdated;
        }

        private void OnDestroy()
        {
            this.GetComponent<LSystemBehavior>().OnSystemStateUpdated -= SystemWasUpdated;
        }

        private void SystemWasUpdated()
        {
            var mesh = GetComponent<MeshFilter>();
            if (mesh.mesh.vertexCount < 5)
            {
                numberOfEmptyUpdates++;
            }
            else
            {
                numberOfEmptyUpdates = 0;
            }
            if (numberOfEmptyUpdates >= maximumConsecutiveEmptyUpdates)
            {
                Destroy(prefabRoot);
            }
        }

    }
}
