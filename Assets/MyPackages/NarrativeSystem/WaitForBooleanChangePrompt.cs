using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "WaitForBooleanChangePrompt", menuName = "Narrative/WaitForBooleanChangePrompt", order = 1)]
    public class WaitForBooleanChangePrompt : Prompt
    {
        public BooleanVariable highlightObjectVariable;

        public override void OpenPrompt(Conversation conversation)
        {
            highlightObjectVariable.SetValue(true);
            highlightObjectVariable.Value
                .Where(x => !x)
                .Take(1)
                .Subscribe(nextval =>
                {
                    conversation.PromptClosed();
                    Destroy(currentPrompt.gameObject);
                });
            OpenPromptWithSetup(() =>
            {
            });
        }
    }
}
