using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.Tiling;
using Dman.Utilities;
using System.Linq;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "HarvestSeedsManipulator", menuName = "Tiling/Manipulators/HarvestSeedsManipulator", order = 2)]
    public class HarvestSeedsManipulator : MapManipulator, ISeedHoldingManipulator, IAreaSelectManipulator
    {

        public GameObjectVariable selectedGameObject;
        public RaycastGroup harvestCaster;

        public Sprite harvestCursor;

        public GameObject dragAreaRenderer;

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
            var resultSeeds = seeds?.TakeN(seedCount);
            if (resultSeeds != null)
            {
                OnSeedsUpdated();
            }
            return resultSeeds;
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
            selectedGameObject.SetValue(null);

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

        private Vector3? mouseDownPosition = null;

        public override bool OnUpdate()
        {
            var planter = GetHoveredPlantContainer();
            var validTarget = planter?.CanHarvest() ?? false;
            singleOutlineHelper.UpdateOutlineObject(validTarget ? planter.GetOutlineObject() : null);


            if (!validTarget)
            {
                return true;
            }
            if (validTarget)
            {
                selectedGameObject.SetValue(planter.gameObject);
            }

            if (Input.GetMouseButtonDown(0))
            {
                mouseDownPosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0) && mouseDownPosition.HasValue)
            {
                var mouseDistance = (Input.mousePosition - mouseDownPosition).Value.magnitude;
                mouseDownPosition = null;
                if (mouseDistance < DragAreaSelector.MouseMoveDragThreshold)
                {
                    if (TryHarvestPlant(planter)) OnSeedsUpdated();
                }
            }

            return true;
        }

        private bool TryHarvestPlant(PlantContainer planter)
        {
            var harvested = planter.TryHarvest();
            if(harvested == null)
            {
                return false;
            }
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
                return false;
            }
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
            if (seeds == null || seeds.Empty)
            {
                // close the manipulator if the bucket is empty
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            draggingSeedsInstance.DisplaySeedBucket(seeds);
        }

        public void OnAreaSelected(UniversalCoordinateRange range)
        {
            Debug.Log("Harvest inside range:");
            Debug.Log(range);

            range.rectangleDataView.ToBox(5, out var center, out var size);
            var extent = size / 2;
            Debug.Log(center);
            Debug.Log(extent);
            var allTargetPlanters = Physics.OverlapBox(center, extent);
            var harvestablePlanters = allTargetPlanters
                .Select(x => x.gameObject.GetComponentInParent<PlantContainer>())
                .Where(x => x?.CanHarvest() ?? false)
                .ToList();
            foreach (var planter in harvestablePlanters)
            {
                if (!TryHarvestPlant(planter))
                {
                    return;
                }
            }
            OnSeedsUpdated();
        }
    }
}
