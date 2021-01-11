using Assets.Scripts.DataModels;
using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ClaimedContractDropSlot : MonoBehaviour
    {
        public GameObjectVariable draggingSeedSet;
        public ContractContainer contract;

        public void SeedSlotClicked()
        {
            var dragginSeeds = draggingSeedSet.CurrentValue?.GetComponent<DraggingSeeds>();
            if (dragginSeeds == null)
            {
                return;
            }
            var seebs = dragginSeeds.myBucket;
            if (ContractEvaluationController.Instance.IsEvaluating)
            {
                return;
            }
            if (contract.plantType.myId != seebs.AllSeeds[0].plantType)
            {
                return;
            }
            if (seebs.AllSeeds.Length < contract.seedRequirement)
            {
                return;
            }
            var mySeeds = seebs.TakeN(contract.seedRequirement);
            dragginSeeds.SeedBucketUpdated();
            EvaluateContract(mySeeds);
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