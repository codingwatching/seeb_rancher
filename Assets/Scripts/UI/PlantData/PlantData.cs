using Assets.Scripts.Plants;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Genetics;
using System.Text;
using Genetics.GeneticDrivers;

namespace Assets.Scripts.UI.PlantData
{
    public class PlantData : MonoBehaviour
    {
        public IntReference levelPhase;
        public GameObjectVariable selectedPlant;
        public ScriptableObjectVariable manipulatorVariable;
        public PollinatePlantManipulator pollinator;

        public TMP_Text plantName;


        public BooleanReference enablePollinateButtonFeature;
        public Button pollinateButton;

        public FloatGeneticDriver[] floatDrivers;
        public BooleanGeneticDriver[] booleanDrivers;

        public TMP_Text geneticDescription;

        private void Awake()
        {
            pollinateButton.onClick.AddListener(DoPollinate);
            selectedPlant.Value
                .Merge(enablePollinateButtonFeature.ValueChanges.Select(x => selectedPlant.CurrentValue))
                .TakeUntilDestroy(this)
                .Select(x => x == null ? null : x)
                .Subscribe(newObject =>
                {
                    var plantContainer = newObject?.GetComponentInParent<PlantContainer>();
                    if (plantContainer == null)
                    {
                        ClearPlantDataUI();
                    }
                    else
                    {
                        RebuildPlantDataViewUI(plantContainer);
                    }
                }).AddTo(this);

            levelPhase.ValueChanges
                .TakeUntilDisable(this)
                .Subscribe(pair =>
                {
                    // deselect on phase change
                    selectedPlant.SetValue(null);
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

        public void RebuildPlantDataViewUI(PlantContainer selectedPlant)
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
            geneticDescription.text = GetGeneDescription(selectedPlant);
        }

        private string GetGeneDescription(PlantContainer container)
        {
            var drivers = container.GeneticDrivers;
            if(drivers == null)
            {
                return "";
            }
            var resultstring = new StringBuilder();
            foreach (var driver in floatDrivers)
            {
                if (drivers.TryGetGeneticData(driver, out var driverValue))
                {
                    resultstring.AppendLine($"{driver.DriverName}: {driverValue:F2}");
                }
            }
            foreach (var driver in booleanDrivers)
            {
                if (drivers.TryGetGeneticData(driver, out var driverValue))
                {
                    resultstring.AppendLine($"{driver.DriverName}: {driverValue}");
                }
            }
            return resultstring.ToString();
        }
    }
}