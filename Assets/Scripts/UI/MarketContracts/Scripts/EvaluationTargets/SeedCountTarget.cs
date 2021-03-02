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
    public class SeedCountTarget : IContractTarget
    {
        public int minSeeds;

        public string GetDescriptionOfTarget()
        {
            return $"yield at least {minSeeds} seeds per plant";
        }
    }
}