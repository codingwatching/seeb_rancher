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
    public class LSystemBuilder : PlantBuilder
    {
        public TurtleInterpreterBehavior turtleInterpretorPrefab;

        public LSystemObject lSystem;
        public float growthPerStep = 0.1f;

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override void BuildPlant(PlantContainer plantParent, CompiledGeneticDrivers geneticDrivers)
        {
            var newPlantTarget = plantParent.SpawnPlant(turtleInterpretorPrefab.gameObject).GetComponent<TurtleInterpreterBehavior>();
            var steps = Mathf.FloorToInt(plantParent.Growth / growthPerStep);

            lSystem.Compile();
            var compiledSystem = lSystem.compiledSystem;
            var systemState = new DefaultLSystemState(lSystem.axiom, plantParent.RandomSeed);
            for (int i = 0; i < steps; i++)
            {
                compiledSystem.StepSystem(systemState);
            }

            newPlantTarget.InterpretSymbols(systemState.currentSymbols);

        }
        private int GetPrefabIndex(float growth)
        {
            return -1;
        }
    }
}
