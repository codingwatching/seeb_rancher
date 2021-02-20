using Assets.Scripts.DataModels;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour
    {
        public GameObject seedGridLayoutParent;
        
        public static SeedInventoryController Instance;


        /// <summary>
        /// finds the first open drop slot, and puts the <paramref name="seedStack"/> in there.
        /// </summary>
        /// <param name="seedStack"></param>
        /// <returns>the object containing the seed drop slot that was modified. Null if no slot is open</returns>
        public GameObject CreateSeedStack(SeedBucketUI seedStack)
        {
            var dropSlot = this.GetFirstEmptyDropSlot();
            if(dropSlot == null)
            {
                return null;
            }
            dropSlot.UpdateDataModel(seedStack);
            return dropSlot.gameObject;
        }

        public SeedInventoryDropSlot GetFirstEmptyDropSlot()
        {
            foreach (Transform bucketInstance in seedGridLayoutParent.transform)
            {
                var dropSlot = bucketInstance.GetComponent<SeedInventoryDropSlot>();
                var seedModel = dropSlot.dataModel;
                if (string.IsNullOrWhiteSpace(seedModel.description) && seedModel.bucket.Empty)
                {
                    return dropSlot;
                }
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