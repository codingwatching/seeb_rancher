using Assets.Scripts.Plants;
using Dman.ObjectSets;
using Dman.SceneSaveSystem;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractContainer : MonoBehaviour, ISaveableData
    {
        public BasePlantType plantType;
        public BooleanGeneticTarget[] targets;
        public float rewardAmount;
        public int seedRequirement;

        public TMP_Text plantNameText;
        public string seedNumberFormat = "# seeds";
        public TMP_Text seedNumberText;
        public TMP_Text rewardText;
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
            plantNameText.text = plantType.plantName;
            seedNumberText.text = seedNumberFormat.Replace("#", seedRequirement.ToString());
            rewardText.text = $"${rewardAmount:F2}";
            targetGeneticsDescriptorText.text = string.Join(", ", targets.Select(target => target.GetDescriptionOfTarget()));
        }

        [System.Serializable]
        class ContractSaveObject
        {
            BooleanGeneticTarget[] targets;
            float rewardAmount;
            int seedRequirement;
            int plantTypeID;
            public ContractSaveObject(ContractContainer source)
            {
                targets = source.targets;
                rewardAmount = source.rewardAmount;
                seedRequirement = source.seedRequirement;
                plantTypeID = source.plantType.myId;
            }
            public void ApplyTo(ContractContainer target)
            {
                target.targets = targets;
                target.rewardAmount = rewardAmount;
                target.seedRequirement = seedRequirement;
                var plantTypeRegistry = RegistryRegistry.GetObjectRegistry<BasePlantType>();
                target.plantType = plantTypeRegistry.GetUniqueObjectFromID(plantTypeID);
            }
        }

        public string UniqueSaveIdentifier => "SeebContract";

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return new ContractSaveObject(this);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is ContractSaveObject contract)
            {
                contract.ApplyTo(this);
                ReRender();
            }
        }
    }
}