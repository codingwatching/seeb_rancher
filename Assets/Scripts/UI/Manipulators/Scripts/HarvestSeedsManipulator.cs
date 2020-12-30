using Assets.Scripts.Plants;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "HarvestSeedsManipulator", menuName = "Tiling/Manipulators/HarvestSeedsManipulator", order = 2)]
    public class HarvestSeedsManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public LayerMask harvestLayers;

        public Sprite harvestCursor;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            Debug.Log("harvest manipulator opened");
            CursorTracker.SetCursor(harvestCursor);
        }

        public override void OnClose()
        {
            CursorTracker.ClearCursor();
        }

        public override void OnUpdate()
        {
            var hits = MyUtilities.RaycastAllToObject(harvestLayers);
            if (hits == null)
            {
                return;
            }
            foreach (var hit in hits)
            {
                var planter = hit.collider.gameObject?.GetComponentInParent<PlantContainer>();
                if (planter.TryHarvest())
                {
                    return;
                }
            }
        }
    }
}
