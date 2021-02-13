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

        public GameObject planter;
        public GameObject plantsParent;

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

        /// <summary>
        /// Handles clicks from the Click Manipulator
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public bool SelfHit(RaycastHit hit)
        {
            if (this.plantType == null)
            {
                return false;
            }
            selectedPlant.SetValue(gameObject);
            return true;
        }

        public bool CanPlantSeed { get => plantType == null; }

        public void PlantSeed(Seed toBePlanted)
        {
            polliationState = new PollinationState(toBePlanted);
            plantType = plantTypes.GetUniqueObjectFromID(polliationState.SelfGenes.plantType);
            this.currentState = plantType.GenerateBaseSate();
            GrowthUpdated(true);
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
                return;
            }
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

        public bool CanPollinate()
        {
            if (plantType == null || currentState == null)
            {
                return false;
            }
            return (plantType.CanPollinate(this.currentState))
                && (polliationState?.CanPollinate() ?? false);
        }

        public bool PollinateFrom(PlantContainer other)
        {
            if(plantType == null || currentState == null)
            {
                return false;
            }
            if (!plantType.CanPollinate(this.currentState))
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

        public Seed[] TryHarvest()
        {
            if (currentState == null || plantType == null || !plantType.CanHarvest(currentState))
            {
                return new Seed[0];
            }
            return HarvestPlant();
        }

        private Seed[] HarvestPlant()
        {
            var harvestedSeeds = plantType.HarvestSeeds(polliationState, currentState);

            plantType = null;
            currentState = null;
            polliationState = null;
            GeneticDrivers = null;
            UpdatePlant();

            return harvestedSeeds;
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
                target.currentState?.AfterDeserialized();
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
