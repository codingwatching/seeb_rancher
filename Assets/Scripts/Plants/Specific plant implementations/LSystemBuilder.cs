using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public struct FloatGeneticDriverToLSystemParameter
    {
        public FloatGeneticDriver geneticDriver;
        public string lSystemDefineDirectiveName;
    }

    [CreateAssetMenu(fileName = "LSystemBuilder", menuName = "Greenhouse/PlantBuilders/LSystemBuilder", order = 2)]
    public class LSystemBuilder : PlantFormDefinition
    {
        public TurtleInterpreterBehavior turtleInterpretorPrefab;

        public LSystemObject lSystem;
        public float growthPerStep = 0.1f;

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override void BuildPlant(
            PlantContainer plantParent,
            CompiledGeneticDrivers geneticDrivers,
            PlantState plantState,
            PollinationState pollination)
        {
            var newPlantTarget = plantParent.SpawnPlantModelObject(turtleInterpretorPrefab.gameObject).GetComponent<TurtleInterpreterBehavior>();
            var steps = Mathf.FloorToInt(plantState.growth / growthPerStep);

            lSystem.Compile();
            var compiledSystem = lSystem.compiledSystem;
            var systemState = new DefaultLSystemState(lSystem.axiom, plantState.randomSeed);
            for (int i = 0; i < steps; i++)
            {
                compiledSystem.StepSystem(systemState);
            }

            newPlantTarget.InterpretSymbols(systemState.currentSymbols);

        }
    }
}
