using System;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    public abstract class Prompt : ScriptableObject
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;
        public Sprite speakerSprite;

        protected PromptController currentPrompt;

        protected void OpenPromptWithSetup(Action onClosed)
        {
            currentPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            currentPrompt.Opened(promptText, speakerSprite, onClosed);
        }

        public abstract void OpenPrompt(Conversation conversation);
    }
}
