using System;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    public abstract class Prompt : ScriptableObject
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;
        public Sprite speakerSprite;

        protected PromptController currentPrompt;

        protected void OpenPromptWithSetup(Action onClosed = null)
        {
            currentPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            currentPrompt.Opened(promptText, speakerSprite, onClosed);
        }

        public abstract void OpenPrompt(Conversation conversation);
    }
}
