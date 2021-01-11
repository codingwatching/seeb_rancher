using Assets.Scripts.Plants;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.UI.Buttery_Toast;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.PlantData
{
    [CreateAssetMenu(fileName = "PollinatePlantManipulator", menuName = "Tiling/Manipulators/PollinatePlantManipulator", order = 2)]
    public class PollinatePlantManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public LayerMask layersToHit;
        public GameObjectVariable selectedThing;

        public Sprite harvestCursor;
        private PlantContainer CurrentlySelectedPlant => selectedThing.CurrentValue?.GetComponent<PlantContainer>();

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
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
        }

        public override void OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }
            var hits = MouseOverHelpers.RaycastAllToObject(layersToHit);
            if (hits == null)
            {
                return;
            }
            foreach (var hit in hits)
            {
                var planter = hit.collider.gameObject?.GetComponentInParent<PlantContainer>();
                if (planter != null)
                {
                    var currentPlant = CurrentlySelectedPlant;
                    if (planter.PollinateFrom(currentPlant))
                    {
                        ToastProvider.ShowToast("pollinated", planter.gameObject, 1);
                        if (currentPlant == planter)
                        {
                            selectedThing.SetValue(selectedThing.CurrentValue);
                            // could've done a self-pollinate, in which case we should close the pollination tool
                            if (!currentPlant.CanPollinate())
                            {
                                controller.manipulatorVariable.SetValue(null);
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
