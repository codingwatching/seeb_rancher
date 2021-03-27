using Assets.Scripts.DataModels;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour
    {
        [Tooltip("The gameobject which all SeedInventoryDropSlots are directly nested under")]
        public GameObject seedGridLayoutParent;

        public SeedInventoryDropSlot trashSlot;

        public static SeedInventoryController Instance;

        /// <summary>
        /// finds the first open drop slot, and puts the <paramref name="seedStack"/> in there.
        /// </summary>
        /// <param name="seedStack"></param>
        /// <returns>the object containing the seed drop slot that was modified. Null if no slot is open</returns>
        public GameObject CreateSeedStack(SeedBucketUI seedStack)
        {
            var dropSlot = GetFirstEmptyDropSlot();
            if (dropSlot == null)
            {
                return null;
            }
            dropSlot.RecieveNewSeeds(seedStack);
            return dropSlot.gameObject;
        }

        public SeedInventoryDropSlot GetFirstEmptyDropSlot()
        {
            foreach (Transform bucketInstance in seedGridLayoutParent.transform)
            {
                var dropSlot = bucketInstance.GetComponent<SeedInventoryDropSlot>();
                if (dropSlot.CanSlotRecieveNewStack())
                {
                    return dropSlot;
                }
            }
            if (trashSlot?.CanSlotRecieveNewStack() ?? false)
            {
                return trashSlot;
            }
            return null;
        }

        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}