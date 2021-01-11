using Assets.Scripts.Plants;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.PlantData
{
    public class PlantViewer : MonoBehaviour
    {
        public GameObjectVariable selectedPlant;


        private void Awake()
        {
            selectedPlant.Value
                .TakeUntilDestroy(this)
                .Subscribe(newObject =>
                {
                    ClearPlantViewer();
                    var plantContainer = newObject?.GetComponentInParent<PlantContainer>();
                    if (plantContainer?.plantType != null)
                    {
                        SetupPlantViewer(plantContainer);
                    }
                }).AddTo(this);
        }

        private void SetupPlantViewer(PlantContainer plantContainer)
        {
            foreach (Transform transform in plantContainer.plantsParent.transform)
            {
                Instantiate(transform.gameObject, this.transform);
            }
        }

        private void ClearPlantViewer()
        {
            gameObject.DestroyAllChildren();
        }
    }
}
