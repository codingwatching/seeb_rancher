using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    /// <summary>
    /// provider for drag-selecting
    /// </summary>
    public class DragAreaSelector : MonoBehaviour
    {
        // how far in pixels that the mouse has to move before the current mouse-down is considered a drag
        public static float MouseMoveDragThreshold = 10f;

        public RaycastGroup targetCastGroup;
        public ManipulatorController manipulationController;
        public RectTransform dragAreaRenderer;


        public bool dragging = false;

        private Vector3? originDragScreenPoint;
        private Vector3 lastDragEndPosition;

        private void Start()
        {
            dragAreaRenderer.gameObject.SetActive(false);
            manipulationController.SetDragging(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (dragging)
            {
                if (!Input.GetMouseButton(0))
                {
                    lastDragEndPosition = Input.mousePosition;
                    this.ClearLastDragRange();
                }
                else
                {
                    if (lastDragEndPosition.Equals(Input.mousePosition))
                    {
                        return;
                    }
                    else
                    {
                        lastDragEndPosition = Input.mousePosition;
                    }

                    this.RenderLastDragRange();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && manipulationController.activeManipulator is IAreaSelectManipulator)
                {
                    originDragScreenPoint = Input.mousePosition;
                    dragging = false;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    manipulationController.SetDragging(false);
                    dragging = false;
                    originDragScreenPoint = null;
                    dragAreaRenderer.gameObject.SetActive(false);
                }
                if (Input.GetMouseButton(0) && originDragScreenPoint.HasValue)
                {
                    var mouseDistance = (Input.mousePosition - originDragScreenPoint).Value.magnitude;
                    if (mouseDistance >= MouseMoveDragThreshold)
                    {
                        dragging = true;
                    }
                }
            }
        }

        private void RenderLastDragRange()
        {
            dragAreaRenderer.gameObject.SetActive(true);
            var originX = Mathf.Min(originDragScreenPoint.Value.x, lastDragEndPosition.x);
            var originY = Mathf.Min(originDragScreenPoint.Value.y, lastDragEndPosition.y);
            var width = Mathf.Abs(originDragScreenPoint.Value.x - lastDragEndPosition.x);
            var height = Mathf.Abs(originDragScreenPoint.Value.y - lastDragEndPosition.y);

            dragAreaRenderer.sizeDelta = new Vector2(width, height);
            var pos = dragAreaRenderer.position;
            pos.x = originX;
            pos.y = originY;
            dragAreaRenderer.position = pos;

            manipulationController.SetDragging(true);
            manipulationController.OnDragAreaChanged(new Vector2(originX, originY), new Vector2(width, height));
        }
        private void ClearLastDragRange()
        {
            var originX = Mathf.Min(originDragScreenPoint.Value.x, lastDragEndPosition.x);
            var originY = Mathf.Min(originDragScreenPoint.Value.y, lastDragEndPosition.y);
            var width = Mathf.Abs(originDragScreenPoint.Value.x - lastDragEndPosition.x);
            var height = Mathf.Abs(originDragScreenPoint.Value.y - lastDragEndPosition.y);

            manipulationController.OnAreaSelected(new Vector2(originX, originY), new Vector2(width, height));
            manipulationController.SetDragging(false);

            dragging = false;
            originDragScreenPoint = null;
            dragAreaRenderer.gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
        }

    }
}