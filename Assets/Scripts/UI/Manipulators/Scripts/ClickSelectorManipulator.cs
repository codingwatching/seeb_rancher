using Assets.Scripts.Utilities.Core;
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
            Debug.Log("click manipulator opened");
        }

        public override void OnClose()
        {
            selectedGameObject.SetValue(null);
        }

        public override void OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }
            var hits = MyUtilities.RaycastAllToObject(layersToHit);
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    var clicker = hit.collider.gameObject.GetComponentInParent<IManipulatorClickReciever>();
                    if (clicker != null && clicker.SelfHit(hit))
                    {
                        return;
                    }
                }
            }
            selectedGameObject.SetValue(null);
        }
    }
}
