using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.Utilities.Core;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;
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

        public void InitiateEvaluation(Seed[] seeds, ContractDescriptor contract)
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

        IEnumerator EvaluateSeebs(Seed[] seeds, ContractDescriptor contract)
        {
            var genome = contract.plantType.genome;
            var generationPhase = seeds.SelectMany(seed => SelfPollinateSeed(seed, contract.plantType.minSeeds, contract.plantType.maxSeeds)).ToList();
            // keep pollinating until there's at least 100 seeds
            while(generationPhase.Count < 100)
            {
                yield return new WaitForSeconds(.1f);
                generationPhase = generationPhase.SelectMany(seed => SelfPollinateSeed(seed, contract.plantType.minSeeds, contract.plantType.maxSeeds)).ToList();
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