using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.UI.SeedInventory;
using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public class PlantContainer : MonoBehaviour, ISpawnable, IManipulatorClickReciever
    {
        public PlantType plantType;
        public PlantTypeRegistry plantTypes;

        [SerializeField]
        [HideInInspector]
        private float growth;
        public float Growth => growth;

        public IntReference levelPhase;
        public GameObjectVariable draggingSeedSet;

        public GameObject planter;
        public GameObject plantsParent;

        private Collider harvestCollider => plantsParent.GetComponent<Collider>();
        private Collider planterCollider => planter.GetComponent<Collider>();
        private void Start()
        {
            levelPhase.ValueChanges
                .TakeUntilDisable(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    AdvanceGrowPhase(pair.Current - pair.Previous);
                }).AddTo(this);
            draggingSeedSet.Value
                .TakeUntilDisable(this)
                .Subscribe(dragger =>
                {
                    SetPlanterColliderEnabled();
                }).AddTo(this);
        }

        private void SetPlanterColliderEnabled()
        {
            planterCollider.enabled = plantType == null && draggingSeedSet.CurrentValue != null;
        }

        private void AdvanceGrowPhase(int phaseDiff)
        {
            if (plantType == null)
                return;
            UpdateGrowth(plantType.AddGrowth(phaseDiff, growth));
        }

        public void OnPlanterClicked()
        {
            var draggingSeeds = draggingSeedSet.CurrentValue.GetComponent<DraggingSeeds>();
            var nextSeed = draggingSeeds.myBucket.TakeOne();
            draggingSeeds.SeedBucketUpdated();

            plantType = plantTypes.GetUniqueObjectFromID(nextSeed.plantType);
            UpdateGrowth(0, true);
            SetPlanterColliderEnabled();
            Debug.Log("plant something");
        }

        public void SetupAfterSpawn()
        {
            UpdateGrowth(Mathf.Clamp(Random.Range(0, 1.2f), 0, 1));
        }

        public void UpdateGrowth(float newGrowth, bool forcePrefabInstantiate = false)
        {
            if (plantType == null)
            {
                return;
            }
            var lastPrefab = plantType.GetPrefabForGrowth(growth);
            var newPrefab = plantType.GetPrefabForGrowth(newGrowth);
            if (lastPrefab != newPrefab || forcePrefabInstantiate)
            {
                plantsParent.DestroyAllChildren();
                Instantiate(newPrefab, plantsParent.transform);
            }
            growth = newGrowth;

            harvestCollider.enabled = growth >= 1 - 1e-5;
        }

        public void SelfHit(RaycastHit hit)
        {
            if (hit.collider == harvestCollider)
            {
                TryHarvest();
            }
            else if (hit.collider == planterCollider)
            {
                OnPlanterClicked();
            }
        }

        private void TryHarvest()
        {
            if (growth < 1 - 1e-5)
            {
                Debug.LogError("Non-grown plant clicked");
                return;
            }
            HarvestPlant();
        }

        private void HarvestPlant()
        {
            var draggingProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
            var dragger = draggingProvider.SpawnNewDraggingSeedsOrGetCurrent();
            if (!dragger.myBucket.TryAddSeedsToSet(plantType.HarvestSeeds()))
            {
                return;
            }
            dragger.SeedBucketUpdated();

            growth = 0;
            plantType = null;
            harvestCollider.enabled = false;
            SetPlanterColliderEnabled();
            plantsParent.DestroyAllChildren();
        }
    }
}
