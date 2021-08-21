using Dman.LSystem.SystemRuntime.GlobalCoordinator;
using Dman.ReactiveVariables;
using Dman.Utilities;
using Simulation.Plants;
using UI.Buttery_Toast;
using UnityEngine;
using UnityFx.Outline;

namespace UI.Manipulators
{
    [CreateAssetMenu(fileName = "PollinatePlantManipulator", menuName = "Tiling/Manipulators/PollinatePlantManipulator", order = 2)]
    public class PollinatePlantManipulator : MapManipulator
    {

        public RaycastGroup targetCaster;
        public LayerMask layersToHit;
        public GameObjectVariable selectedThing;

        public Sprite harvestCursor;
        private PlantedLSystem CurrentlySelectedPlant => selectedThing.CurrentValue?.GetComponent<PlantedLSystem>();
        private ManipulatorController controller;
        private MovingOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            var selectedPlantContainer = CurrentlySelectedPlant;
            if (!selectedPlantContainer.pollinationState.CanPollinate())
            {
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            CursorTracker.SetCursor(harvestCursor);
            singleOutlineHelper = new MovingOutlineHelper(outlineCollection);
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
            targetPlant.ClipAnthers();

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
        private PlantedLSystem GetHoveredPlantContainer()
        {
            var behavior = GlobalLSystemCoordinator.instance.GetBehaviorContainingOrganId(SelectedIdProvider.instance.HoveredId);

            // TODO: ensure l system behavior is in same game object as planted l system
            return behavior?.GetComponent<PlantedLSystem>();
        }
    }
}
