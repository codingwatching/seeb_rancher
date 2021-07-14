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

            var mainScene = SceneManager.GetActiveScene();

            var sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(simulationScene.scenePath);
            var sceneLoader = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
            yield return new WaitUntil(() => sceneLoader.isDone);

            // TODO: hide the ui?

            var farmerScene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
            var rootObjs = farmerScene.GetRootGameObjects();
            var farmer = rootObjs
                .Select(x => x.GetComponent<FarmerSimulator>())
                .Where(x => x != null)
                .FirstOrDefault();
            if(farmer == null)
            {
                Debug.LogError("could not find a farmer in the new scene");
            }
            farmer.BeginSimulation(seeds);

            SceneManager.SetActiveScene(farmerScene);
            areContractsEvaluating.SetValue(true);

            yield return new WaitUntil(() => farmer.totalPlantsGrown >= targetSeedQuantity);// TODO: rename targetSeedQuanity
            SceneManager.SetActiveScene(mainScene);
            areContractsEvaluating.SetValue(false);

            SceneManager.UnloadSceneAsync(farmerScene);

            var resultSeebs = farmer.seedPool;

            yield return StartCoroutine(contractData.EvaluateComplianceOfSeeds(resultSeebs));

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