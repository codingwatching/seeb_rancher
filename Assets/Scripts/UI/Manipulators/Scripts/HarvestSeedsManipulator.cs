using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "HarvestSeedsManipulator", menuName = "Tiling/Manipulators/HarvestSeedsManipulator", order = 2)]
    public class HarvestSeedsManipulator : MapManipulator, ISeedHoldingManipulator
    {

        public GameObjectVariable selectedGameObject;
        public RaycastGroup harvestCaster;

        public Sprite harvestCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedBucket seeds = null;

        private ManipulatorController controller;
        private MovingSingleOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public string SeedGroupName => "harvested";

        public bool AttemptTransferAllSeedsInto(SeedBucket target)
        {
            if (target.TryTransferSeedsIntoSelf(seeds))
            {
                OnSeedsUpdated();
                return true;
            }
            return false;
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
            seeds = new SeedBucket();
            singleOutlineHelper = new MovingSingleOutlineHelper(outlineCollection);
        }

        public override void OnClose()
        {
            if (draggingSeedsInstance != null)
            {
                GameObject.Destroy(draggingSeedsInstance.gameObject);
            }
            draggingSeedsInstance = null;

            CursorTracker.ClearCursor();
            singleOutlineHelper.ClearOutlinedObject();

            if (!seeds.Empty)
            {
                var seedReceiver = SeedInventoryController.Instance.CreateSeedStack(new SeedBucketUI
                {
                    bucket = seeds,
                    description = "Harvested"
                });
                if (seedReceiver == null)
                {
                    Debug.LogError($"No open seed slots, {seeds.SeedCount} seeds lost");
                }
            }
            seeds = null;
        }

        public override bool OnUpdate()
        {
            var planter = GetHoveredPlantContainer();
            var validTarget = planter?.CanHarvest() ?? false;

            singleOutlineHelper.UpdateOutlineObject(validTarget ? planter.GetOutlineObject() : null);

            if (!validTarget || !Input.GetMouseButtonDown(0))
            {
                return true;
            }
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
        private PlantContainer GetHoveredPlantContainer()
        {
            var mouseOvered = harvestCaster.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            return hoveredGameObject?.GetComponentInParent<PlantContainer>();
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
