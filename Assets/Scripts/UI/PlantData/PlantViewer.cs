using Dman.ReactiveVariables;
using Simulation.Plants;
using UniRx;
using UnityEngine;

namespace UI.PlantData
{
    public class PlantViewer : MonoBehaviour
    {
        public GameObjectVariable selectedPlant;
        public GameObject plantRenderer;
        public float maxBoundSizeToOrthographicAdjustment = 1;
        public Camera renderCamera;

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


            var boundSize = targetMeshFilter.mesh.bounds.size;
            boundSize.Scale(plantRenderer.transform.localScale);
            var maxBound = Mathf.Max(boundSize.x, boundSize.y, boundSize.z);

            targetRenderer.transform.localPosition = -Vector3.Scale(targetMeshFilter.mesh.bounds.center, plantRenderer.transform.localScale);

            var newOrthoSize = maxBound * maxBoundSizeToOrthographicAdjustment;
            renderCamera.orthographicSize = newOrthoSize;
        }

        private void ClearPlantViewer()
        {
            plantRenderer.SetActive(false);
        }
    }
}
