using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.DisplayControllers
{
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController instance;

        public TMPro.TMP_Text tooltipText;
        public GameObject tooltipObject;

        private List<Tooltip> prioritizedTooltips;

        private void Awake()
        {
            prioritizedTooltips = new List<Tooltip>();
            instance = this;
        }

        public void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;

            var quadrantVector = (mousePos * 2) / new Vector2(Screen.width, Screen.height);
            var quadrant = new Vector2Int(Mathf.FloorToInt(quadrantVector.x), Mathf.FloorToInt(quadrantVector.y));

            var tooltipPositioner = tooltipObject.GetComponent<RectTransform>();
            tooltipPositioner.anchorMin = quadrant;
            tooltipPositioner.anchorMax = quadrant;
            tooltipPositioner.pivot = quadrant;
            tooltipPositioner.anchoredPosition = Vector2.zero;
        }

        public void PushTooltip(Tooltip owner)
        {
            prioritizedTooltips.Add(owner);
            UpdateTooltip();
        }

        public void PopTooltip(Tooltip owner)
        {
            prioritizedTooltips.Remove(owner);
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            for (int i = prioritizedTooltips.Count - 1; i >= 0; i--)
            {
                var tooltip = prioritizedTooltips[i];
                if (string.IsNullOrEmpty(tooltip.currentTooltip))
                {
                    continue;
                }
                this.SetTooltip(tooltip.currentTooltip);
                return;
            }
            this.ClearTooltip();
        }

        private void SetTooltip(string text)
        {
            this.tooltipText.text = text;
            tooltipObject.SetActive(true);
        }

        private void ClearTooltip()
        {
            tooltipObject.SetActive(false);
        }


        private void TestHoveredObjects()
        {
            var evs = EventSystem.current;
            if (evs.IsPointerOverGameObject())
            {
                var pointer = new PointerEventData(evs)
                {
                    position = Input.mousePosition
                };
                var hits = new List<RaycastResult>();
                evs.RaycastAll(pointer, hits);

                foreach (var hit in hits)
                {
                    if (!hit.isValid || hit.gameObject == null)
                    {
                        continue;
                    }
                    Debug.Log("Mouse Over: " + hit.gameObject.name);
                }
            }
        }
    }
}
