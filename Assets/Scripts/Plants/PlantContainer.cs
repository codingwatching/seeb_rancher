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
        public GameObjectVariable selectedPlant;

        [SerializeField]
        [HideInInspector]
        private float growth;
        public float Growth => growth;
        //public Seed plantGenes;
        private CompiledGeneticDrivers _drivers;
        private CompiledGeneticDrivers GeneticDrivers
        {
            get
            {
                if (plantType == null || polliationState == default)
                {
                    return null;
                }
                if (_drivers == null)
                {
                    _drivers = plantType.genome.CompileGenome(polliationState.SelfGenes.genes);
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
        public PollinationState polliationState;

        public IntReference levelPhase;
        public GameObjectVariable draggingSeedSet;

        public GameObject planter;
        public GameObject plantsParent;

        private Collider plantCollider => plantsParent.GetComponent<Collider>();
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

        public bool OnPlanterClicked()
        {
            var draggingSeeds = draggingSeedSet.CurrentValue?.GetComponent<DraggingSeeds>();
            var nextSeed = draggingSeeds?.myBucket.TakeOne();
            if (nextSeed == null)
            {
                return false;
            }
            draggingSeeds.SeedBucketUpdated();
            PlantSeed(nextSeed);
            return true;
        }

        private void PlantSeed(Seed toBePlanted)
        {
            polliationState = new PollinationState(toBePlanted);
            plantType = plantTypes.GetUniqueObjectFromID(polliationState.SelfGenes.plantType);
            UpdateGrowth(0, true);
            SetPlanterColliderEnabled();
        }

        public void SetupAfterSpawn()
        {
            polliationState = new PollinationState(new Seed
            {
                genes = plantType.genome.GenerateBaseGenomeData(),
                plantType = plantType.myId
            });
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
            growth = newGrowth;
            UpdatePlant();
        }

        public void UpdatePlant()
        {
            plantsParent.DestroyAllChildren();
            if (plantType == null)
            {
                plantCollider.enabled = false;
                return;
            }
            plantCollider.enabled = true;
            plantType.plantBuilder.BuildPlant(this, GeneticDrivers);
        }

        public GameObject SpawnPlant(GameObject plantPrefab)
        {
            return Instantiate(plantPrefab, plantsParent.transform);
        }

        public bool SelfHit(RaycastHit hit)
        {
            if (hit.collider == planterCollider)
            {
                return OnPlanterClicked();
            }
            if (hit.collider == plantCollider)
            {
                selectedPlant.SetValue(gameObject);
                return true;
            }
            return false;
        }

        public bool CanPollinate()
        {
            return (plantType?.IsInPollinationRange(Growth) ?? false)
                && (polliationState?.CanPollinate() ?? false);
        }

        public bool PollinateFrom(PlantContainer other)
        {
            if(!(plantType?.IsInPollinationRange(Growth) ?? false))
            {
                return false;
            }
            if (polliationState.RecieveGenes(other.polliationState))
            {
                UpdatePlant();
                return true;
            }
            return false;
        }

        public bool TryHarvest()
        {
            if (growth < 1 - 1e-5)
            {
                return false;
            }
            HarvestPlant();
            return true;
        }

        private void HarvestPlant()
        {
            var draggingProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
            var dragger = draggingProvider.SpawnNewDraggingSeedsOrGetCurrent();
            var harvestedSeeds = plantType.HarvestSeeds(polliationState);
            if (!dragger.myBucket.TryAddSeedsToSet(harvestedSeeds))
            {
                return;
            }
            dragger.SeedBucketUpdated();

            growth = 0;
            plantType = null;
            polliationState = null;
            GeneticDrivers = null;
            plantCollider.enabled = false;
            SetPlanterColliderEnabled();
            UpdatePlant();
        }

        #region Saveable
        [System.Serializable]
        class PlantSaveObject
        {
            int plantTypeId;
            float growth;
            PollinationState pollination;
            public PlantSaveObject(PlantContainer source)
            {
                plantTypeId = source.plantType?.myId ?? -1;
                growth = source.growth;
                pollination = source.polliationState;
            }

            public void Apply(PlantContainer target)
            {
                target.plantType = plantTypeId == -1 ? null : target.plantTypes.GetUniqueObjectFromID(plantTypeId);
                target.polliationState = pollination;
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
