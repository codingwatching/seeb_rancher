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
        public GameObject plantRenderer;


        private void Awake()
        {
            selectedPlant.Value
                .TakeUntilDestroy(this)
                .Select(x => x == null ? null : x)
                .Subscribe(newObject =>
                {
                    ClearPlantViewer();
                    var plantContainer = newObject?.GetComponentInParent<PlantedLSystem>();
                    if (plantContainer?.plantType != null)
                    {
                        SetupPlantViewer(plantContainer);
                    }
                }).AddTo(this);
        }

        private void SetupPlantViewer(PlantedLSystem plantContainer)
        {
            plantRenderer.SetActive(true);

            var sourceMesh = plantContainer.GetComponentInChildren<MeshFilter>();
            var targetMeshFilter = plantRenderer.GetComponent<MeshFilter>();
            targetMeshFilter.mesh = sourceMesh.mesh;

            var sourceRenderer = plantContainer.GetComponentInChildren<MeshRenderer>();
            var targetRenderer = plantRenderer.GetComponent<MeshRenderer>();
            targetRenderer.materials = sourceRenderer.materials;
        }

        private void ClearPlantViewer()
        {
            plantRenderer.SetActive(false);
        }
    }
}
