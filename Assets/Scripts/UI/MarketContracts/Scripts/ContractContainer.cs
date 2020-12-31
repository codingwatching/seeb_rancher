using Assets.Scripts.Utilities.SaveSystem.Components;
using Genetics;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractContainer : MonoBehaviour, ISaveableData
    {
        public BooleanGeneticTarget[] targets;
        public float rewardAmount;

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
            rewardText.text = $"${rewardAmount:F2}";
            targetGeneticsDescriptorText.text = string.Join(", ", targets.Select(target => target.GetDescriptionOfTarget()));
        }

        [System.Serializable]
        class ContractSaveObject
        {
            BooleanGeneticTarget[] targets;
            float rewardAmount;
            public ContractSaveObject(ContractContainer source)
            {
                targets = source.targets;
                rewardAmount = source.rewardAmount;
            }
            public void ApplyTo(ContractContainer target)
            {
                target.targets = targets;
                target.rewardAmount = rewardAmount;
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