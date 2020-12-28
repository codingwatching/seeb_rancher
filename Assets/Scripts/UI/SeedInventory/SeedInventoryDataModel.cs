using System;

namespace Assets.Scripts.UI.SeedInventory
{
    [Serializable]
    public class SeedInventoryDataModel
    {
        public SeedBucket[] seedBuckets;
        public SeedInventoryDataModel(int seedBucketNum)
        {
            seedBuckets = new SeedBucket[seedBucketNum];
            for (int i = 0; i < seedBucketNum; i++)
            {
                seedBuckets[i] = new SeedBucket();
            }
        }
    }
}
