using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "DebugPrompt", menuName = "Narrative/Prompts/DebugPrompt", order = 1)]
    public class DebugPrompt : Prompt
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;

        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();
            var newPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            newPrompt.Opened(promptText, () =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(newPrompt.gameObject);
            });
        }
    }
}
