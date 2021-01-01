using Assets.Scripts.DataModels;
using Assets.Scripts.Utilities.Core;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    // TODO: some way to return the dragging seeds to their source inventory slot
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class DraggingSeeds : MonoBehaviour
    {
        public GameObjectVariable currentDraggingSeeds;
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
            if (myBucket.Empty)
            {
                currentDraggingSeeds.SetValue(null);
                Destroy(gameObject);
            }else
            {
                GetComponent<SeedBucketDisplay>().DisplaySeedBucket(myBucket);
            }
        }
    }
}
