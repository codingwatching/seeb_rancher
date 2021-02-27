using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Dman.ObjectSets;
using Dman.ReactiveVariables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractEvaluationController : MonoBehaviour
    {
        public FloatReference money;

        public GameObject loadingSection;
        public GameObject evaluationResultsSection;
        public GameObject evaluationModal;

        public TMP_Text seedPercentageComplianceText;
        public TMP_Text rewardAmountText;

        public static ContractEvaluationController Instance;

        public bool IsEvaluating;
        public float rewardAmount;

        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void InitiateEvaluation(Seed[] seeds, TargetContractDescriptor contract)
        {
            if (IsEvaluating)
            {
                throw new System.Exception("shouldn't be evaluating: evaluation in progress");
            }
            evaluationModal.SetActive(true);
            loadingSection.SetActive(true);
            evaluationResultsSection.SetActive(false);

            StartCoroutine(EvaluateSeebs(seeds, contract));
        }

        /// <summary>
        /// Coroutine to evaluate the fitness of a set of seeds, and write the results into this object
        /// </summary>
        /// <param name="seeds"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        IEnumerator EvaluateSeebs(Seed[] seeds, TargetContractDescriptor contract)
        {
            var plantTypeRegistry = RegistryRegistry.GetObjectRegistry<BasePlantType>();
            var plantType = plantTypeRegistry.GetUniqueObjectFromID(seeds[0].plantType);
            if (seeds.Any(seed => seed.plantType != seeds[0].plantType))
            {
                throw new System.Exception("Cannot evaluate fitness of seeds of different species");
            }
            var genome = contract.plantType.genome;
            var generationPhase = seeds.SelectMany(seed => plantType.SimulateGrowthToHarvest(seed)).ToList();
            // keep pollinating until there's at least 100 seeds
            while (generationPhase.Count < 100)
            {
                yield return new WaitForSeconds(.1f);
                generationPhase = generationPhase.SelectMany(seed => plantType.SimulateGrowthToHarvest(seed)).ToList();
            }
            yield return new WaitForSeconds(.1f);

            var seedsSatisfyingDescriptors = 0;

            for (int i = 0; i < generationPhase.Count; i++)
            {
                var seed = generationPhase[i];
                var resultingDrivers = genome.CompileGenome(seed.genes);
                if (contract.Matches(resultingDrivers))
                {
                    seedsSatisfyingDescriptors++;
                }
            }

            var seedComplianceRatio = ((float)seedsSatisfyingDescriptors) / generationPhase.Count;
            seedPercentageComplianceText.text = $"{seedComplianceRatio:P0}";
            rewardAmount = contract.reward * seedComplianceRatio;
            rewardAmountText.text = $"${rewardAmount:F2}";

            loadingSection.SetActive(false);
            evaluationResultsSection.SetActive(true);
        }

        IEnumerable<Seed> SelfPollinateSeed(Seed seed, int minSeedCopies, int maxSeedCopies)
        {
            var copies = Random.Range(minSeedCopies, maxSeedCopies);
            for (int i = 0; i < copies; i++)
            {
                yield return new Seed
                {
                    plantType = seed.plantType,
                    genes = new Genetics.Genome(seed.genes, seed.genes)
                };
            }
        }

        public void AcceptResults()
        {
            // add rewardAmount
            Debug.Log($"Reward cash {rewardAmount}");
            evaluationModal.SetActive(false);
            IsEvaluating = false;
            money.SetValue(money.CurrentValue + rewardAmount);
        }
    }
}