using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "DragSeedsManipulator", menuName = "Tiling/Manipulators/DragSeedsManipulator", order = 3)]
    public class DragSeedsManipulator : MapManipulator, ISeedHoldingManipulator
    {
        public bool IsActive { get; private set; }

        [SerializeField] public RaycastGroup harvestCaster;
        [SerializeField] private Sprite plantCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedBucket seeds = null;

        [SerializeField] private GameObjectVariable selectedGameObject;

        private ManipulatorController controller;
        private MovingSingleOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public void AttemptTransferAllSeedsInto(SeedBucket target)
        {
            target.TryTransferSeedsIntoSelf(seeds);
            OnSeedsUpdated();
        }
        public Seed[] AttemptTakeSeeds(int seedCount)
        {
            return seeds?.TakeN(seedCount);
        }

        /// <summary>
        /// Take all seeds from <paramref name="sourceBucket"/> and place them into a new bucket for this manipulator
        /// </summary>
        /// <param name="sourceBucket"></param>
        public void InitializeSeedBucketFrom(SeedBucket sourceBucket)
        {
            if (seeds != default)
            {
                Debug.LogError("Overwriting existing seed bucket!");
            }
            seeds = new SeedBucket();
            seeds.TryTransferSeedsIntoSelf(sourceBucket);
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
            if (!seeds.Empty)
            {
                //TODO: handle the case where seeds is not empty here. Remember where the seeds came from in the inventory
                Debug.LogError("Seeds lost!!");
            }
            seeds = null;
            IsActive = false;
            singleOutlineHelper.ClearOutlinedObject();
        }

        public override bool OnUpdate()
        {
            var planter = GetHoveredPlantContainer();
            // must be able to plant seed, in a planter
            var planterValid = planter?.CanPlantSeed ?? false;

            singleOutlineHelper.UpdateOutlineObject(planterValid ? planter.GetOutlineObject() : null);

            if (!planterValid || !Input.GetMouseButtonDown(0))
            {
                return true;
            }

            var nextSeed = seeds.TakeOne();
            if (nextSeed == null)
            {
                // close the manipulator if we can't get any more seeds
                return false;
            }
            planter.PlantSeed(nextSeed);

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
