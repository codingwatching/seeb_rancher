using Assets.Scripts.ContractEvaluator;
using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Dman.ObjectSets;
using Dman.ReactiveVariables;
using Dman.Utilities;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractEvaluationSession
    {
        private Seed[] seeds;
        private ContractContainer contract;
        private SceneReference simulationScene;
        private BooleanVariable areContractsEvaluating;
        private int totalPlantsToTest;

        private MonoBehaviour coroutineOwner;

        public float rewardResult = 0f;
        public float comlianceRatio = 0f;

        public ContractEvaluationSession(
            Seed[] seeds,
            ContractContainer container,
            SceneReference simulationScene,
            BooleanVariable areContractsEvaluating,
            int totalPlantsToTest,
            MonoBehaviour coroutineOwner)
        {
            this.seeds = seeds;
            this.contract = container;
            this.simulationScene = simulationScene;

            this.areContractsEvaluating = areContractsEvaluating;
            this.totalPlantsToTest = totalPlantsToTest;

            this.coroutineOwner = coroutineOwner;
        }

        public IEnumerator BeginSession()
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
                .Select(x => x.GetComponentInChildren<FarmerSimulator>())
                .Where(x => x != null)
                .FirstOrDefault();
            if (farmer == null)
            {
                Debug.LogError("could not find a farmer in the new scene");
            }

            farmer.onPlantHarvested += SimulatedPlantHarvested;

            farmer.BeginSimulation(seeds);

            SceneManager.SetActiveScene(farmerScene);
            areContractsEvaluating.SetValue(true);

            yield return new WaitUntil(() => farmer.totalPlantsGrown >= totalPlantsToTest);
            SceneManager.SetActiveScene(mainScene);
            areContractsEvaluating.SetValue(false);

            SceneManager.UnloadSceneAsync(farmerScene);

            var resultSeebs = farmer.seedPool;

            yield return coroutineOwner.StartCoroutine(contractData.EvaluateComplianceOfSeeds(resultSeebs));

            comlianceRatio = contractData.ComplianceResult;

            if (comlianceRatio >= contractData.minimumComplianceRatio)
            {
                GameObject.Destroy(contract.gameObject);
                rewardResult = contractData.reward * comlianceRatio;
            }
            else
            {
                rewardResult = 0;
            }
        }

        private void SimulatedPlantHarvested(PlantedLSystem obj)
        {
            Debug.Log("plant harvested");
        }
    }
}