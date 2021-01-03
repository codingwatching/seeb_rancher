using Assets.Scripts.UI.MarketContracts;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem.ConversationTriggers
{
    [CreateAssetMenu(fileName = "ClaimedContractsClearedTrigger", menuName = "Narrative/Triggers/ClaimedContractsClearedTrigger", order = 1)]
    public class ClaimedContractsClearedTrigger : ConversationTrigger
    {
        public override bool ShouldTrigger(GameNarrative narrative)
        {
            return MarketManager.Instance.ClaimedContractsCount() <= 0;
        }
    }
}