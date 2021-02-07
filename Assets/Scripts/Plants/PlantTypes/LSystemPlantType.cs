using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
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
        public LSystemPlantState(string axiom, float growth) : base(growth)
        {
            lSystemState = new DefaultLSystemState(axiom, randomSeed);
            totalSystemSteps = 0;
        }

        public void StepStateUpToSteps(
            int targetSteps,
            LSystem<double> system,
            double[] globalParameters = null)
        {
            while (totalSystemSteps < targetSteps)
            {
                system.StepSystem(lSystemState, globalParameters);
                totalSystemSteps++;
            }
        }
    }

    [CreateAssetMenu(fileName = "LSystemPlantType", menuName = "Greenhouse/LSystemPlantType", order = 1)]
    public class LSystemPlantType : BasePlantType
    {
        public TurtleInterpreterBehavior turtleInterpretorPrefab;
        public LSystemObject lSystem;

        public float stepsPerPhase = 3f;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

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
            PlantContainer targetContainer,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }
            var targetSteps = Mathf.FloorToInt(systemState.growth * stepsPerPhase);
            lSystem.Compile();
            var compiledSystem = lSystem.compiledSystem;
            systemState.StepStateUpToSteps(targetSteps, compiledSystem);


            var newPlantTarget = targetContainer.SpawnPlantModelObject(turtleInterpretorPrefab.gameObject).GetComponent<TurtleInterpreterBehavior>();

            newPlantTarget.InterpretSymbols(systemState.lSystemState.currentSymbols);
        }

        public override bool CanPollinate(PlantState currentState)
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
            return systemState.lSystemState.currentSymbols.symbols.Contains((int)seedBearingCharacter);
        }
        protected override int GetHarvestedSeedNumber(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return 0;
            }
            return systemState.lSystemState.currentSymbols.symbols.Sum(symbol => symbol == seedBearingCharacter ? 1 : 0);
        }
    }
}
