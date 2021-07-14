using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Dman.LSystem.UnityObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ContractEvaluator
{

    public class FarmerSimulator : MonoBehaviour
    {
        public LSystemPlantType farmedPlant;
        public Vector2 spawnExtent = Vector2.one;

        public LayerMask plantableThings;
        public int maxConcurrentPlants;
        public StochasticTimerFrequencyVaried plantSpawnFrequency;
        public StochasticTimerFrequencyVaried plantUpdateFrequency;


        public List<Seed> seedPool;
        public int totalPlantsGrown = 0;
        [SerializeField] private List<FarmedLSystem> tendedPlants;
        private HaltonSequenceGenerator sequenceGenerator;

        public void BeginSimulation(IEnumerable<Seed> seeds)
        {
            sequenceGenerator = new HaltonSequenceGenerator(2, 3, Random.Range(0, 1000), -Vector2.one, Vector2.one);
            seedPool = seeds
                .ToList();
            totalPlantsGrown = 0;
        }

        private void Awake()
        {
            sequenceGenerator = new HaltonSequenceGenerator(2, 3, Random.Range(0, 1000), -Vector2.one, Vector2.one);

            var randomProvider = new System.Random(Random.Range(1, int.MaxValue));

            seedPool = Enumerable.Range(0, 10)
                .Select(x => farmedPlant.GenerateRandomSeed(randomProvider))
                .ToList();
            totalPlantsGrown = 0;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (tendedPlants.Count < maxConcurrentPlants && seedPool.Count > 0 && plantSpawnFrequency.Tick())
            {
                this.SpawnPlant();
            }

            for (int i = 0; i < tendedPlants.Count; i++)
            {
                var system = tendedPlants[i];
                var seebs = system.TryStep();
                if (seebs != null)
                {
                    tendedPlants.RemoveAt(i);
                    i--;
                    seedPool.AddRange(seebs);
                }
            }
        }


        private void SpawnPlant()
        {
            var rayPoint = sequenceGenerator.Sample();
            rayPoint.Scale(spawnExtent);
            rayPoint += new Vector2(transform.position.x, transform.position.z);

            var ray = new Ray(new Vector3(rayPoint.x, transform.position.y, rayPoint.y), Vector3.down);
            if(!Physics.Raycast(ray, out var hit, 100f, plantableThings))
            {
                return;
            }
            var nextSeedIndex = Random.Range(0, seedPool.Count);
            var nextSeed = seedPool[nextSeedIndex];
            seedPool.RemoveAt(nextSeedIndex);

            var newPlant = farmedPlant.SpawnNewPlant(hit.point, nextSeed, false);
            totalPlantsGrown++;

            this.tendedPlants.Add(new FarmedLSystem(newPlant, plantUpdateFrequency, this));
        }

        private void OnDrawGizmosSelected()
        {
            var newColor = Color.green;
            newColor.a = 0.4f;
            Gizmos.color = newColor;
            Gizmos.DrawCube(transform.position, new Vector3(spawnExtent.x * 2, 0.1f, spawnExtent.y * 2));
            DrawArrow.ForGizmo(transform.position, Vector3.down);
        }

        [System.Serializable]
        class FarmedLSystem
        {
            public StochasticTimerFrequencyVaried stepTimer;
            public PlantedLSystem plant;
            public FarmerSimulator parent;

            public FarmedLSystem(PlantedLSystem plant, StochasticTimerFrequencyVaried timerDefinition, FarmerSimulator parent)
            {
                this.plant = plant;
                stepTimer = new StochasticTimerFrequencyVaried(timerDefinition);
                this.parent = parent;
            }

            public Seed[] TryStep()
            {
                var stepper = plant.lSystemManager.steppingHandle;
                if (plant.IsMature())
                {
                    // TODO: do actual breeding
                    plant.pollinationState.SelfPollinateIfNotFertile();
                    return plant.TryHarvest();
                }
                if (!stepTimer.Tick())
                {
                    return null;
                }
                if (!stepper.CanStep())
                {
                    return null;
                }

                plant.lSystemManager.StepSystem();
                return null;
            }
        }
    }
}