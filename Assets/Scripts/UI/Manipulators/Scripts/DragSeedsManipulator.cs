using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ObjectSets;
using Dman.ReactiveVariables;
using Dman.Tiling;
using Dman.Utilities;
using System.Collections.Generic;
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

        [SerializeField] public RaycastGroup plantableCaster;
        [SerializeField] private Sprite plantCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedInventoryDropSlot sourceSlot = null;

        [SerializeField] private GameObjectVariable selectedGameObject;

        private ManipulatorController controller;
        private MovingOutlineHelper singleOutlineHelper;
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
            if (seedResult != null)
            {
                OnSeedsUpdated();
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
            singleOutlineHelper = new MovingOutlineHelper(outlineCollection);
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
        private RaycastHit? mouseDownRaycastHit = null;
        private bool isDragging;
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

            var hoveredSpot = plantableCaster.CurrentlyHitObject;
            var dirtPlanter = hoveredSpot.HasValue ? hoveredSpot.Value.collider.GetComponent<PlantableDirt>() : null;

            var canPlantHere = dirtPlanter != null;

            if (!canPlantHere)
            {
                singleOutlineHelper.UpdateOutlineObject(null);
                return true;
            }

            if (!isDragging)
            {
                // if draging is happening, don't update preview here.
                singleOutlineHelper.UpdateOutlineObject(dirtPlanter.GetOutlineObject());
            }


            if (Input.GetMouseButtonDown(0))
            {
                mouseDownPosition = Input.mousePosition;
                mouseDownRaycastHit = hoveredSpot;
            }

            if (Input.GetMouseButtonUp(0) && mouseDownPosition.HasValue)
            {
                var mouseDistance = (Input.mousePosition - mouseDownPosition).Value.magnitude;
                mouseDownPosition = null;
                if (mouseDistance < DragAreaSelector.MouseMoveDragThreshold)
                {
                    if (!TryPlantSeed(mouseDownRaycastHit.Value))
                    {
                        return false;
                    }
                    OnSeedsUpdated();
                }
            }
            return true;
        }

        /// <summary>
        /// return true if there are any more seebs to plant. false if the last seeb was planted,
        ///     or if there were no seebs to plant at all
        /// </summary>
        /// <param name="plantLocation"></param>
        /// <returns></returns>
        private bool TryPlantSeed(RaycastHit plantLocation)
        {
            var nextSeed = sourceSlot.dataModel.bucket.TakeOne();
            if (nextSeed == null)
            {
                // close the manipulator if we can't get any more seeds
                return false;
            }

            var plantTypeRegistry = RegistryRegistry.GetObjectRegistry<BasePlantType>();
            var seedType = plantTypeRegistry.GetUniqueObjectFromID(nextSeed.plantType);

            var newPlant = seedType.SpawnNewPlant(plantLocation.point, nextSeed, true);

            return !sourceSlot.dataModel.bucket.Empty;
        }

        private void OnSeedsUpdated()
        {
            draggingSeedsInstance.DisplaySeedBucket(sourceSlot.dataModel.bucket);
            sourceSlot.MySeedsUpdated();
        }
        public void OnAreaSelected(UniversalCoordinateRange range)
        {
            Debug.Log("plant inside range:");
            Debug.Log(range);

            range.rectangleDataView.ToBox(5, out var center, out var size);
            var extent = size / 2;
            Debug.Log(center);
            Debug.Log(extent);

            var plantDensity = 1f;

            var totalPlants = size.x * size.z * plantDensity;

            for (int i = 0; i < totalPlants; i++)
            {
                var plantPoint = 
                    new Vector3(
                        Random.Range(-extent.x, extent.x),
                        0f,
                        Random.Range(-extent.z, extent.z)) +
                    center +
                    Vector3.up * 10;
                var ray = new Ray(plantPoint, Vector3.down);
                if(Physics.Raycast(ray, out var hit, 100f, plantableCaster.layersToRaycastTo))
                {
                    if (!TryPlantSeed(hit))
                    {
                        break;
                    }
                }
            }
            OnSeedsUpdated();
        }
        public void OnDragAreaChanged(UniversalCoordinateRange range)
        {
            // noop
        }
        public void SetDragging(bool isDragging)
        {
            this.isDragging = isDragging;
        }
    }
}
