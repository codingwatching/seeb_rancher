using Assets.Scripts.Plants;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using Genetics.GeneticDrivers;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    /// <summary>
    /// class used during runtime to describe a contract
    /// </summary>
    [System.Serializable]
    public class ContractDescriptor
    {
        public BooleanGeneticTarget[] targets;
        public float reward;
        public RandomResultPlantType plantType;
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

    public class MarketManager : MonoBehaviour
    {

        [Header("Contract prefabs")]
        public ContractContainer contractOfferPrefab;
        public SaveablePrefabParent marketModalContractsParent;
        public GameObject marketModal;
        public ContractContainer claimedContractPrefab;
        public SaveablePrefabParent claimedContractsModalParent;
        public GameObject claimedContractModal;
        public EventGroup onModalOpened;

        [Header("Contract generation parameters")]
        public BooleanReference contractGenerationEnabled;
        public IntReference levelPhase;
        public float defaultReward;
        [Tooltip("For every extra genetic driver over 1, multiply the reward by this amount. 3 genetic drivers will be defaultReward * multiplierPerAdditional^2")]
        public float multiplierPerAdditional;
        public BooleanGeneticDriver[] targetBooleanDrivers;
        [Range(0, 1)]
        public float chanceForNewContractPerPhase;
        public int defaultSeedCountRequirement = 5;
        public RandomResultPlantType defaultPlantType;

        public static MarketManager Instance;

        private void Awake()
        {
            Instance = this;
            levelPhase.ValueChanges
                .TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (!contractGenerationEnabled.CurrentValue)
                    {
                        return;
                    }
                    if (pair.Current - pair.Previous != 1)
                    {
                        return;
                    }
                    var hasNewContract = Random.Range(0f, 1f) < chanceForNewContractPerPhase;
                    if (hasNewContract)
                    {
                        GenerateNewContract();
                    }
                }).AddTo(this);
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        public void TriggerNewContractGeneration()
        {
            GenerateNewContract();
        }
        private void GenerateNewContract()
        {
            var rangeSample = Random.Range(0f, 1f - 1e-5f);
            // varies from 1 to targetSetLength, weighted towards lower numbers
            var numberOfTargets = Mathf.FloorToInt(Mathf.Pow(rangeSample, 2) * targetBooleanDrivers.Length) + 1;

            var targetIndexes = ArrayExtensions.SelectIndexSources(numberOfTargets, targetBooleanDrivers.Length);
            var chosenDrivers = targetIndexes.Select(index => targetBooleanDrivers[index]);
            var newPrice = defaultReward * (Mathf.Pow(multiplierPerAdditional, numberOfTargets));

            var newTargets = chosenDrivers
                .Select(x => new BooleanGeneticTarget(x))
                .ToArray();
            CreateMarketContract(new ContractDescriptor
            {
                targets = newTargets,
                reward = newPrice,
                seedRequirement = defaultSeedCountRequirement,
                plantType = defaultPlantType
            });
        }

        public void CreateMarketContract(ContractDescriptor contract)
        {
            var newContract = Instantiate(contractOfferPrefab, marketModalContractsParent.transform);
            newContract.targets = contract.targets;
            newContract.rewardAmount = contract.reward;
            newContract.seedRequirement = contract.seedRequirement;
            newContract.plantType = contract.plantType;
        }
        public ContractContainer CreateClaimedContract(ContractDescriptor contract)
        {
            var newContract = Instantiate(claimedContractPrefab, claimedContractsModalParent.transform);
            newContract.targets = contract.targets;
            newContract.rewardAmount = contract.reward;
            newContract.seedRequirement = contract.seedRequirement;
            newContract.plantType = contract.plantType;

            return newContract;
        }
        public void ShowClaimedContractsModal()
        {
            onModalOpened.TriggerEvent();
            claimedContractModal.SetActive(true);
        }
        public int ClaimedContractsCount()
        {
            return claimedContractsModalParent.transform.childCount;
        }

        public void ClaimContract(ContractContainer marketContract)
        {
            if (marketContract.transform.parent != marketModalContractsParent.transform)
            {
                throw new System.Exception("contract must be in the market");
            }
            var contractDescriptor = new ContractDescriptor
            {
                reward = marketContract.rewardAmount,
                targets = marketContract.targets,
                seedRequirement = marketContract.seedRequirement,
                plantType = marketContract.plantType
            };
            Destroy(marketContract.gameObject);
            CreateClaimedContract(contractDescriptor);
        }
    }
}