using Assets.Scripts.UI.NarrativeSystem.ConversationTriggers;
using Dman.ObjectSets;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "Conversation", menuName = "Narrative/Conversation", order = 1)]
    public class Conversation : IDableObject
    {
        public Prompt[] prompts;
        public int currentPromptIndex;
        public ConversationTrigger[] triggerAll;

        private GameNarrative narrative;



        public bool ShouldStartConversation(GameNarrative narrative)
        {
            return triggerAll.All(x => x.ShouldTrigger(narrative));
        }

        public void StartConversation(GameNarrative narrative)
        {
            currentPromptIndex = 0;
            prompts[0].OpenPrompt(this);
            this.narrative = narrative;
        }

        public void PromptClosed()
        {
            currentPromptIndex++;
            if (currentPromptIndex >= prompts.Length)
            {
                narrative.ConversationEnded(this);
            }
            else
            {
                prompts[currentPromptIndex].OpenPrompt(this);
            }
        }
    }
}
