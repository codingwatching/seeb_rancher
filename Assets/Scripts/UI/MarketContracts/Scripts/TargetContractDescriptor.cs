using Assets.Scripts.Plants;
using Genetics.GeneticDrivers;
using System.Linq;

namespace Assets.Scripts.UI.MarketContracts
{
    /// <summary>
    /// class used during runtime to describe a contract
    /// Never goes through odin serialization. Intended to be edited inside the unity editor
    /// </summary>
    [System.Serializable]
    public class TargetContractDescriptor
    {
        public BooleanGeneticTarget[] targets;
        public float reward;
        public BasePlantType plantType;
        public int seedRequirement;

        public bool Matches(CompiledGeneticDrivers drivers)
        {
            if (!targets.All(boolTarget =>
                     drivers.TryGetGeneticData(boolTarget.targetDriver, out var boolValue)
                     && boolValue == boolTarget.targetValue))
            {
                return false;
            }
            return true;
        }
    }
}
