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

        public GameObject plantDataUI;

        public TMP_Text plantName;

        private void Awake()
        {
            selectedPlant.Value
                .TakeUntilDisable(this)
                .Subscribe(newObject =>
                {
                    var plantContainer = newObject.GetComponent<PlantContainer>();
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
            plantDataUI.SetActive(false);
        }

        public void RebuildPlantDataViewUI(PlantContainer selectedPlant)
        {
            plantDataUI.SetActive(true);
            var isPlanted = selectedPlant.plantType == null;
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