using Dman.LSystem.SystemRuntime.GlobalCoordinator;
using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;
using UnityFx.Outline;

namespace UI.Manipulators
{
    [CreateAssetMenu(fileName = "ClickSelectorManipulator", menuName = "Tiling/Manipulators/ClickSelectorManipulator", order = 2)]
    public class ClickSelectorManipulator : MapManipulator
    {
        public RaycastGroup harvestCaster;
        public OutlineLayerCollection selectableOutline;

        public GameObjectVariable selectedGameObject;

        private MovingOutlineHelper singleOutlineHelper;
        private ManipulatorController controller;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            singleOutlineHelper = new MovingOutlineHelper(selectableOutline);
        }

        public override void OnClose()
        {
            singleOutlineHelper.ClearOutlinedObject();
        }

        public override bool OnUpdate()
        {
            var (clickable, hit) = GetHoveredManipulationReciever();
            var canClick = clickable?.IsSelectable() ?? false;
            if (!canClick)
            {
                singleOutlineHelper.UpdateOutlineObject(null);
                return true;
            }

            singleOutlineHelper.UpdateOutlineObject(clickable.GetOutlineObject());

            if (canClick && Input.GetMouseButtonDown(0))
            {
                ClickObject(clickable, hit);
            }
            return true;
        }

        private (IManipulatorClickReciever clickable, uint hoveredId) GetHoveredManipulationReciever()
        {

            var hoveredId = SelectedIdProvider.instance.HoveredId;
            var hoveredController = GlobalLSystemCoordinator.instance.GetBehaviorContainingOrganId(hoveredId)?.GetComponentInParent<IManipulatorClickReciever>();
            if (hoveredController != null)
            {
                return (hoveredController, hoveredId);
            }

            var mouseOvered = harvestCaster.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            var raycastedClickable = hoveredGameObject?.GetComponentInParent<IManipulatorClickReciever>();

            return (raycastedClickable, 0);
        }

        private void ClickObject(IManipulatorClickReciever reciever, uint objectId)
        {
            if (!reciever.SelfClicked(objectId))
            {
                selectedGameObject.SetValue(null);
            }
        }
    }
}
