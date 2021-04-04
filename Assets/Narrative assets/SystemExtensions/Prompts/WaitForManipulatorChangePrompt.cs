using Assets.Scripts.Plants;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityFx.Outline;

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
