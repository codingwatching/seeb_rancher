using Assets.Scripts.DataModels;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ClaimedContractDropSlot : MonoBehaviour
    {
        public ContractContainer contract;
        public ScriptableObjectVariable activeManipulator;

        public void SeedSlotClicked()
        {
            if (ContractEvaluationController.Instance.IsEvaluating || contract.seedRequirement <= 0)
            {
                return;
            }
            if (activeManipulator.CurrentValue is ISeedHoldingManipulator seedHolder)
            {
                var seeds = seedHolder.AttemptTakeSeeds(contract.seedRequirement);
                if (seeds == null || contract.plantType.myId != seeds[0].plantType)
                {
                    return;
                }
                EvaluateContract(seeds);
            }
        }

        private void EvaluateContract(Seed[] seebs)
        {
            var descriptor = new ContractDescriptor
            {
                plantType = contract.plantType,
                reward = contract.rewardAmount,
                seedRequirement = contract.seedRequirement,
                targets = contract.targets
            };
            Destroy(contract.gameObject);

            ContractEvaluationController.Instance.InitiateEvaluation(seebs, descriptor);
        }
    }
}