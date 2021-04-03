using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Dman.SceneSaveSystem;
using Genetics.ParameterizedGenomeGenerator;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class SeebBinContainer : MonoBehaviour, ISaveableData
    {
        public SeebStoreBinDescriptor binDescriptor;

        public TMP_Text plantNameText;
        public string seedNumberFormat = "# seeds";
        public TMP_Text seedNumberText;
        public TMP_Text priceText;
        public TMP_Text targetGeneticsDescriptorText;

        // Start is called before the first frame update
        void Start()
        {
            ReRender();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void ReRender()
        {
            plantNameText.text = binDescriptor.plantType.plantName;
            seedNumberText.text = seedNumberFormat
                .Replace("#", binDescriptor.seedCount.ToString());
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