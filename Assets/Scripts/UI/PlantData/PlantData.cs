using Assets.Scripts.Plants;
using Assets.Scripts.Utilities.Core;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.PlantData
{
    public class PlantData : MonoBehaviour
    {
        public GameObjectVariable selectedPlant;

        public TMP_Text plantName;

        private void Awake()
        {
            selectedPlant.Value
                .TakeUntilDestroy(this)
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

        public void ClearPlantDataUI()
        {
            gameObject.SetActive(false);
        }

        public void RebuildPlantDataViewUI(PlantContainer selectedPlant)
        {
            gameObject.SetActive(true);
            var isPlanted = selectedPlant.plantType != null;
            if (isPlanted)
            {
                plantName.text = selectedPlant.plantType.plantName;
            }else
            {
                plantName.text = "Empty";
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}