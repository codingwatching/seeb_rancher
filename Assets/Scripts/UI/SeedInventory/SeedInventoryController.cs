using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour
    {

        public int defaultSeedBuckets;

        public GameObject seedGridLayoutParent;
        public SeedInventoryDropSlot seedSlotUIElementPrefab;

        private SeedInventoryDataModel dataModel;
        private void Awake()
        {
            //TODO: load from save. currently generating on load
            dataModel = new SeedInventoryDataModel(defaultSeedBuckets);
            RenderDataModel();
        }

        private void RenderDataModel()
        {
            seedGridLayoutParent.DestroyAllChildren();
            foreach (var bucket in dataModel.seedBuckets)
            {
                var newBucket = Instantiate(seedSlotUIElementPrefab, seedGridLayoutParent.transform);
                newBucket.SetDataModelLink(bucket);
            }
        }
    }
}