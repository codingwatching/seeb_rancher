using System;

namespace Assets.Scripts.UI.SeedInventory
{
    [Serializable]
    public class SeedInventoryDataModel
    {
        public SeedBucketUI[] seedBuckets;
        public SeedInventoryDataModel(int seedBucketNum)
        {
            seedBuckets = new SeedBucketUI[seedBucketNum];
            for (int i = 0; i < seedBucketNum; i++)
            {
                seedBuckets[i] = new SeedBucketUI();
            }
        }
    }
}
