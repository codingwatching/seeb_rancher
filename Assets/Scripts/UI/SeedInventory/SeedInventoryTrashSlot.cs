using Assets.Scripts.DataModels;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class SeedInventoryTrashSlot : SeedInventoryDropSlot
    {
        public override bool CanSlotRecieveNewStack()
        {
            return true;
        }

        protected override void AddSeedsFromManipulator(ISeedHoldingManipulator seedHolder)
        {
            if (!dataModel.bucket.Empty)
            {
                DoTrashEffect();
                dataModel = new SeedBucketUI();
            }
            base.AddSeedsFromManipulator(seedHolder);
        }

        public override bool RecieveNewSeeds(SeedBucketUI model)
        {
            if (!dataModel.bucket.Empty)
            {
                DoTrashEffect();
            }
            this.UpdateDataModel(model);
            return true;
        }

        private void DoTrashEffect()
        {
            var trashAnim = GetComponent<Animator>();
            trashAnim.SetTrigger("trashed");
        }

    }
}