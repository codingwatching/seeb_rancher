using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "DebugPrompt", menuName = "Narrative/Prompts/DebugPrompt", order = 1)]
    public class DebugPrompt : Prompt
    {
        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();
            this.OpenPromptWithSetup(() =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(currentPrompt.gameObject);
            });
        }
    }
}
