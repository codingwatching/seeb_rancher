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

    /// <summary>
    /// used to control seeds being moved by the cursor, when not harvesting.
    /// </summary>
    [CreateAssetMenu(fileName = "DragSeedsManipulator", menuName = "Tiling/Manipulators/DragSeedsManipulator", order = 3)]
    public class DragSeedsManipulator : MapManipulator, ISeedHoldingManipulator, IAreaSelectManipulator
    {
        public bool IsActive { get; private set; }

        [SerializeField] public RaycastGroup harvestCaster;
        [SerializeField] private Sprite plantCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedInventoryDropSlot sourceSlot = null;

        [SerializeField] private GameObjectVariable selectedGameObject;

        private ManipulatorController controller;
        private MovingSingleOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public string SeedGroupName => sourceSlot?.dataModel.description;

        public bool AttemptTransferAllSeedsInto(SeedBucket target)
        {
            if (Object.ReferenceEquals(target, sourceSlot.dataModel.bucket))
            {
                //if attempting to transfer into the bucket which is already the source for this manipulator, shit it all down.
                return false;
            }
            if (target.TryTransferSeedsIntoSelf(sourceSlot.dataModel.bucket))
            {
                OnSeedsUpdated();
                return true;
            }
            return false;
        }
        public Seed[] AttemptTakeSeeds(int seedCount)
        {
            var seedResult = sourceSlot?.dataModel.bucket.TakeN(seedCount);
            if(seedResult != null)
            {
                this.OnSeedsUpdated();
            }
            return seedResult;
        }

        /// <summary>
        /// Take all seeds from <paramref name="sourceBucket"/> and place them into a new bucket for this manipulator
        /// </summary>
        /// <param name="sourceBucket"></param>
        public void InitializeSeedBucketFrom(SeedInventoryDropSlot sourceSlot)
        {
            if (sourceSlot.dataModel.bucket.Empty)
            {
                Debug.LogError("cannot initialize source slot to an empty slot. Closing drag seeds manipulator immediately.");
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            this.sourceSlot = sourceSlot;
            OnSeedsUpdated();
        }


        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            selectedGameObject.SetValue(null);
            Debug.Log("seed cursor manipulator opened");
            CursorTracker.SetCursor(plantCursor);

            var draggingParentProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
            draggingSeedsInstance = draggingParentProvider.SpawnNewDraggingSeeds();

            IsActive = true;
            singleOutlineHelper = new MovingSingleOutlineHelper(outlineCollection);
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
            GameObject.Destroy(draggingSeedsInstance.gameObject);
            draggingSeedsInstance = null;
            sourceSlot = null;
            IsActive = false;
            singleOutlineHelper.ClearOutlinedObject();
        }

        private Vector3? mouseDownPosition = null;
        public override bool OnUpdate()
        {
            if (sourceSlot?.dataModel.bucket.Empty ?? true)
            {
                // clear the description out. the description moves with the seeds, not owned by the slot.
                sourceSlot.dataModel.description = "";
                sourceSlot.MySeedsUpdated();
                // close the manipulator if the bucket is empty
                return false;
            }

            var planter = GetHoveredPlantContainer();
            // must be able to plant seed, in a planter
            var planterValid = planter?.CanPlantSeed ?? false;

            singleOutlineHelper.UpdateOutlineObject(planterValid ? planter.GetOutlineObject() : null);

            if (!planterValid)
            {
                return true;
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
                    if (!TryPlantSeed(planter))
                    {
                        return false;
                    }
                    OnSeedsUpdated();
                }
            }
            return true;
        }

        private bool TryPlantSeed(PlantContainer planter)
        {
            var nextSeed = sourceSlot.dataModel.bucket.TakeOne();
            if (nextSeed == null)
            {
                // close the manipulator if we can't get any more seeds
                return false;
            }
            planter.PlantSeed(nextSeed);
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
            draggingSeedsInstance.DisplaySeedBucket(sourceSlot.dataModel.bucket);
            sourceSlot.MySeedsUpdated();
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
            var plantableSeeds = allTargetPlanters
                .Select(x => x.gameObject.GetComponentInParent<PlantContainer>())
                .Where(x => x?.CanPlantSeed ?? false)
                .ToList();
            foreach (var planter in plantableSeeds)
            {
                if (!TryPlantSeed(planter))
                {
                    return;
                }
            }
            OnSeedsUpdated();
        }
    }
}
