using Assets.Scripts.DataModels;
using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.UI.SeedInventory;
using Assets.Scripts.Utilities.Core;
using Assets.Scripts.Utilities.SaveSystem.Components;
using Genetics.GeneticDrivers;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Plants
{

    public class PlantContainer : MonoBehaviour,
        ISpawnable,
        IManipulatorClickReciever,
        ISaveableData
    {
        public PlantType plantType;
        public PlantTypeRegistry plantTypes;

        [SerializeField]
        [HideInInspector]
        private float growth;
        public float Growth => growth;
        public Seed sourceSeed;
        private CompiledGeneticDrivers _drivers;
        private CompiledGeneticDrivers GeneticDrivers
        {
            get
            {
                if (plantType == null || sourceSeed == default)
                {
                    return null;
                }
                if (_drivers == null)
                {
                    _drivers = plantType.genome.CompileGenome(sourceSeed.genes);
                }
                return _drivers;
            }
            set
            {
                if (value != null)
                {
                    throw new System.Exception("can only clear");
                }
                _drivers = null;
            }
        }


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
            PlantSeed(nextSeed);
        }

        private void PlantSeed(Seed toBePlanted)
        {
            sourceSeed = toBePlanted;
            plantType = plantTypes.GetUniqueObjectFromID(sourceSeed.plantType);
            UpdateGrowth(0, true);
            SetPlanterColliderEnabled();
        }

        public void SetupAfterSpawn()
        {
            sourceSeed = new Seed
            {
                genes = plantType.genome.GenerateBaseGenomeData(),
                plantType = plantType.plantID
            };
            UpdateGrowth(Mathf.Clamp(Random.Range(0, 1.2f), 0, 1));
        }

        public void UpdateGrowth(float newGrowth, bool forcePrefabInstantiate = false)
        {
            if (plantType == null)
            {
                if (forcePrefabInstantiate)
                {
                    growth = 0;
                    UpdatePlant();
                }
                return;
            }
            var oldGrowth = growth;
            growth = newGrowth;
            if (forcePrefabInstantiate ||
                plantType.GetPrefabForGrowth(oldGrowth) != plantType.GetPrefabForGrowth(growth))
            {
                UpdatePlant();
            }

            harvestCollider.enabled = growth >= 1 - 1e-5;
        }

        public void UpdatePlant()
        {
            plantsParent.DestroyAllChildren();
            if (plantType == null)
            {
                return;
            }
            var newPrefab = plantType.GetPrefabForGrowth(growth);
            var newPlant = Instantiate(newPrefab, plantsParent.transform);
            plantType.ApplyGeneticModifiers(newPlant, GeneticDrivers);
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
            if (!dragger.myBucket.TryAddSeedsToSet(plantType.HarvestSeeds(sourceSeed)))
            {
                return;
            }
            dragger.SeedBucketUpdated();

            growth = 0;
            plantType = null;
            sourceSeed = default;
            GeneticDrivers = null;
            harvestCollider.enabled = false;
            SetPlanterColliderEnabled();
            UpdatePlant();
        }

        #region Saveable
        [System.Serializable]
        class PlantSaveObject
        {
            int plantTypeId;
            float growth;
            Seed sourceSeed;
            public PlantSaveObject(PlantContainer source)
            {
                plantTypeId = source.plantType?.plantID ?? -1;
                growth = source.growth;
                sourceSeed = source.sourceSeed;
            }

            public void Apply(PlantContainer target)
            {
                target.plantType = plantTypeId == -1 ? null : target.plantTypes.GetUniqueObjectFromID(plantTypeId);
                target.sourceSeed = sourceSeed;
                target.UpdateGrowth(growth, true);
            }
        }

        public string UniqueSaveIdentifier => "PlantContainer";
        public object GetSaveObject()
        {
            return new PlantSaveObject(this);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is PlantSaveObject saveObj)
            {
                saveObj.Apply(this);
            }
        }

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }
        #endregion
    }
}
