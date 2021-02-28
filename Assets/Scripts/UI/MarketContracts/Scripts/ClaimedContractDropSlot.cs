using Assets.Scripts.DataModels;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ClaimedContractDropSlot : MonoBehaviour
    {
        public ContractContainer contractContainer;
        public ScriptableObjectVariable activeManipulator;

        public void SeedSlotClicked()
        {
            if (ContractEvaluationController.Instance.IsEvaluating || contractContainer.contract.seedRequirement <= 0)
            {
                return;
            }
            if (activeManipulator.CurrentValue is ISeedHoldingManipulator seedHolder)
            {
                var seeds = seedHolder.AttemptTakeSeeds(contractContainer.contract.seedRequirement);
                if (seeds == null || contractContainer.contract.plantType.myId != seeds[0].plantType)
                {
                    return;
                }
                EvaluateContract(seeds);
            }
        }

        private void EvaluateContract(Seed[] seebs)
        {
            var descriptor = contractContainer.contract;
            Destroy(contractContainer.gameObject);

            ContractEvaluationController.Instance.InitiateEvaluation(seebs, descriptor);
        }
    }
}