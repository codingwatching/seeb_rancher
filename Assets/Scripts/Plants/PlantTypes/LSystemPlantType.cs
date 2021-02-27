using Assets.Scripts.DataModels;
using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [System.Serializable]
    public class LSystemPlantState : PlantState
    {
        [System.NonSerialized()]
        public DefaultLSystemState lSystemState;
        [System.NonSerialized()]
        public int totalSystemSteps;
        [System.NonSerialized()]
        public bool lastStepChanged;

        //cached stuff
        [System.NonSerialized()]
        private LSystem<double> compiledSystem;
        [System.NonSerialized()]
        public ArrayParameterRepresenation<double> runtimeParameters;

        public float randomRotationAmount;
        private string axiom;

        public LSystemPlantState(string axiom, float growth) : base(growth)
        {
            // used to generate properties for the plant not related to the L-system. such as random rotation
            // Is a duplicate, parallel, random generator to the one used by the l-system
            var ephimeralRandoms = new System.Random(randomSeed);
            totalSystemSteps = 0;
            randomRotationAmount = (float)(ephimeralRandoms.NextDouble() * 360);

            this.axiom = axiom;
            lSystemState = new DefaultLSystemState(this.axiom, randomSeed);
        }

        public override void AfterDeserialized()
        {
            base.AfterDeserialized();
            lSystemState = new DefaultLSystemState(axiom, randomSeed);
            totalSystemSteps = 0;
        }

        public void CompileSystemIfNotCached(
            FloatGeneticDriverToLSystemParameter[] geneticModifiers,
            CompiledGeneticDrivers geneticDrivers,
            LSystemObject system)
        {
            if(this.compiledSystem != null)
            {
                return;
            }
            Debug.Log("compiling System");
            var geneticModifiedParameters = geneticModifiers
                .Select(x =>
                {
                    if (geneticDrivers.TryGetGeneticData(x.geneticDriver, out var driverValue))
                    {
                        return new { x.lSystemDefineDirectiveName, driverValue };
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToDictionary(x => x.lSystemDefineDirectiveName, x => x.driverValue.ToString());
            this.compiledSystem = system.CompileWithParameters(geneticModifiedParameters);

            this.runtimeParameters = system.GetRuntimeParameters();
        }

        public void StepStateUpToSteps(int targetSteps)
        {
            var ruintimeParamValues = runtimeParameters.GetCurrentParameters();
            while (totalSystemSteps < targetSteps)
            {
                var lastSymbols = lSystemState.currentSymbols;
                compiledSystem.StepSystem(lSystemState, ruintimeParamValues);
                lastStepChanged = !lastSymbols.Equals(lSystemState.currentSymbols);
                totalSystemSteps++;
            }
        }
    }

    [System.Serializable]
    public struct FloatGeneticDriverToLSystemParameter
    {
        public FloatGeneticDriver geneticDriver;
        public string lSystemDefineDirectiveName;
    }

    [CreateAssetMenu(fileName = "LSystemPlantType", menuName = "Greenhouse/LSystemPlantType", order = 1)]
    public class LSystemPlantType : BasePlantType
    {
        public TurtleInterpreterBehavior turtleInterpretorPrefab;
        public LSystemObject lSystem;

        public float stepsPerPhase = 3f;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override PlantState GenerateBaseSate()
        {
            return new LSystemPlantState(lSystem.axiom, 0);
        }

        /// <summary>
        /// In this implemntation, the growth property is used to directly reflect how many phases the plant has aged through.
        ///     during rendering this is converted into L-system steps
        /// This is done because there is no known end-time for the L-system plant, the harvestability and pollination state
        ///     is derived only from the state of the L-system instead of a special value of the growth variable
        /// </summary>
        /// <param name="phaseDiff"></param>
        /// <param name="currentState"></param>
        public override void AddGrowth(int phaseDiff, PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }
            systemState.growth += phaseDiff;
        }

        public override void BuildPlantInto(
            Transform targetContainer,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }

            systemState.CompileSystemIfNotCached(geneticModifiers, geneticDrivers, lSystem);
            systemState.runtimeParameters.SetParameter("hasAnther", pollination.HasAnther ? 1 : 0);
            systemState.runtimeParameters.SetParameter("isPollinated", pollination.HasAnther ? 1 : 0);

            var targetSteps = Mathf.FloorToInt(systemState.growth * stepsPerPhase);
            systemState.StepStateUpToSteps(targetSteps);


            var newPlantTarget = Instantiate(turtleInterpretorPrefab, targetContainer);

            newPlantTarget.InterpretSymbols(systemState.lSystemState.currentSymbols);
            var lastAngles = newPlantTarget.transform.parent.localEulerAngles;
            lastAngles.y = systemState.randomRotationAmount;
            newPlantTarget.transform.parent.localEulerAngles = lastAngles;
        }

        public override bool HasFlowers(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            return systemState.lSystemState.currentSymbols.symbols.Contains((int)flowerCharacter);
        }
        public override bool CanHarvest(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            // if the plant is done growing always allow harvesting to avoid letting plants with no fruit hang around
            return !systemState.lastStepChanged || systemState.lSystemState.currentSymbols.symbols.Contains((int)seedBearingCharacter);
        }
        protected override int GetHarvestedSeedNumber(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return 0;
            }
            return systemState.lSystemState.currentSymbols.symbols.Sum(symbol => symbol == seedBearingCharacter ? 1 : 0);
        }
        public override IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed)
        {
            // TODO: these seed counts are picked out of the blue. Consider running the l-system to get the seed counts
            //  for a more accurate simulation.
            //  more relevent if there are certain trait combinations that can sterilize a seed
            return SelfPollinateSeed(seed, 1, 5);
        }
        IEnumerable<Seed> SelfPollinateSeed(Seed seed, int minSeedCopies, int maxSeedCopies)
        {
            var copies = Random.Range(minSeedCopies, maxSeedCopies);
            for (int i = 0; i < copies; i++)
            {
                yield return new Seed
                {
                    plantType = seed.plantType,
                    genes = new Genetics.Genome(seed.genes, seed.genes)
                };
            }
        }
    }
}
