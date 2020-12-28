using Assets.Scripts.Utilities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class DraggingSeedSingletonProvider: MonoBehaviour
    {
        public GameObjectVariable currentDraggingSeeds;
        public DraggingSeeds draggingSeedPrefab;

        public bool TryAddToSeed(Seed[] newSeeds)
        {
            if(currentDraggingSeeds.CurrentValue == null)
            {
                var newDragger = Instantiate(draggingSeedPrefab, transform);
                currentDraggingSeeds.SetValue(newDragger.gameObject);
            }

            var dragger = currentDraggingSeeds.CurrentValue.GetComponent<DraggingSeeds>();
            return dragger.TryAddSeedsToSet(newSeeds);
        }
    }
}
