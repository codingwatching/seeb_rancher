using Dman.ReactiveVariables;
using UI.Manipulators;
using UniRx;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "WaitForManipulatorChangePrompt", menuName = "Narrative/Prompts/WaitForManipulatorChangePrompt", order = 1)]
    public class WaitForManipulatorChangePrompt : Prompt
    {
        public ScriptableObjectVariable manipulatorVariable;
        public MapManipulator manipulator;

        public override void OpenPrompt(Conversation conversation)
        {
            OpenPromptWithSetup();

            manipulatorVariable.Value
                .TakeUntilDestroy(currentPrompt.gameObject)
                .Subscribe(nextValue =>
                {
                    if (nextValue != manipulator)
                    {
                        conversation.PromptClosed();
                        Destroy(currentPrompt.gameObject);
                    }
                }).AddTo(currentPrompt.gameObject);
        }
    }
}
