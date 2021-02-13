using Assets.Scripts.DataModels;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class SeedInventoryDropSlot : MonoBehaviour
    {
        [Tooltip("Called when seeds are pulled out of a slot to be moved around")]
        public UnityEvent onSeedFirstGrabbed;

        public Button DropSlotButton;
        public TMP_InputField labelInputField;

        public DragSeedsManipulator draggingSeedsManipulator;
        public ScriptableObjectVariable activeManipulator;

        public SeedBucketUI dataModel { get; private set; }

        private SeedBucketDisplay Displayer => GetComponent<SeedBucketDisplay>();

        /// <summary>
        /// This method will be kind of messy because it keys off of a button instead of the <see cref="Manipulators.Scripts.ManipulatorController"/> system
        /// </summary>
        public void SeedSlotClicked()
        {
            if(activeManipulator.CurrentValue is ISeedHoldingManipulator seedHolder)
            {
                seedHolder.AttemptTransferAllSeedsInto(dataModel.bucket);
            }else
            {
                if (dataModel.bucket.Empty)
                {
                    return;
                }
                // if there are no dragging seeds, pull out of the slot and start dragging seeds
                activeManipulator.SetValue(draggingSeedsManipulator);
                draggingSeedsManipulator.InitializeSeedBucketFrom(dataModel.bucket);
                onSeedFirstGrabbed?.Invoke();
            }
            Displayer.DisplaySeedBucket(dataModel.bucket);
        }

        public void SetDataModelLink(SeedBucketUI dataModel)
        {
            this.dataModel = dataModel;
            Displayer.DisplaySeedBucket(dataModel.bucket);
            labelInputField.text = dataModel.description;
            labelInputField.onDeselect.AddListener(newValue =>
            {
                if (isResetting)
                {
                    return;
                }
                Debug.Log(newValue);
                dataModel.description = newValue;
                StartCoroutine(ResetTextField());
                //resetText = true;
            });
        }
        private bool isResetting = false;
        /// <summary>
        /// hack used to force the text label to display the first section of the text, in case
        ///     the text has scrolled horizontally due to editing
        /// </summary>
        /// <returns></returns>
        IEnumerator ResetTextField()
        {
            isResetting = true;
            labelInputField.onFocusSelectAll = false;
            labelInputField.ActivateInputField();
            labelInputField.MoveToStartOfLine(false, false);
            yield return new WaitForEndOfFrame();
            labelInputField.DeactivateInputField();
            labelInputField.onFocusSelectAll = true;
            yield return new WaitForEndOfFrame();
            isResetting = false;
        }
    }
}