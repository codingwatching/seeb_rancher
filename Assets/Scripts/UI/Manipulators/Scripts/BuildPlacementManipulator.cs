using Assets.Scripts.GreenhouseLoader;
using Dman.Tiling;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "BuildPlacementManipulator", menuName = "Tiling/Manipulators/BuildPlacementManipulator", order = 1)]
    public class BuildPlacementManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public TileMember buildPreviewPrefab;
        public TileMember buildMemeberPrefab;
        public LayerMask blockingLayers;

        private TileMember activeBuildPreview;

        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening build manipulator");
            this.controller = controller;
            activeBuildPreview = GameObject.Instantiate(buildPreviewPrefab, controller.transform);
            currentHoverCoordinate = default;
        }

        public override void OnClose()
        {
            if (activeBuildPreview != null)
                Destroy(activeBuildPreview.gameObject);
        }

        private UniversalCoordinate currentHoverCoordinate;

        public override void OnUpdate()
        {
            UpdatePreviewPositionAndBlocking();
            if (currentHoverCoordinate.IsValid() && Input.GetMouseButtonDown(0))
            {
                var greenhouse = GameObject.FindObjectOfType<GreenhouseBuilder>();
                var newBuildableObject = GameObject.Instantiate(buildMemeberPrefab, greenhouse.transform);
                newBuildableObject.SetPosition(currentHoverCoordinate);

                activeBuildPreview.gameObject.SetActive(false);
            }
        }

        private void UpdatePreviewPositionAndBlocking()
        {
            var floorPlan = GameObject.FindObjectOfType<FloorPlan>();
            var hoveredCoordinate = floorPlan.GetHoveredCoordinate();
            if (!hoveredCoordinate.HasValue || !hoveredCoordinate.Value.IsValid())
            {
                currentHoverCoordinate = default;
                activeBuildPreview.gameObject.SetActive(false);
                return;
            }
            if (hoveredCoordinate.Equals(currentHoverCoordinate))
            {
                return;
            }
            currentHoverCoordinate = hoveredCoordinate.Value;

            activeBuildPreview.gameObject.SetActive(true);
            activeBuildPreview.SetPosition(currentHoverCoordinate);
        }
    }
}
