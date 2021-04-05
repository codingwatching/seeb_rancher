using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    [RequireComponent(typeof(ContractContainer))]
    public class MarketContractClaimTrigger : MonoBehaviour
    {
        public Animator buttonAnimator;
        public string cannotClaimAnimationTrigger;

        public void ClaimThisContract()
        {
            var canClaim = MarketManager.Instance.CanClaimContract();
            if (canClaim)
            {
                MarketManager.Instance.ClaimContract(GetComponent<ContractContainer>());
            }
            else
            {
                buttonAnimator.SetTrigger(cannotClaimAnimationTrigger);
            }
        }
    }
}