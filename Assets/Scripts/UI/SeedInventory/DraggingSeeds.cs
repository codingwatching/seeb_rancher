using Assets.Scripts.DataModels;
using UnityEngine;

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
        public void SeedBucketUpdated()
        {
            GetComponent<SeedBucketDisplay>().DisplaySeedBucket(myBucket);
        }
    }
}
