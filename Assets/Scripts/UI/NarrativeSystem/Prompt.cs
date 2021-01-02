using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    public abstract class Prompt : ScriptableObject
    {
        public abstract void OpenPrompt(Conversation conversation);
    }
}
