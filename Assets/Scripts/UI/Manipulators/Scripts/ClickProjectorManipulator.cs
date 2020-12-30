using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "ClickProjectorManipulator", menuName = "Tiling/Manipulators/ClickProjectorManipulator", order = 2)]
    public class ClickProjectorManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public LayerMask layersToHit;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            Debug.Log("click manipulator opened");
        }

        public override void OnClose()
        {
        }

        public override void OnUpdate()
        {
            var hits = MyUtilities.RaycastAllToObject(layersToHit);
            if (hits == null)
            {
                return;
            }
            foreach (var hit in hits)
            {
                var clicker = hit.collider.gameObject.GetComponentInParent<IManipulatorClickReciever>();
                if(clicker != null && clicker.SelfHit(hit))
                {
                    return;
                }
            }
        }
    }
}
