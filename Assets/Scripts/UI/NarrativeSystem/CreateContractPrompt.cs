using Assets.Scripts.UI.MarketContracts;
using Assets.Scripts.Utilities.Core;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "CreateContractPrompt", menuName = "Narrative/CreateContractPrompt", order = 1)]
    public class CreateContractPrompt : Prompt
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;
        public ContractDescriptor contractToCreate;
        public GameObjectVariable variableToPutContractIn;

        public override void OpenPrompt(Conversation conversation)
        {
            var createdContract = MarketManager.Instance.CreateClaimedContract(contractToCreate);
            variableToPutContractIn.SetValue(createdContract.gameObject);

            var newPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            newPrompt.Opened(promptText, () =>
            {
                conversation.PromptClosed();
                Destroy(newPrompt.gameObject);
            });
        }
    }
}
