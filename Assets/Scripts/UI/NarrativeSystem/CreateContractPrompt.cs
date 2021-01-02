﻿using Assets.Scripts.UI.MarketContracts;
using Assets.Scripts.Utilities.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "CreateContractPrompt", menuName = "Narrative/CreateContractPrompt", order = 1)]
    public class CreateContractPrompt : Prompt
    {
        public PromptController promptPrefab;
        [Multiline]
        public string promptText;
        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public ContractDescriptor contractToCreate;
        public GameObjectVariable variableToPutContractIn;

        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();
            var createdContract = MarketManager.Instance.CreateClaimedContract(contractToCreate);
            createdContract.StartCoroutine(HighlightContract(createdContract.gameObject));
            //variableToPutContractIn.SetValue(createdContract.gameObject);

            var newPrompt = Instantiate(promptPrefab, PromptParentSingleton.Instance.transform);
            newPrompt.Opened(promptText, () =>
            {
                variableToPutContractIn.SetValue(null);
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(newPrompt.gameObject);
            });
        }

        IEnumerator HighlightContract(GameObject toBeHighlighted)
        {
            yield return new WaitForEndOfFrame();
            variableToPutContractIn.SetValue(toBeHighlighted);
        }
    }
}
