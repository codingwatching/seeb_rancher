using System;

namespace Assets.Scripts.UI.SeedInventory
{
    [Serializable]
    public class SeedInventoryDataModel
    {
        public SeedBucketSlot[] seedBuckets;
        public SeedInventoryDataModel(int seedBucketNum)
        {
            seedBuckets = new SeedBucketSlot[seedBucketNum];
            for (int i = 0; i < seedBucketNum; i++)
            {
                seedBuckets[i] = new SeedBucketSlot();
            }
        }
    }
}
