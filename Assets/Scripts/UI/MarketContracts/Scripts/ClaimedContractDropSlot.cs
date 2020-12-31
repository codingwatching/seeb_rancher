using Assets.Scripts.DataModels;
using Assets.Scripts.UI.SeedInventory;
using Assets.Scripts.Utilities.Core;
using UnityEngine;
using UnityEngine.UI;

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
            EvaluateContract(seebs);
        }

        public void EvaluateContract(SeedBucket seebs)
        {
        }

        public void ClaimThisContract()
        {
            MarketManager.Instance.ClaimContract(GetComponent<ContractContainer>());
        }
    }
}