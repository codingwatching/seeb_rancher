using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "DebugPrompt", menuName = "Narrative/DebugPrompt", order = 1)]
    public class DebugPrompt : Prompt
    {
        public PromptController promptPrefab;
        public string promptText;

        public override void OpenPrompt(Conversation conversation)
        {
            var newPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            newPrompt.Opened(promptText, () =>
            {
                conversation.PromptClosed();
                Destroy(newPrompt.gameObject);
            });
        }
    }
}
