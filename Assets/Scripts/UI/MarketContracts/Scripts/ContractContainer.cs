using Assets.Scripts.UI.MarketContracts.EvaluationTargets;
using Dman.SceneSaveSystem;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class ContractContainer : MonoBehaviour, ISaveableData
    {
        public TargetContractDescriptor contract;

        public TMP_Text plantNameText;
        public string seedNumberFormat = "Minimum % compliance Submit # seeds";
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
            plantNameText.text = contract.plantType.plantName;
            seedNumberText.text = seedNumberFormat
                .Replace("#", contract.seedRequirement.ToString())
                .Replace("%", (contract.minimumComplianceRatio * 100).ToString("F0") + "%");
            rewardText.text = $"${contract.reward:F2}";
            targetGeneticsDescriptorText.text = string.Join(", ",
                contract.booleanTargets.Cast<IContractTarget>()
                    .Concat(contract.floatTargets)
                    .Concat(contract.seedCountTarget)
                    .Select(target => target.GetDescriptionOfTarget())
                );
        }

        [System.Serializable]
        class ContractSaveObject
        {
            TargetContractDescriptor contract;
            public ContractSaveObject(ContractContainer source)
            {
                contract = source.contract;
            }
            public void ApplyTo(ContractContainer target)
            {
                target.contract = contract;
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