using UnityEngine;

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
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100, layersToHit))
                {
                    Debug.DrawLine(ray.origin, hit.point);
                    Debug.Log($"Clicked {hit.collider.gameObject.name}");
                    hit.collider.gameObject.GetComponentInChildren<IManipulatorClickReciever>()?.SelfHit(hit);
                }
            }
        }
    }
}
