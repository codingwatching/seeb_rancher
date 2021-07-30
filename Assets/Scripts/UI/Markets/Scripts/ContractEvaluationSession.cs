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

        public FloatReference successfulPlants;
        public FloatReference failedPlants;
        public FloatReference successRatio;

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
            successfulPlants.CurrentValue = 0;
            failedPlants.CurrentValue = 0;
            successRatio.CurrentValue = 0;

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

            yield return new WaitUntil(() => failedPlants + successfulPlants >= totalPlantsToTest);
            SceneManager.SetActiveScene(mainScene);
            areContractsEvaluating.SetValue(false);

            SceneManager.UnloadSceneAsync(farmerScene);

            comlianceRatio = successRatio.CurrentValue;

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
            var matches = contract.contract.Matches(obj);
            if (matches)
            {
                successfulPlants.Increment();
            }else
            {
                failedPlants.Increment();
            }
            obj.SetHarvestEffectColor(matches ? Color.green : Color.red);
            successRatio.CurrentValue = successfulPlants / (successfulPlants + failedPlants);
        }
    }
}