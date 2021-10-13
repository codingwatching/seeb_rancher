using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.DisplayControllers
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string currentTooltip;

        private TooltipController tpController => TooltipController.instance;

        public void SetTooltip(string newTooltip)
        {
            currentTooltip = newTooltip;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(currentTooltip))
            {
                return;
            }
            tpController.PushTooltip(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tpController.PopTooltip(this);
        }
    }
}
