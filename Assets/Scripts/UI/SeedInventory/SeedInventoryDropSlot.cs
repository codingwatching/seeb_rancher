using Assets.Scripts.DataModels;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class SeedInventoryDropSlot : MonoBehaviour, ISaveableData
    {
        [Tooltip("Called when seeds are pulled out of a slot to be moved around")]
        public UnityEvent onSeedFirstGrabbed;

        public Button DropSlotButton;
        public TMP_InputField labelInputField;

        public DragSeedsManipulator draggingSeedsManipulator;
        public ScriptableObjectVariable activeManipulator;

        public SeedBucketUI dataModel { get; protected set; }

        protected SeedBucketDisplay Displayer => GetComponent<SeedBucketDisplay>();

        private void Awake()
        {
            if (dataModel == null)
            {
                UpdateDataModel(new SeedBucketUI());
            }
            InitializeListeners();
        }

        public virtual bool CanSlotRecieveNewStack()
        {
            return string.IsNullOrWhiteSpace(dataModel.description) && dataModel.bucket.Empty;
        }

        /// <summary>
        /// This method will be kind of messy because it keys off of a button instead of the <see cref="Manipulators.Scripts.ManipulatorController"/> system
        /// </summary>
        public void SeedSlotClicked()
        {
            if (activeManipulator.CurrentValue is ISeedHoldingManipulator seedHolder)
            {
                AddSeedsFromManipulator(seedHolder);
            }
            else
            {
                GrabSeedsOut();
            }
            MySeedsUpdated();
        }

        /// <summary>
        /// Grab the seeds out from this slot into a new dragging cursor
        /// </summary>
        protected void GrabSeedsOut()
        {
            if (dataModel.bucket.Empty)
            {
                return;
            }
            // if there are no dragging seeds, pull out of the slot and start dragging seeds
            activeManipulator.SetValue(draggingSeedsManipulator);
            draggingSeedsManipulator.InitializeSeedBucketFrom(this);
            onSeedFirstGrabbed?.Invoke();
        }

        /// <summary>
        /// Takes seeds from the manipulator and add it to this slot, if possible
        /// </summary>
        protected virtual void AddSeedsFromManipulator(ISeedHoldingManipulator seedHolder)
        {
            var emptyPreTransfer = dataModel?.bucket.Empty ?? true;
            var seedsTransferred = seedHolder.AttemptTransferAllSeedsInto(dataModel.bucket);
            if (emptyPreTransfer && seedsTransferred)
            {
                dataModel.description = seedHolder.SeedGroupName;
            }
        }

        public virtual bool RecieveNewSeeds(SeedBucketUI model)
        {
            if (!CanSlotRecieveNewStack())
            {
                return false;
            }
            this.UpdateDataModel(model);
            return true;
        }


        public void MySeedsUpdated()
        {
            Displayer.DisplaySeedBucket(dataModel.bucket);
            labelInputField.text = dataModel.description;
        }

        public void UpdateDataModel(SeedBucketUI model)
        {
            dataModel = model;
            MySeedsUpdated();
        }

        public void InitializeListeners()
        {
            labelInputField.onDeselect.AddListener(newValue =>
            {
                if (isResetting)
                {
                    return;
                }
                if (dataModel != null)
                {
                    dataModel.description = newValue;
                }
                StartCoroutine(ResetTextField());
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

        #region save data
        public string UniqueSaveIdentifier => "SeedSlot";

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return dataModel;
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SeedBucketUI data)
            {
                UpdateDataModel(data);
            }
            else
            {
                UpdateDataModel(new SeedBucketUI());
            }
        }
        #endregion
    }
}