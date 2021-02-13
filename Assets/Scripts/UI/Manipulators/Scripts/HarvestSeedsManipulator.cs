using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "HarvestSeedsManipulator", menuName = "Tiling/Manipulators/HarvestSeedsManipulator", order = 2)]
    public class HarvestSeedsManipulator : MapManipulator, ISeedHoldingManipulator
    {
        private ManipulatorController controller;

        public GameObjectVariable selectedGameObject;
        public LayerMask harvestLayers;

        public Sprite harvestCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedBucket seeds = null;


        public void AttemptTransferAllSeedsInto(SeedBucket target)
        {
            target.TryTransferSeedsIntoSelf(seeds);
            OnSeedsUpdated();
        }
        public Seed[] AttemptTakeSeeds(int seedCount)
        {
            return seeds?.TakeN(seedCount);
        }

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            selectedGameObject.SetValue(null);
            Debug.Log("harvest manipulator opened");
            CursorTracker.SetCursor(harvestCursor);
            if (seeds != default)
            {
                Debug.LogError("Overwriting existing seed bucket!");
            }
            seeds = new SeedBucket();
        }

        public override void OnClose()
        {
            GameObject.Destroy(draggingSeedsInstance.gameObject);
            draggingSeedsInstance = null;

            CursorTracker.ClearCursor();
        }

        public override bool OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return true;
            }
            if (!MouseOverHelpers.RaycastToObject(harvestLayers, out var singleHit))
            {
                // if hit the UI or nothing, do nothing
                return true;
            }
            var planter = singleHit.collider.gameObject?.GetComponentInParent<PlantContainer>();
            var harvested = planter.TryHarvest();
            if (harvested.Length <= 0)
            {
                return true;
            }
            if (draggingSeedsInstance == null)
            {
                var draggingParentProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
                draggingSeedsInstance = draggingParentProvider.SpawnNewDraggingSeeds();
            }
            if (!seeds.TryAddSeedsToSet(harvested))
            {
                // TODO: what happens to the seeds if they can't be added? 
                //  we should probably not harvest the plant if the seeds will just dissapear into the void
                Debug.LogError("Incompatible seeds, they have been lost!");
                return true;
            }
            OnSeedsUpdated();
            return true;
        }

        private void OnSeedsUpdated()
        {
            if (seeds.Empty)
            {
                // close the manipulator if the bucket is empty
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            draggingSeedsInstance.DisplaySeedBucket(seeds);
        }
    }
}
