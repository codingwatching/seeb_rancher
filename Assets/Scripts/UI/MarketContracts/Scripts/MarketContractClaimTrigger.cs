using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    [RequireComponent(typeof(ContractContainer))]
    public class MarketContractClaimTrigger : MonoBehaviour
    {
        public void ClaimThisContract()
        {
            MarketManager.Instance.ClaimContract(GetComponent<ContractContainer>());
        }
    }
}