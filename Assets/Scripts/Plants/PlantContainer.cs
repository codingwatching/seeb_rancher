using Assets.Scripts.DataModels;
using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
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
        public BasePlantType plantType;
        public PlantTypeRegistry plantTypes;
        public GameObjectVariable selectedPlant;

        [SerializeField]
        [HideInInspector]
        //private float growth;
        //public float Growth => growth;
        //public int RandomSeed { get; private set; }

        public PlantState currentState;

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
            if(currentState != null && this.plantType != null)
            {
                plantType.AddGrowth(phaseDiff, currentState);
            }
            GrowthUpdated();
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
            this.currentState = plantType.GenerateBaseSate();
            GrowthUpdated(true);
            SetPlanterColliderEnabled();
        }

        /// <summary>
        /// Called whenever the object is spawned through spawning code
        /// </summary>
        public void SetupAfterSpawn()
        {
            plantType = null;
            currentState = null;
            polliationState = null;
            GeneticDrivers = null;
            polliationState = null;
            GrowthUpdated(true);
        }

        /// <summary>
        /// called whenever growth is changed, to conditionally trigger updating the plant.
        /// </summary>
        /// <param name="forcePrefabInstantiate"></param>
        public void GrowthUpdated(bool forcePrefabInstantiate = false)
        {
            if (plantType == null || currentState == null)
            {
                if (forcePrefabInstantiate)
                {
                    UpdatePlant();
                }
                return;
            }
            UpdatePlant();
        }

        /// <summary>
        /// update the redndered plant view based on current state
        /// </summary>
        public void UpdatePlant()
        {
            plantsParent.DestroyAllChildren();
            if (plantType == null || currentState == null)
            {
                plantCollider.enabled = false;
                return;
            }
            plantCollider.enabled = true;
            plantType.BuildPlantInto(this, GeneticDrivers, currentState, polliationState);
        }

        /// <summary>
        /// utility callback used by the PlantBuilders to inject a plant into this container
        /// </summary>
        /// <param name="plantPrefab"></param>
        /// <returns></returns>
        public GameObject SpawnPlantModelObject(GameObject plantPrefab)
        {
            return Instantiate(plantPrefab, plantsParent.transform);
        }

        /// <summary>
        /// Handles clicks from the Click Manipulator
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
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
            if (plantType == null || currentState == null)
            {
                return false;
            }
            return (plantType.IsInPollinationRange(this.currentState))
                && (polliationState?.CanPollinate() ?? false);
        }

        public bool PollinateFrom(PlantContainer other)
        {
            if(plantType == null || currentState == null)
            {
                return false;
            }
            if (!plantType.IsInPollinationRange(this.currentState))
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
            if (currentState  == null|| currentState.growth < 1 - 1e-5)
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
            var harvestedSeeds = plantType.HarvestSeeds(polliationState, currentState);
            if (!dragger.myBucket.TryAddSeedsToSet(harvestedSeeds))
            {
                // TODO: what happens to the seeds if they can't be added? 
                //  we should probably not harvest the plant if the seeds will just dissapear into the void
                return;
            }
            dragger.SeedBucketUpdated();

            plantType = null;
            currentState = null;
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
            PlantState plantState;
            PollinationState pollination;
            public PlantSaveObject(PlantContainer source)
            {
                plantTypeId = source.plantType?.myId ?? -1;
                plantState = source.currentState;
                pollination = source.polliationState;
            }

            public void Apply(PlantContainer target)
            {
                target.plantType = plantTypeId == -1 ? null : target.plantTypes.GetUniqueObjectFromID(plantTypeId);
                target.polliationState = pollination;
                target.currentState = plantState;
                target.currentState.AfterDeserialized();
                target.GrowthUpdated(true);
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
