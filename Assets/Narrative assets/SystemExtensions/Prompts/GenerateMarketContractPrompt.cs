using Assets.Scripts.UI.MarketContracts;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.NarrativeSystem
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

            OpenPromptWithSetup(() =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(currentPrompt.gameObject);
            });
        }
    }
}
