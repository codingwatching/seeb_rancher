using Dman.SceneSaveSystem;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour, ISaveableData
    {
        public int defaultSeedBuckets;

        public GameObject seedGridLayoutParent;
        public SeedInventoryDropSlot seedSlotUIElementPrefab;

        public static SeedInventoryController Instance;

        [HideInInspector]
        public SeedInventoryDataModel dataModel { get; private set; }

        private void Awake()
        {
            if (dataModel == null)
                SetupDefaultDataModel();
            Instance = this;
            RenderDataModel();
        }

        private void SetupDefaultDataModel()
        {
            dataModel = new SeedInventoryDataModel(defaultSeedBuckets);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void DataModelUpdated()
        {
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
            }
            else
            {
                SetupDefaultDataModel();
            }
            RenderDataModel();
        }
        #endregion
    }
}