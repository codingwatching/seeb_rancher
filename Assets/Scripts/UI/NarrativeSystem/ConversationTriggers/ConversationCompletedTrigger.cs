using Assets.Scripts.UI.MarketContracts;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem.ConversationTriggers
{
    [CreateAssetMenu(fileName = "ConversationCompletedTrigger", menuName = "Narrative/Triggers/ConversationCompletedTrigger", order = 1)]
    public class ConversationCompletedTrigger : ConversationTrigger
    {
        public Conversation[] conversationsToBeCompleted;
        public override bool ShouldTrigger(GameNarrative narrative)
        {
            return conversationsToBeCompleted.All(convo => narrative.completedConversations.Contains(convo.myId));
        }
    }
}