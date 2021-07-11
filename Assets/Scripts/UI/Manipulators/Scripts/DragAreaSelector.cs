using Dman.Tiling;
using Dman.Tiling.SquareCoords;
using Dman.Utilities;
using System.Linq;
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


        private Vector3? mouseDownPosition = null;

        private bool dragging = false;


        private UniversalCoordinate originDragPoint = default;
        private UniversalCoordinateRange lastDragRange = default;
        public GameObject dragAreaRenderer;

        private void Start()
        {
            dragAreaRenderer.SetActive(false);
            manipulationController.SetDragging(false);
        }

        // Update is called once per frame
        void Update()
        {
            var hoveredCoordinate = GetHoveredCoordinate();


            if (Input.GetMouseButtonDown(0) && hoveredCoordinate != null && manipulationController.activeManipulator is IAreaSelectManipulator)
            {
                mouseDownPosition = Input.mousePosition;
                originDragPoint = hoveredCoordinate.Value;
                dragging = false;
            }
            if (Input.GetMouseButtonUp(0))
            {
                mouseDownPosition = null;
            }
            if (Input.GetMouseButton(0) && mouseDownPosition.HasValue)
            {
                var mouseDistance = (Input.mousePosition - mouseDownPosition).Value.magnitude;
                if (mouseDistance >= MouseMoveDragThreshold)
                {
                    dragging = true;
                    mouseDownPosition = null;
                }
            }
            if (dragging)
            {
                if (!Input.GetMouseButton(0))
                {
                    if (hoveredCoordinate != null)
                    {
                        var currentDraggingPoint = hoveredCoordinate.Value;
                        lastDragRange = UniversalCoordinateRange.From(
                            RectCoordinateRange.FromCoordsInclusive(currentDraggingPoint.squareDataView, originDragPoint.squareDataView),
                            originDragPoint.CoordinatePlaneID);
                    }
                    this.ClearLastDragRange();
                }
                else if (hoveredCoordinate != null)
                {
                    var currentDraggingPoint = hoveredCoordinate.Value;
                    var newDragRange = UniversalCoordinateRange.From(
                        RectCoordinateRange.FromCoordsInclusive(currentDraggingPoint.squareDataView, originDragPoint.squareDataView),
                        originDragPoint.CoordinatePlaneID);
                    if (newDragRange.Equals(lastDragRange))
                    {
                        return;
                    }else
                    {
                        lastDragRange = newDragRange;
                    }

                    this.RenderLastDragRange();
                }

            }
        }

        private void RenderLastDragRange()
        {
            var coords = lastDragRange.BoundingPolygon();
            var vMin = coords.OrderBy(x => x.x + x.y).First();
            var vMax = coords.OrderByDescending(x => x.x + x.y).First();
            var vAvg = (vMin + vMax) / 2;
            dragAreaRenderer.SetActive(true);
            dragAreaRenderer.transform.localScale = new Vector3(vMax.x - vMin.x, 1, vMax.y - vMin.y);
            dragAreaRenderer.transform.position = new Vector3(vAvg.x, dragAreaRenderer.transform.position.y, vAvg.y);

            manipulationController.SetDragging(true);
            manipulationController.OnDragAreaChanged(lastDragRange);
        }
        private void ClearLastDragRange()
        {
            manipulationController.OnAreaSelected(lastDragRange);
            manipulationController.SetDragging(false);

            dragging = false;
            originDragPoint = default;
            dragAreaRenderer.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            if (lastDragRange.IsValid)
            {
                lastDragRange.rectangleDataView.ToBox(3, out var center, out var size);
                Gizmos.color = new Color(0.1f, 1f, 0.3f, 0.6f);
                Gizmos.DrawCube(center, size);
            }
        }

        private UniversalCoordinate? GetHoveredCoordinate()
        {
            var mouseOvered = targetCastGroup.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            return hoveredGameObject?.GetComponentInParent<TileMember>()?.CoordinatePosition;
        }

    }
}