using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "LSystemBuilder", menuName = "Greenhouse/PlantBuilders/LSystemBuilder", order = 2)]
    public class LSystemBuilder : PlantBuilder
    {
        public GameObject turtleInterpretorPrefab;

        public GameObject flower;
        public GameObject stamens;

        public GeneticDrivenModifier[] geneticModifiers;

        public BooleanGeneticDriver largeOrSmallSwitch;

        public override void BuildPlant(PlantContainer plantParent, CompiledGeneticDrivers geneticDrivers)
        {
        }
        private int GetPrefabIndex(float growth)
        {
            return -1;
        }
        private void ApplyGeneticModifiers(GameObject plant, CompiledGeneticDrivers geneticDrivers)
        {
            foreach (var geneticModifier in geneticModifiers)
            {
                geneticModifier.ModifyObject(plant, geneticDrivers);
            }
        }
    }
}
