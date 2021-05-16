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

        public GameObject contractThinkingObject;
        public GameObject contractSuccessObj;
        public GameObject contractRejectObj;

        public static ContractEvaluationController Instance;

        public int targetSeedQuantity = 100;

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

        private void ClearHeadings()
        {
            contractThinkingObject.SetActive(false);
            contractSuccessObj.SetActive(false);
            contractRejectObj.SetActive(false);
        }

        public void InitiateEvaluation(Seed[] seeds, ContractContainer contract)
        {
            if (IsEvaluating)
            {
                throw new System.Exception("shouldn't be evaluating: evaluation in progress");
            }
            IsEvaluating = true;

            evaluationModal.SetActive(true);
            loadingSection.SetActive(true);
            evaluationResultsSection.SetActive(false);

            ClearHeadings();
            contractThinkingObject.SetActive(true);

            StartCoroutine(EvaluateSeebs(seeds, contract));
        }

        /// <summary>
        /// Coroutine to evaluate the fitness of a set of seeds, and write the results into this object
        /// </summary>
        /// <param name="seeds"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        IEnumerator EvaluateSeebs(Seed[] seeds, ContractContainer contract)
        {
            var plantTypeRegistry = RegistryRegistry.GetObjectRegistry<BasePlantType>();
            var plantType = plantTypeRegistry.GetUniqueObjectFromID(seeds[0].plantType);
            if (seeds.Any(seed => seed.plantType != seeds[0].plantType))
            {
                throw new System.Exception("Cannot evaluate fitness of seeds of different species");
            }

            var contractData = contract.contract;

            var genome = contractData.plantType.genome;
            var generationPhase = seeds.ToList();
            // keep pollinating until there's at least 100 seeds
            while (generationPhase.Count < targetSeedQuantity)
            {
                var previousGeneration = generationPhase;
                generationPhase = new List<Seed>();
                foreach (var seed in previousGeneration)
                {
                    yield return new WaitForEndOfFrame();
                    generationPhase.AddRange(plantType.SimulateGrowthToHarvest(seed));
                    if (generationPhase.Count >= targetSeedQuantity)
                    {
                        break;
                    }
                }
            }

            yield return StartCoroutine(contractData.EvaluateComplianceOfSeeds(generationPhase));

            var seedComplianceRatio = contractData.ComplianceResult;

            if (seedComplianceRatio >= contractData.minimumComplianceRatio)
            {
                Destroy(contract.gameObject);
                rewardAmount = contractData.reward * seedComplianceRatio;
                ClearHeadings();
                contractSuccessObj.SetActive(true);
            }
            else
            {
                rewardAmount = 0;
                ClearHeadings();
                contractRejectObj.SetActive(true);
            }

            seedPercentageComplianceText.text = $"{seedComplianceRatio:P0}";
            rewardAmountText.text = $"${rewardAmount:F2}";

            loadingSection.SetActive(false);
            evaluationResultsSection.SetActive(true);
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