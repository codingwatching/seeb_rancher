using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem.ConversationTriggers
{
    public abstract class ConversationTrigger : ScriptableObject
    {
        public abstract bool ShouldTrigger(GameNarrative narrative);
    }
}