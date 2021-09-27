using Dman.ReactiveVariables;
using Genetics.GeneticDrivers;
using Simulation.Plants;
using System.Text;
using TMPro;
using UI.Manipulators;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlantData
{
    public class PlantData : MonoBehaviour
    {
        public GameObjectVariable selectedPlant;
        public ScriptableObjectVariable manipulatorVariable;
        public PollinatePlantManipulator pollinator;

        public TMP_Text plantName;


        public BooleanReference enablePollinateButtonFeature;
        public Button pollinateButton;

        public FloatGeneticDriver[] floatDrivers;
        public BooleanGeneticDriver[] booleanDrivers;

        public TMP_Text plantDescription;

        private void Awake()
        {
            pollinateButton.onClick.AddListener(DoPollinate);
            selectedPlant.Value
                .Merge(enablePollinateButtonFeature.ValueChanges.Select(x => selectedPlant.CurrentValue))
                .TakeUntilDestroy(this)
                .Select(x => x == null ? null : x)
                .Subscribe(newObject =>
                {
                    var plantContainer = newObject?.GetComponentInParent<PlantedLSystem>();
                    if (plantContainer == null)
                    {
                        ClearPlantDataUI();
                    }
                    else
                    {
                        RebuildPlantDataViewUI(plantContainer);
                    }
                }).AddTo(this);
        }

        public void DoPollinate()
        {
            manipulatorVariable.SetValue(pollinator);
        }

        public void ClearPlantDataUI()
        {
            gameObject.SetActive(false);
        }

        public void RebuildPlantDataViewUI(PlantedLSystem selectedPlant)
        {
            gameObject.SetActive(true);
            var isPlanted = selectedPlant.plantType != null;
            if (!isPlanted)
            {
                plantName.text = "Empty";
                return;
            }
            plantName.text = selectedPlant.plantType.plantName;
            pollinateButton.gameObject.SetActive(enablePollinateButtonFeature.CurrentValue && selectedPlant.CanPollinate());
            plantDescription.text = GetPlantDescription(selectedPlant);
        }

        private string GetPlantDescription(PlantedLSystem container)
        {
            var drivers = container.GeneticDrivers;
            if (drivers == null)
            {
                return "";
            }
            var resultstring = new StringBuilder();
            foreach (var driver in floatDrivers)
            {
                if (drivers.TryGetGeneticData(driver, out var driverValue))
                {
                    resultstring.AppendLine(driver.DescribeState(driverValue));
                }
            }
            foreach (var driver in booleanDrivers)
            {
                if (drivers.TryGetGeneticData(driver, out var driverValue))
                {
                    resultstring.AppendLine(driver.DescribeState(driverValue));
                }
            }

            var seedCount = container.CurrentSeedCount();
            resultstring.AppendLine($"Seebs: {seedCount}");
            return resultstring.ToString();
        }
    }
}