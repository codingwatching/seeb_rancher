using Assets.Scripts.Plants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class DraggingSeeds : MonoBehaviour
    {
        public SeedBucket myBucket;

        private void Awake()
        {
            myBucket = new SeedBucket();
        }

        private void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;
        }
        public bool TryAddSeedsToSet(Seed[] seeds)
        {
            if (!myBucket.TryAddSeedsToSet(seeds))
            {
                return false;
            }
            GetComponent<SeedBucketDisplay>().DisplaySeedBucket(myBucket);
            return true;
        }
    }
}
