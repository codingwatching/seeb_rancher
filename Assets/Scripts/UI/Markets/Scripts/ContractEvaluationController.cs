using Assets.Scripts.ContractEvaluator;
using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Dman.ObjectSets;
using Dman.ReactiveVariables;
using Dman.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractEvaluationController : MonoBehaviour
    {
        public SceneReference simulationScene;

        public FloatReference money;

        public BooleanVariable areContractsEvaluating;

        public GameObject loadingSection;
        public GameObject evaluationResultsSection;
        public GameObject evaluationModal;


        public TMP_Text seedPercentageComplianceText;
        public TMP_Text rewardAmountText;

        public GameObject contractThinkingObject;
        public GameObject contractSuccessObj;
        public GameObject contractRejectObj;

        public static ContractEvaluationController Instance;

        public int targetPlantsTested = 100;

        public bool IsEvaluating;
        public float rewardAmount;

        public FloatReference successfulPlants;
        public FloatReference failedPlants;
        public FloatReference successRatio;

        public EventGroup narrativeUpdateTrigger;

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
            var evaluationSession = new ContractEvaluationSession(
                seeds,
                contract,
                simulationScene,
                areContractsEvaluating,
                targetPlantsTested,
                this)
            {
                successfulPlants = successfulPlants,
                failedPlants = failedPlants,
                successRatio = successRatio
            };

            yield return StartCoroutine(evaluationSession.BeginSession());

            rewardAmount = evaluationSession.rewardResult;
            var seedComplianceRatio = evaluationSession.comlianceRatio;

            ClearHeadings();
            if (rewardAmount >= 0)
            {
                contractSuccessObj.SetActive(true);
            }
            else
            {
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
            narrativeUpdateTrigger.TriggerEvent();
        }
    }
}