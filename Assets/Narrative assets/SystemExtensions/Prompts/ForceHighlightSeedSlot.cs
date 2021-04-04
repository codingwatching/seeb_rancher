using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "ForceHighlightSeedSlot", menuName = "Narrative/Prompts/ForceHighlightSeedSlot", order = 1)]
    public class ForceHighlightSeedSlot : Prompt
    {

        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        [Tooltip("When true, will highlight a filled slot. When false, will highlight an empty slot.")]
        public bool selectFilled = true;

        public GameObjectVariable highlightedGameObject;

        private Conversation sourceConversation;
        private SeedInventoryDropSlot targetSlot;

        public override void OpenPrompt(Conversation conversation)
        {
            sourceConversation = conversation;
            onOpened?.Invoke();

            var highlightedObj = TryHighlightDropSlot();

            OpenPromptWithSetup();
        }

        private GameObject TryHighlightDropSlot()
        {
            targetSlot = GameObject
                .FindObjectsOfType<SeedInventoryDropSlot>()
                .Where(slot => selectFilled && !slot.dataModel.bucket.Empty || !selectFilled && slot.dataModel.bucket.Empty)
                .FirstOrDefault();
            if (targetSlot != null)
            {
                highlightedGameObject.SetValue(targetSlot.gameObject);
                targetSlot.DropSlotButton.onClick.AddListener(DropSlotClicked);
                return targetSlot.gameObject;
            }
            return null;
        }

        private void DropSlotClicked()
        {
            if (sourceConversation == null)
            {
                return;
            }
            targetSlot.DropSlotButton.onClick.RemoveListener(DropSlotClicked);
            onCompleted?.Invoke();
            sourceConversation.PromptClosed();
            Destroy(currentPrompt.gameObject);
            highlightedGameObject.SetValue(null);

            sourceConversation = null;
            currentPrompt = null;
            targetSlot = null;
        }
    }
}
