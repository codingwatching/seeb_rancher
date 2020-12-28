using UnityEngine;
using System.Collections;
using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class SeedInventoryDropSlot : MonoBehaviour
    {
        public GameObjectVariable draggingSeedSet;

        public Button DropSlotButton;

        private SeedBucket dataModel;

        private SeedBucketDisplay Displayer => GetComponent<SeedBucketDisplay>();


        // Use this for initialization
        void Start()
        {
            draggingSeedSet.Value
                .TakeUntilDestroy(this)
                .Subscribe(newDraggingThing =>
                {
                    DropSlotButton.enabled = newDraggingThing != null;
                }).AddTo(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TryAddHoveringSeedToSelf()
        {
            var dragginSeeds = draggingSeedSet.CurrentValue?.GetComponent<DraggingSeeds>();
            if (dragginSeeds == null)
                return;
            dataModel.TryCombineSeedsInto(dragginSeeds.myBucket);
            Displayer.DisplaySeedBucket(dataModel);
            if (dragginSeeds.myBucket.Empty)
            {
                Destroy(dragginSeeds.gameObject);
                draggingSeedSet.SetValue(null);
            }
        }

        public void SetDataModelLink(SeedBucket dataModel)
        {
            this.dataModel = dataModel;
            Displayer.DisplaySeedBucket(dataModel);
        }
    }
}