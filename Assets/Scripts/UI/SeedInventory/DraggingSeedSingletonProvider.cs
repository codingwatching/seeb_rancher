using UnityEngine;

namespace UI.SeedInventory
{
    public class DraggingSeedSingletonProvider : MonoBehaviour
    {
        private GameObject currentDraggingSeedsInstance;
        public SeedBucketDisplay draggingSeedPrefab;
        public SeedBucketDisplay SpawnNewDraggingSeeds()
        {
            if (currentDraggingSeedsInstance != null)
            {
                Debug.LogError("Overwriting existing dragging seeds object instance");
                Destroy(currentDraggingSeedsInstance);
                currentDraggingSeedsInstance = null;
            }

            var newDragger = Instantiate(draggingSeedPrefab, transform);
            currentDraggingSeedsInstance = newDragger.gameObject;
            return newDragger;
        }
    }
}
