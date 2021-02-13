using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class DraggingSeedSingletonProvider : MonoBehaviour
    {
        // TODO: get rid of this
        public GameObjectVariable currentDraggingSeeds;
        public DraggingSeeds draggingSeedPrefab;

        public DraggingSeeds SpawnNewDraggingSeedsOrGetCurrent()
        {
            if (currentDraggingSeeds.CurrentValue == null)
            {
                var newDragger = Instantiate(draggingSeedPrefab, transform);
                currentDraggingSeeds.SetValue(newDragger.gameObject);
                return newDragger;
            }
            var dragger = currentDraggingSeeds.CurrentValue.GetComponent<DraggingSeeds>();
            return dragger;
        }
        public DraggingSeeds SpawnNewDraggingSeeds()
        {
            if (currentDraggingSeeds.CurrentValue != null)
            {
                Debug.LogError("Overwriting existing dragging seeds object instance");
                Destroy(currentDraggingSeeds.CurrentValue.gameObject);
            }

            var newDragger = Instantiate(draggingSeedPrefab, transform);
            currentDraggingSeeds.SetValue(newDragger.gameObject);
            return newDragger;
        }
    }
}
