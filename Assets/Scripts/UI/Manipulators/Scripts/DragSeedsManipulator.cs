using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "DragSeedsManipulator", menuName = "Tiling/Manipulators/DragSeedsManipulator", order = 3)]
    public class DragSeedsManipulator : MapManipulator, ISeedHoldingManipulator
    {
        public bool IsActive { get; private set; }

        [SerializeField] private LayerMask layersToHit;
        [SerializeField] private Sprite plantCursor;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedBucket seeds = null;

        [SerializeField] private GameObjectVariable selectedGameObject;

        private ManipulatorController controller;

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
        }

        public override bool OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return true;
            }
            if (!MouseOverHelpers.RaycastToObject(layersToHit, out var singleHit))
            {
                // if hit the UI or nothing, do nothing
                return true;
            }
            var planter = singleHit.collider.gameObject?.GetComponentInParent<PlantContainer>();
            if (planter == null || !planter.CanPlantSeed)
            {
                // must be able to plant seed, in a planter
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
