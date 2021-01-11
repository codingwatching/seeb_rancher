using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "ForceHighlightOpenSeedSlot", menuName = "Narrative/Prompts/ForceHighlightOpenSeedSlot", order = 1)]
    public class ForceHighlightFilledSeedSlot : Prompt
    {

        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        public GameObjectVariable highlightedGameObject;

        private Conversation sourceConversation;
        private SeedInventoryDropSlot filledSlot;

        public override void OpenPrompt(Conversation conversation)
        {
            sourceConversation = conversation;
            onOpened?.Invoke();

            var highlightedObj = TryHighlightDropSlot();

            OpenPromptWithSetup(() =>
            {
                if (highlightedObj == null && sourceConversation != null)
                {
                    onCompleted?.Invoke();
                    sourceConversation.PromptClosed();
                    Destroy(currentPrompt.gameObject);
                }
            });
        }

        private GameObject TryHighlightDropSlot()
        {
            filledSlot = GameObject
                .FindObjectsOfType<SeedInventoryDropSlot>()
                .Where(slot => !slot.dataModel.bucket.Empty)
                .FirstOrDefault();
            if (filledSlot != null)
            {
                highlightedGameObject.SetValue(filledSlot.gameObject);
                filledSlot.DropSlotButton.onClick.AddListener(DropSlotClicked);
                return filledSlot.gameObject;
            }
            return null;
        }

        private void DropSlotClicked()
        {
            if (sourceConversation == null)
            {
                return;
            }
            filledSlot.DropSlotButton.onClick.RemoveListener(DropSlotClicked);
            onCompleted?.Invoke();
            sourceConversation.PromptClosed();
            Destroy(currentPrompt.gameObject);
            highlightedGameObject.SetValue(null);

            sourceConversation = null;
            currentPrompt = null;
            filledSlot = null;
        }
    }
}
