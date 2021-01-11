using UnityEngine;

namespace Dman.NarrativeSystem.ConversationTriggers
{
    public abstract class ConversationTrigger : ScriptableObject
    {
        public abstract bool ShouldTrigger(GameNarrative narrative);
    }
}