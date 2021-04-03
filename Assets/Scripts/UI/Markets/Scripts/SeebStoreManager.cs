using Assets.Scripts.Plants;
using Assets.Scripts.UI.MarketContracts.ChildCycler;
using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using Genetics.GeneticDrivers;
using Genetics.ParameterizedGenomeGenerator;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{

    public class SeebStoreManager : MonoBehaviour, IChildBuilder
    {
        [Header("store prefabs")]
        public GameObject seebOfferBinPrefab;
        public EventGroup onModalOpened;

        [Header("Contract generation parameters")]
        public float defaultPrice;
        [Tooltip("For every extra genetic driver over 1, multiply the reward by this amount. 3 genetic drivers will be defaultReward * multiplierPerAdditional^2")]
        public float multiplierPerAdditional;

        public BooleanGeneticDriver[] booleanTargetGenerators;
        public FloatGeneticTargetGenerator[] floatTargetGenerators;
        public BasePlantType seebType;

        public static SeebStoreManager Instance;

        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public GameObject InstantiateUnderParent(GameObject parent)
        {
            return this.GenerateNewContract(parent);
        }
        private GameObject GenerateNewContract(GameObject parent)
        {
            var rangeSample = Random.Range(0f, 1f - 1e-5f);
            var targetBuckets = new int[] { booleanTargetGenerators.Length, floatTargetGenerators.Length };
            var totalPossibleTargets = targetBuckets.Sum();
            // varies from 1 to targetSetLength, weighted towards lower numbers
            var numberOfTargets = Mathf.FloorToInt(Mathf.Pow(rangeSample, 2) * totalPossibleTargets) + 1;

            SubtractFromBuckets(totalPossibleTargets - numberOfTargets, targetBuckets);

            var contract = new TargetContractDescriptor
            {
                booleanTargets = GenerateBooleanTargets(targetBuckets[0]),
                floatTargets = GenerateFloatTargets(targetBuckets[1]),
                plantType = seebType
            };
            var totalTargets = (contract.booleanTargets?.Length ?? 0) + (contract.floatTargets?.Length ?? 0) + (contract.seedCountTarget?.Length ?? 0);
            if (totalTargets != numberOfTargets)
            {
                Debug.LogError($"Something has gone very wrong. The number of targets does not match. Expected {numberOfTargets} but actually got {totalTargets}");
            }

            contract.minimumComplianceRatio = -1;

            contract.reward =
                defaultPrice
                * Mathf.Pow(multiplierPerAdditional, totalTargets);
            return CreateMarketContract(contract, parent);
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

        public GameObject CreateMarketContract(TargetContractDescriptor contract, GameObject parent)
        {
            var newContract = Instantiate(seebOfferBinPrefab, parent.transform);
            // TODO: newContract.contract = contract;
            return newContract;
        }
    }
}