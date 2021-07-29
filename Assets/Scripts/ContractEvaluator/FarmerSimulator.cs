using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Dman.ObjectSets;
using Dman.ReactiveVariables;
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
        public FloatReference simulationSpeed;
        public StochasticTimerFrequencyVaried plantSpawnFrequency;


        public List<Seed> seedPool;
        public int seedPoolCap = 500;
        public int totalPlantsGrown = 0;
        public event System.Action<PlantedLSystem> onPlantHarvested;
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

            seedPool = Enumerable.Range(0, 100)
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
                UnityEngine.Profiling.Profiler.BeginSample("spawning a plant");
                SpawnPlant();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            var seebsWereAdded = false;
            for (int i = 0; i < tendedPlants.Count; i++)
            {
                var system = tendedPlants[i];
                var seebs = system.TryStep();
                if (seebs != null)
                {
                    tendedPlants.RemoveAt(i);
                    i--;
                    if(seebs.Length > 0)
                    {
                        seebsWereAdded = true;
                        seedPool.AddRange(seebs);
                    }
                }
            }
            if (seebsWereAdded)
            {
                seedPool.Shuffle();
                if (seedPool.Count > seedPoolCap)
                {
                    seedPool.RemoveRange(0, seedPool.Count - seedPoolCap);
                }
            }
        }


        private void SpawnPlant()
        {
            var rayPoint = sequenceGenerator.Sample();
            rayPoint.Scale(spawnExtent);
            rayPoint += new Vector2(transform.position.x, transform.position.z);

            var ray = new Ray(new Vector3(rayPoint.x, transform.position.y, rayPoint.y), Vector3.down);
            if (!Physics.Raycast(ray, out var hit, 100f, plantableThings))
            {
                return;
            }

            var nextSeedIndex = 0; // seed pool list is shuffled
            var nextSeed = seedPool[nextSeedIndex];
            seedPool.RemoveAt(nextSeedIndex);

            var newPlant = farmedPlant.SpawnNewPlant(hit.point, nextSeed, false);
            totalPlantsGrown++;
            tendedPlants.Add(new FarmedLSystem(newPlant, this));
        }

        private void OnDrawGizmosSelected()
        {
            var newColor = Color.green;
            newColor.a = 0.4f;
            Gizmos.color = newColor;
            DrawGizmoBox();
        }

        private void DrawGizmoBox()
        {
            Gizmos.DrawCube(transform.position, new Vector3(spawnExtent.x * 2, 0.1f, spawnExtent.y * 2));
            DrawArrow.ForGizmo(transform.position, Vector3.down);
        }

        [System.Serializable]
        class FarmedLSystem
        {
            public StochasticTimerFrequencyVaried stepTimer;
            public StochasticTimerFrequencyVaried pollinateTimer;
            public float timeReachedMaturity = 0;
            public PlantedLSystem plant;
            public FarmerSimulator parent;

            public FarmedLSystem(
                PlantedLSystem plant,
                FarmerSimulator parent)
            {
                this.plant = plant;
                stepTimer = new StochasticTimerFrequencyVaried(plant.plantType.updateStepTiming);
                pollinateTimer = new StochasticTimerFrequencyVaried(plant.plantType.pollinationSpreadTiming);
                this.parent = parent;
            }

            public Seed[] TryStep()
            {
                UnityEngine.Profiling.Profiler.BeginSample("plant farming step");
                try
                {
                    var stepper = plant.lSystemManager.steppingHandle;
                    if (timeReachedMaturity != 0 && timeReachedMaturity + 3 < Time.time)
                    {
                        plant.pollinationState.SelfPollinateIfNotFertile();
                        parent.onPlantHarvested?.Invoke(plant);
                        return plant.TryHarvest();
                    }
                    if (plant.IsMature() && plant.CanHarvest() && timeReachedMaturity == 0)
                    {
                        timeReachedMaturity = Time.time;
                    }
                    if (stepTimer.Tick() && stepper.CanStep())
                    {
                        plant.StepOnce();
                    }
                    if (pollinateTimer.Tick(parent.simulationSpeed.CurrentValue) && plant.CanPollinate())
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("pollination");
                        plant.SprayMySeed();
                        UnityEngine.Profiling.Profiler.EndSample();
                    }

                    return null;
                }
                finally
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }
    }
}