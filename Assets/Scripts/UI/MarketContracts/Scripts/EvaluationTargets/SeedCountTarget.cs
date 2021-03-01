namespace Assets.Scripts.UI.MarketContracts.EvaluationTargets
{
    [System.Serializable] // odin and unity inspector
    public class SeedCountTarget : IContractTarget
    {
        public int minSeeds;

        public string GetDescriptionOfTarget()
        {
            return $"Yield at least {minSeeds} seeds";
        }
    }
}