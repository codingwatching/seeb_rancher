using Assets.Scripts.Plants;
using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using Genetics.GeneticDrivers;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{

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

        public BooleanGeneticDriver[] booleanTargetGenerators;
        public FloatGeneticTargetGenerator[] floatTargetGenerators;
        public SeedCountTargetRandomGenerator seedTargetGenerator;
        [Range(0, 1)]
        public float chanceForNewContractPerPhase;
        public int defaultSeedCountRequirement = 5;
        public BasePlantType defaultPlantType;

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
            var targetBuckets = new int[] { booleanTargetGenerators.Length, floatTargetGenerators.Length, seedTargetGenerator == null ? 0 : 1 };
            var totalPossibleTargets = targetBuckets.Sum();
            // varies from 1 to targetSetLength, weighted towards lower numbers
            var numberOfTargets = Mathf.FloorToInt(Mathf.Pow(rangeSample, 2) * totalPossibleTargets) + 1;

            SubtractFromBuckets(totalPossibleTargets - numberOfTargets, targetBuckets);

            var contract = new TargetContractDescriptor
            {
                booleanTargets = GenerateBooleanTargets(targetBuckets[0]),
                floatTargets = GenerateFloatTargets(targetBuckets[1]),
                seedCountTarget = GenerateSeedTargets(targetBuckets[2]),
                seedRequirement = defaultSeedCountRequirement,
                plantType = defaultPlantType
            };
            var totalTargets = (contract.booleanTargets?.Length ?? 0) + (contract.floatTargets?.Length ?? 0) + (contract.seedCountTarget?.Length ?? 0);
            if (totalTargets != numberOfTargets)
            {
                Debug.LogError($"Something has gone very wrong. The number of targets does not match. Expected {numberOfTargets} but actually got {totalTargets}");
            }
            contract.reward = defaultReward * Mathf.Pow(multiplierPerAdditional, totalTargets);
            CreateMarketContract(contract);
        }

        /// <summary>
        /// Subtract <paramref name="amount"/> from elements in <paramref name="buckets"/> such that buckets.Sum() - amount before this method equals buckets.Sum() after the method.
        ///     The chance each bucket will be decreased by each successive unit of <paramref name="amount"/> is proportional to the current size of the bucket.
        ///     This can be though of as defining a list of bit flags buckets.Sum() in length, and then randomly flipping any of the currently On flags into the off position,
        ///         as many times as <paramref name="amount"/>
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="buckets"></param>
        private void SubtractFromBuckets(int amount, int[] buckets)
        {
            while (amount > 0)
            {
                var totalPossibleTargets = buckets.Sum();
                var nextTarget = Random.Range(0, totalPossibleTargets);
                var currentTargetIndex = 0;
                for (int i = 0; i < buckets.Length; i++)
                {
                    currentTargetIndex += buckets[i];
                    if (nextTarget < currentTargetIndex)
                    {
                        buckets[i] -= 1;
                        amount--;
                        break;
                    }
                }
            }

        }

        private BooleanGeneticTarget[] GenerateBooleanTargets(int numberOfTargets)
        {
            var targetIndexes = ArrayExtensions.SelectIndexSources(numberOfTargets, booleanTargetGenerators.Length);
            var chosenDrivers = targetIndexes.Select(index => booleanTargetGenerators[index]);

            return chosenDrivers
                .Select(x => new BooleanGeneticTarget(x))
                .ToArray();
        }

        private FloatGeneticTarget[] GenerateFloatTargets(int numberOfTargets)
        {
            var targetIndexes = ArrayExtensions.SelectIndexSources(numberOfTargets, floatTargetGenerators.Length);
            return targetIndexes.Select(index => floatTargetGenerators[index].GenerateTarget()).ToArray();
        }
        private SeedCountTarget[] GenerateSeedTargets(int numberOfTargets)
        {
            if (numberOfTargets <= 0)
            {
                return new SeedCountTarget[0];
            }
            if (numberOfTargets > 1)
            {
                Debug.LogError($"Cannot have more than one seed target. tried to create with {numberOfTargets}");
            }
            return new SeedCountTarget[] { seedTargetGenerator.GenerateTarget() };
        }

        public void CreateMarketContract(TargetContractDescriptor contract)
        {
            var newContract = Instantiate(contractOfferPrefab, marketModalContractsParent.transform);
            newContract.contract = contract;
        }
        public ContractContainer CreateClaimedContract(TargetContractDescriptor contract)
        {
            var newContract = Instantiate(claimedContractPrefab, claimedContractsModalParent.transform);
            newContract.contract = contract;

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
            var contractDescriptor = marketContract.contract;
            Destroy(marketContract.gameObject);
            CreateClaimedContract(contractDescriptor);
        }
    }
}