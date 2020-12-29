using Assets.Scripts.Utilities.SaveSystem.Components;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour, ISaveableData
    {
        public int defaultSeedBuckets;

        public GameObject seedGridLayoutParent;
        public SeedInventoryDropSlot seedSlotUIElementPrefab;

        private SeedInventoryDataModel dataModel;

        private void Awake()
        {
            if (dataModel == null)
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

        #region save data
        public string UniqueSaveIdentifier => "SeedInventory";

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return dataModel;
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SeedInventoryDataModel data)
            {
                dataModel = data;
                RenderDataModel();
            }
        }
        #endregion
    }
}