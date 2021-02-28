using Assets.Scripts.Plants;
using Genetics.GeneticDrivers;
using System.Linq;
using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Assets.Scripts.DataModels;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using Dman.ObjectSets;
using Assets;
using System.Collections;

namespace Assets.Scripts.UI.MarketContracts
{
    /// <summary>
    /// class used during runtime to describe a contract
    /// Goes through odin serialization. Intended to be edited inside the unity editor
    /// </summary>
    [System.Serializable]
    public class TargetContractDescriptor: ISerializable
    {
        public BooleanGeneticTarget[] booleanTargets;
        public FloatGeneticTarget[] floatTargets;
        [Tooltip("only ever put one thing in here")]
        public SeedCountTarget[] seedCountTarget;
        public float reward;
        public BasePlantType plantType;
        [Tooltip("how many seeds need to be submitted to evaluate the contract")]
        public int seedRequirement;

        public TargetContractDescriptor()
        {}

        #region Seed Compliance
        public float _complianceResult;
        public IEnumerator EvaluateComplianceOfSeeds(IEnumerable<Seed> seeds)
        {
            var seedsSatisfyingDescriptors = 0;
            var totalSeeds = 0;
            foreach (var seed in seeds)
            {
                totalSeeds++;
                yield return new WaitForEndOfFrame();
                if (Matches(seed))
                {
                    seedsSatisfyingDescriptors++;
                }
            }
            yield return new WaitForEndOfFrame();

            var seedComplianceRatio = ((float)seedsSatisfyingDescriptors) / totalSeeds;
            _complianceResult = seedComplianceRatio;
            //return seedComplianceRatio;
        }

        private bool Matches(Seed seed)
        {
            var drivers = plantType.genome.CompileGenome(seed.genes);

            if (!booleanTargets.All(boolTarget =>
                     drivers.TryGetGeneticData(boolTarget.targetDriver, out var boolValue)
                     && boolValue == boolTarget.targetValue))
            {
                return false;
            }
            if (!floatTargets.All(floatTarget => floatTarget.Matches(drivers)))
            {
                return false;
            }
            if (seedCountTarget.Length > 0)
            {
                var generatedSeeds = plantType.SimulateGrowthToHarvest(seed).Count();
                if(generatedSeeds < seedCountTarget[0].minSeeds)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("booleanTargets", booleanTargets);
            info.AddValue("floatTargets", floatTargets);
            info.AddValue("seedCountTarget", seedCountTarget);
            info.AddValue("reward", reward);
            info.AddValue("seedRequirement", seedRequirement);
            info.AddValue("plantType", new IDableSavedReference(plantType));
        }


        // The special constructor is used to deserialize values.
        private TargetContractDescriptor(SerializationInfo info, StreamingContext context)
        {
            booleanTargets = info.GetValue<BooleanGeneticTarget[]>("booleanTargets");
            floatTargets = info.GetValue<FloatGeneticTarget[]>("floatTargets");
            seedCountTarget = info.GetValue<SeedCountTarget[]>("seedCountTarget");

            reward = info.GetSingle("reward");
            seedRequirement = info.GetInt32("seedRequirement");

            var savedReference = info.GetValue("plantType", typeof(IDableSavedReference)) as IDableSavedReference;
            plantType = savedReference?.GetObject<BasePlantType>();
        }

        #endregion
    }
}
