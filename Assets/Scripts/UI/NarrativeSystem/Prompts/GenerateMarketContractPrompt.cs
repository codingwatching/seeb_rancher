using Assets.Scripts.UI.MarketContracts;
using Assets.Scripts.Utilities.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "GenerateMarketContractPrompt", menuName = "Narrative/Prompts/GenerateMarketContractPrompt", order = 1)]
    public class GenerateMarketContractPrompt : Prompt
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;
        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();
            MarketManager.Instance.TriggerNewContractGeneration();

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
