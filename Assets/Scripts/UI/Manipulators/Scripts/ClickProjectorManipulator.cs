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
        }

        public override void OnClose()
        {
        }

        public override void OnUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100, layersToHit))
                {
                    Debug.DrawLine(ray.origin, hit.point);
                    hit.collider.gameObject.GetComponentInParent<IManipulatorClickReciever>()?.SelfHit(hit);
                }
            }
        }
    }
}
