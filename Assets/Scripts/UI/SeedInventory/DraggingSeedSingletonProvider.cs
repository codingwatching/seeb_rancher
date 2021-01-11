using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class DraggingSeedSingletonProvider : MonoBehaviour
    {
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
    }
}
