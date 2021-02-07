using Assets.Scripts.DataModels;
using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "RandomResultPlantType", menuName = "Greenhouse/RandomResultPlantType", order = 1)]
    public class RandomResultPlantType : BasePlantType
    {

        [Header("Growth")]
        public PlantFormDefinition plantBuilder;
        public float growthPerPhase;
        public float minPollinationGrowthPhase;
        public float maxPollinationGrowthPhase;

        [Header("Seebs")]
        public int minSeeds;
        public int maxSeeds;

        public override PlantState GenerateBaseSate()
        {
            return new PlantState(0);
        }

        public override void AddGrowth(int phaseDiff, PlantState currentState)
        {
            var extraGrowth = growthPerPhase * phaseDiff;
            currentState.growth = Mathf.Clamp(currentState.growth + extraGrowth, 0, 1);
        }

        public override bool CanPollinate(PlantState currentState)
        {
            return currentState.growth >= minPollinationGrowthPhase && currentState.growth <= maxPollinationGrowthPhase;
        }
        public override bool CanHarvest(PlantState currentState)
        {
            return currentState.growth >= 1 - 1e-5;
        }

        public override void BuildPlantInto(
            PlantContainer targetContainer,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination) {
            plantBuilder.BuildPlant(targetContainer, geneticDrivers, currentState, pollination);
        }

        protected override int GetHarvestedSeedNumber(PlantState currentState)
        {
            return Random.Range(minSeeds, maxSeeds);
        }
    }
}
