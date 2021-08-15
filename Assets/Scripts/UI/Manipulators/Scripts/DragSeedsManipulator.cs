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
    public class DragSeedsManipulator : MapManipulator, ISeedHoldingManipulator
    {
        public bool IsActive { get; private set; }

        public GameObjectVariable seedSproutGameObject;

        [SerializeField] public RaycastGroup plantableCaster;
        [SerializeField] private Sprite plantCursor;
        [SerializeField] private float plantableSquareSideSize;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedInventoryDropSlot sourceSlot = null;

        [SerializeField] private GameObjectVariable selectedGameObject;

        private ManipulatorController controller;

        public string SeedGroupName => sourceSlot?.dataModel.description;

        public bool AttemptTransferAllSeedsInto(SeedBucket target)
        {
            if (Object.ReferenceEquals(target, sourceSlot.dataModel.bucket))
            {
                //if attempting to transfer into the bucket which is already the source for this manipulator, shit it all down.
                if (IsActive)
                {
                    // if we're active, become inactive
                    controller.CloseManipulator();
                }
                return false;
            }
            if (target.TryTransferSeedsIntoSelf(sourceSlot.dataModel.bucket))
            {
                OnSeedsUpdated();
                return true;
            }
            return false;
        }
        public SeedBucketUI SwapSeedsWithBucket(SeedBucketUI target)
        {
            var oldui = sourceSlot?.dataModel;
            sourceSlot.UpdateDataModel(target);
            OnSeedsUpdated();
            if (IsActive)
            {
                // if we're active, become inactive
                controller.CloseManipulator();
            }
            return oldui;
        }
        public int PlantIdOfSeebs()
        {
            return sourceSlot?.dataModel.bucket.PlantTypeId ?? -1;
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
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
            GameObject.Destroy(draggingSeedsInstance.gameObject);
            draggingSeedsInstance = null;
            sourceSlot = null;
            IsActive = false;
            seedSproutGameObject.CurrentValue.SetActive(false);
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

            var hitPoint2D = hoveredSpot.HasValue ? new Vector2(hoveredSpot.Value.point.x, hoveredSpot.Value.point.z) : Vector2.zero;
            var canPlantHere = dirtPlanter != null &&
                Mathf.Abs(hitPoint2D.x) <= plantableSquareSideSize / 2 &&
                Mathf.Abs(hitPoint2D.y) <= plantableSquareSideSize / 2;

            if (!canPlantHere)
            {
                seedSproutGameObject.CurrentValue.SetActive(false);
                return true;
            }

            
            if (!isDragging)
            {
                // if draging is happening, don't update preview here.
                seedSproutGameObject.CurrentValue.transform.position = hoveredSpot.Value.point;
                seedSproutGameObject.CurrentValue.SetActive(true);
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
                    var couldPlantSeed = TryPlantSeed(mouseDownRaycastHit.Value);

                    OnSeedsUpdated();

                    if (!couldPlantSeed)
                    {
                        return false;
                    }
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
        public void OnAreaSelected(Vector2 origin, Vector2 size)
        {
            Debug.Log("plant inside range:");
            Debug.Log(origin);
            Debug.Log(size);

            // TODO:

            //range.rectangleDataView.ToBox(5, out var center, out var size);
            //var extent = size / 2;
            //Debug.Log(center);
            //Debug.Log(extent);

            //var plantDensity = 1f;

            //var totalPlants = size.x * size.z * plantDensity;

            //for (int i = 0; i < totalPlants; i++)
            //{
            //    var plantPoint = 
            //        new Vector3(
            //            Random.Range(-extent.x, extent.x),
            //            0f,
            //            Random.Range(-extent.z, extent.z)) +
            //        center +
            //        Vector3.up * 10;
            //    var ray = new Ray(plantPoint, Vector3.down);
            //    if(Physics.Raycast(ray, out var hit, 100f, plantableCaster.layersToRaycastTo))
            //    {
            //        if (!TryPlantSeed(hit))
            //        {
            //            break;
            //        }
            //    }
            //}
            //OnSeedsUpdated();
        }
        public void OnDragAreaChanged(Vector2 origin, Vector2 size)
        {
            // noop
        }
        public void SetDragging(bool isDragging)
        {
            this.isDragging = isDragging;
        }
    }
}
