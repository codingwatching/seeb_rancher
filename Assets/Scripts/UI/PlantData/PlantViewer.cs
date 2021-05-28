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
                .Select(x => x == null ? null : x)
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
            var plantModel = plantContainer.plantsParent.transform.GetChild(0);
            var plantMesh = plantModel.GetComponentInChildren<MeshFilter>();


            var newObject = Instantiate(plantModel, this.transform);
            var newMesh = newObject.GetComponentInChildren<MeshFilter>();
            newMesh.mesh = plantMesh.mesh;
        }

        private void ClearPlantViewer()
        {
            gameObject.DestroyAllChildren();
        }
    }
}
