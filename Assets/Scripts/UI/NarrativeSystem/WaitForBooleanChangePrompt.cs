using Assets.Scripts.UI.MarketContracts;
using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "WaitForBooleanChangePrompt", menuName = "Narrative/WaitForBooleanChangePrompt", order = 1)]
    public class WaitForBooleanChangePrompt : Prompt
    {
        public PromptController promptPrefab;
        public BooleanVariable highlightObjectVariable;
        [Multiline]
        public string promptText;

        public override void OpenPrompt(Conversation conversation)
        {
            var newPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            highlightObjectVariable.SetValue(true);
            highlightObjectVariable.Value
                .Where(x => !x)
                .Take(1)
                .Subscribe(nextval =>
                {
                    conversation.PromptClosed();
                    Destroy(newPrompt.gameObject);
                });
            newPrompt.Opened(promptText, () =>
            {
            });
        }
    }
}
