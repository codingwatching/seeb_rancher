using Assets.Scripts.DataModels;
using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Dman.ObjectSets;
using Genetics;
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
        public LSystemPlantState(string axiom, float growth): base(growth)
        {
            lSystemState = new DefaultLSystemState(axiom, this.randomSeed);
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
        public float growthPerStep = 0.02f;
        public float growthPerPhase = 0.3f;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

        public override PlantState GenerateBaseSate()
        {
            return new LSystemPlantState(lSystem.axiom, 0);
        }

        public override void AddGrowth(int phaseDiff, PlantState currentState)
        {
            var extraGrowth = growthPerPhase * phaseDiff;
            currentState.growth = Mathf.Clamp(currentState.growth + extraGrowth, 0, 1);
        }

        public override bool IsInPollinationRange(PlantState currentState)
        {
            if(!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            return systemState.lSystemState.currentSymbols.symbols.Contains((int)flowerCharacter);
        }
        public override void BuildPlantInto(
            PlantContainer targetContainer,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination) {

            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }
            var targetSteps = Mathf.FloorToInt(systemState.growth / growthPerStep);
            this.lSystem.Compile();
            var compiledSystem = this.lSystem.compiledSystem;
            systemState.StepStateUpToSteps(targetSteps, compiledSystem);


            var newPlantTarget = targetContainer.SpawnPlantModelObject(turtleInterpretorPrefab.gameObject).GetComponent<TurtleInterpreterBehavior>();

            newPlantTarget.InterpretSymbols(systemState.lSystemState.currentSymbols);
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
