using Assets.Scripts.Plants;
using Assets.UI.Buttery_Toast;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "PollinatePlantManipulator", menuName = "Tiling/Manipulators/PollinatePlantManipulator", order = 2)]
    public class PollinatePlantManipulator : MapManipulator
    {

        [SerializeField] public RaycastGroup targetCaster;
        public LayerMask layersToHit;
        public GameObjectVariable selectedThing;

        public Sprite harvestCursor;
        private PlantContainer CurrentlySelectedPlant => selectedThing.CurrentValue?.GetComponent<PlantContainer>();
        private ManipulatorController controller;
        private MovingSingleOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            var selectedPlantContainer = CurrentlySelectedPlant;
            if (!selectedPlantContainer.polliationState.CanPollinate())
            {
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            CursorTracker.SetCursor(harvestCursor);
            singleOutlineHelper = new MovingSingleOutlineHelper(outlineCollection);
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
            singleOutlineHelper.ClearOutlinedObject();
        }

        public override bool OnUpdate()
        {
            var targetPlant = GetHoveredPlantContainer();

            var currentPlant = CurrentlySelectedPlant;
            // must be able to pollinate. check for genetic compatability, anthers, etc
            var planterValid = targetPlant?.CanPollinateFrom(currentPlant) ?? false;
            singleOutlineHelper.UpdateOutlineObject(planterValid ? targetPlant.GetOutlineObject() : null);

            if (!planterValid || !Input.GetMouseButtonDown(0) || !targetPlant.PollinateFrom(currentPlant))
            {
                return true;
            }

            ToastProvider.ShowToast("pollinated", targetPlant.gameObject, 1);
            if (currentPlant == targetPlant)
            {
                // force update of selected plant views
                selectedThing.SetValue(selectedThing.CurrentValue);
                // could've done a self-pollinate, in which case we should close the pollination tool
                if (!currentPlant.CanPollinate())
                {
                    controller.manipulatorVariable.SetValue(null);
                }
            }
            return true;
        }
        private PlantContainer GetHoveredPlantContainer()
        {
            var mouseOvered = targetCaster.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            return hoveredGameObject?.GetComponentInParent<PlantContainer>();
        }
    }
}
