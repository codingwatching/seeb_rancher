using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
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
            singleOutlineHelper.UpdateOutlineObject(canClick ? clickable.GetOutlineObject() : null);

            if (canClick && hit.HasValue && Input.GetMouseButtonDown(0))
            {
                ClickObject(clickable, hit.Value);
            }
            return true;
        }

        private (IManipulatorClickReciever clickable, RaycastHit? hit) GetHoveredManipulationReciever()
        {
            var mouseOvered = harvestCaster.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            return (hoveredGameObject?.GetComponentInParent<IManipulatorClickReciever>(), mouseOvered);
        }

        private void ClickObject(IManipulatorClickReciever reciever, RaycastHit hit)
        {
            if (!reciever.SelfHit(hit))
            {
                selectedGameObject.SetValue(null);
            }
        }
    }
}
