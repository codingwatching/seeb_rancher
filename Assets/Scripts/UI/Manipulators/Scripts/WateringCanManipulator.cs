using Dman.ObjectSets;
using Dman.ReactiveVariables;
using Dman.Utilities;
using Gameplay;
using Simulation.Plants.PlantData;
using Simulation.Plants.PlantTypes;
using UI.SeedInventory;
using UnityEngine;

namespace UI.Manipulators
{
    [CreateAssetMenu(fileName = "WateringCanManipulator", menuName = "Tiling/Manipulators/WateringCanManipulator", order = 4)]
    public class WateringCanManipulator : MapManipulator
    {
        public bool IsActive { get; private set; }

        [SerializeField] public RaycastGroup dirtCaster;
        public FloatVariable waterInventory;

        private ManipulatorController controller;

        private WateringCan wateringCan;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            Debug.Log("watering can manipulator opened");

            wateringCan = GameObject.FindObjectOfType<WateringCan>(true);

            IsActive = true;
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
            wateringCan.isWatering = false;
            wateringCan.gameObject.SetActive(false);
            wateringCan.wateringCanRenderer.gameObject.SetActive(false);
            wateringCan = null;
            IsActive = false;
        }

        public override bool OnUpdate()
        {
            if(waterInventory.CurrentValue < wateringCan.waterFlowRate * Time.deltaTime)
            {
                wateringCan.gameObject.SetActive(false);
                wateringCan.wateringCanRenderer.gameObject.SetActive(false);
                return false;
            }

            var hoveredSpot = dirtCaster.CurrentlyHitObject;
            if (!hoveredSpot.HasValue)
            {
                wateringCan.gameObject.SetActive(false);
                wateringCan.wateringCanRenderer.gameObject.SetActive(false);
                return true;
            }
            wateringCan.gameObject.SetActive(true);
            wateringCan.wateringCanRenderer.gameObject.SetActive(true);
            wateringCan.transform.position = hoveredSpot.Value.point;

            wateringCan.isWatering = Input.GetMouseButton(0);
            return true;
        }
    }
}
