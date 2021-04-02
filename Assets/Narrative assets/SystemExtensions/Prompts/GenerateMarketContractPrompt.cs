using Assets.Scripts.UI.MarketContracts;
using Assets.Scripts.UI.MarketContracts.ChildCycler;
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
            MarketManager.Instance.GetComponent<PhaseBasedChildCyclingManager>().TriggerNewChild();

            OpenPromptWithSetup(() =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(currentPrompt.gameObject);
            });
        }
    }
}
