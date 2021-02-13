using Dman.ReactiveVariables;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "ClickSelectorManipulator", menuName = "Tiling/Manipulators/ClickSelectorManipulator", order = 2)]
    public class ClickSelectorManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public LayerMask layersToHit;

        public GameObjectVariable selectedGameObject;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
        }

        public override void OnClose()
        {
        }

        public override bool OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return true;
            }
            if (!MouseOverHelpers.RaycastToObject(layersToHit, out var singleHit))
            {
                // if hit the UI or nothing, do nothing
                return true;
            }
            var clicker = singleHit.collider.gameObject.GetComponentInParent<IManipulatorClickReciever>();
            if (clicker != null && clicker.SelfHit(singleHit))
            {
                return true;
            }
            selectedGameObject.SetValue(null);
            return true;
        }
    }
}
