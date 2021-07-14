using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Dman.ObjectSets;
using Genetics.ParameterizedGenomeGenerator;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    /// <summary>
    /// class used during runtime to describe a contract
    /// Goes through binary serialization. Intended to be edited inside the unity editor
    /// </summary>
    [System.Serializable]
    public class TargetContractDescriptor : ISerializable
    {
        public BooleanGeneticTarget[] booleanTargets;
        public FloatGeneticTarget[] floatTargets;
        [Tooltip("This array should contain either 0 or 1 elements, never more")]
        public SeedCountTarget[] seedCountTarget;
        public float reward;
        public BasePlantType plantType;
        [Tooltip("how many seeds need to be submitted to evaluate the contract")]
        public int seedRequirement;
        [Tooltip("The lower bound for compliance in order for this contract to be completed. Seeds which do not satisfy this compliance will not cause the contract to be removed.")]
        [Range(0f, 1f)]
        public float minimumComplianceRatio;
        public int expirationTime;

        public TargetContractDescriptor()
        {
        }

        #region Seed Compliance
        public float ComplianceResult { get; set; }

        public IEnumerator EvaluateComplianceOfSeeds(IEnumerable<Seed> seeds, int seedsPerPause = 20)
        {
            var seedsSatisfyingDescriptors = 0;
            var totalSeeds = 0;
            foreach (var seed in seeds)
            {
                totalSeeds++;
                if(totalSeeds % seedsPerPause == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (Matches(seed))
                {
                    seedsSatisfyingDescriptors++;
                }
            }
            yield return new WaitForEndOfFrame();

            var seedComplianceRatio = ((float)seedsSatisfyingDescriptors) / totalSeeds;
            ComplianceResult = seedComplianceRatio;
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
                if (generatedSeeds < seedCountTarget[0].minSeeds)
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
            info.AddValue("minimumCompliance", minimumComplianceRatio);
            info.AddValue("expirationTime", expirationTime);
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
            minimumComplianceRatio = info.GetSingle("minimumCompliance");
            expirationTime = info.GetInt32("expirationTime");

            var savedReference = info.GetValue("plantType", typeof(IDableSavedReference)) as IDableSavedReference;
            plantType = savedReference?.GetObject<BasePlantType>();
        }

        #endregion
    }
}
