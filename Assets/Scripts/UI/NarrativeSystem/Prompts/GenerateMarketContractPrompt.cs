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
        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();
            MarketManager.Instance.TriggerNewContractGeneration();
            
            this.OpenPromptWithSetup(() =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(currentPrompt.gameObject);
            });
        }
    }
}
