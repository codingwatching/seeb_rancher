using Genetics.GeneticDrivers;
using Genetics.ParameterizedGenomeGenerator;

namespace Assets.Scripts.UI.MarketContracts.EvaluationTargets
{
    [System.Serializable] // unity inspector
    public class SeedCountTargetRandomGenerator
    {
        public int minSeedRequirement;
        public int maxSeedRequirement;
        public SeedCountTarget GenerateTarget()
        {
            return new SeedCountTarget
            {
                minSeeds = UnityEngine.Random.Range(minSeedRequirement, maxSeedRequirement)
            };
        }
    }


    [System.Serializable] // odin and unity inspector
    public class SeedCountTarget : IGeneticTarget
    {
        public int minSeeds;

        public string GetDescriptionOfTarget()
        {
            return $"yield at least {minSeeds} seebs per plant";
        }

        public bool Matches(CompiledGeneticDrivers drivers)
        {
            // doesn't use drivers. uses plant state. may not be a good fit for genetic target interface
            throw new System.NotImplementedException();
        }
    }
}