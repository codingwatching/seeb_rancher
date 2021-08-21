using Dman.SceneSaveSystem;
using Genetics.ParameterizedGenomeGenerator;
using System.Linq;
using TMPro;
using UI.SeedInventory;
using UnityEngine;

namespace UI.Markets
{
    public class SeebBinContainer : MonoBehaviour, ISaveableData
    {
        public SeebStoreBinDescriptor binDescriptor;
        public SeedInventoryDropSlot purchaseSpot;

        public TMP_Text plantNameText;
        public string seedNumberFormat = "# seebs";
        public TMP_Text seedNumberText;
        public TMP_Text priceText;
        public TMP_Text targetGeneticsDescriptorText;

        // Start is called before the first frame update
        void Start()
        {
            BinAndSeebSlotStateUpdated();
        }

        public void BinAndSeebSlotStateUpdated()
        {
            Debug.Log("seeb slot updated");
            if (binDescriptor.seedCount <= 0 && binDescriptor.seedCount != -1 && purchaseSpot.dataModel.bucket.Empty)
            {
                Destroy(gameObject);
            }
            else
            {
                ReRender();
            }
        }

        private void ReRender()
        {
            plantNameText.text = binDescriptor.plantType.plantName;
            var seedCountText = binDescriptor.seedCount >= 0 ? binDescriptor.seedCount.ToString() : "∞";
            seedNumberText.text = seedNumberFormat
                .Replace("#", seedCountText);
            priceText.text = $"${binDescriptor.price:F2}";
            targetGeneticsDescriptorText.text = string.Join(", ",
                binDescriptor.booleanTargets.Cast<IGeneticTarget>()
                    .Concat(binDescriptor.floatTargets)
                    .Select(target => target.GetDescriptionOfTarget())
                );
        }

        [System.Serializable]
        class SeebBinContainerSaveObject
        {
            SeebStoreBinDescriptor contract;
            public SeebBinContainerSaveObject(SeebBinContainer source)
            {
                contract = source.binDescriptor;
            }
            public void ApplyTo(SeebBinContainer target)
            {
                target.binDescriptor = contract;
            }
        }

        public string UniqueSaveIdentifier => "SeebBin";

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return new SeebBinContainerSaveObject(this);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SeebBinContainerSaveObject contract)
            {
                contract.ApplyTo(this);
                ReRender();
            }
        }
    }
}