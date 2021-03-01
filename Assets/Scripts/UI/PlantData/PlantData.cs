using Assets.Scripts.Plants;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.PlantData
{
    public class PlantData : MonoBehaviour
    {
        public GameObjectVariable selectedPlant;
        public ScriptableObjectVariable manipulatorVariable;
        public PollinatePlantManipulator pollinator;

        public TMP_Text plantName;

        public BooleanReference enablePollinateButtonFeature;
        public Button pollinateButton;

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
        }
    }
}