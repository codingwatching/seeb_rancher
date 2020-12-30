﻿using Assets.Scripts.Plants;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.Utilities.Core;
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
            var hits = MyUtilities.RaycastAllToObject(layersToHit);
            if (hits == null)
            {
                return;
            }
            foreach (var hit in hits)
            {
                var planter = hit.collider.gameObject?.GetComponentInParent<PlantContainer>();
                if(planter != null)
                {
                    planter.PollinateFrom(CurrentlySelectedPlant);
                    controller.manipulatorVariable.SetValue(null);
                    break;
                }
            }
        }
    }
}
