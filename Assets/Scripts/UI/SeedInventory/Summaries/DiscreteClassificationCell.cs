using Dman.ReactiveVariables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    public class DiscreteClassificationCell : MonoBehaviour
    {
        public BooleanReference colorblindMode;
        public TMPro.TMP_Text descriptionLabel;
        public RectTransform uiCellComponent;

        public Image uiCellImage;
        public float maxUiCellImageHeight;

        public void SetCellDisplay(float proportionalWeight, Color cellColor, string description)
        {
            descriptionLabel.text = description;
            uiCellImage.color = cellColor;

            var newHeight = maxUiCellImageHeight * proportionalWeight;

            uiCellImage.rectTransform.sizeDelta = new Vector2(uiCellImage.rectTransform.sizeDelta.x, newHeight);


            descriptionLabel.rectTransform.sizeDelta = new Vector2(descriptionLabel.preferredWidth, descriptionLabel.rectTransform.sizeDelta.y);
            var pos = uiCellComponent.anchoredPosition;
            pos.x = descriptionLabel.rectTransform.sizeDelta.x;
            uiCellComponent.anchoredPosition = pos;

            var myRect = this.transform as RectTransform;
            myRect.sizeDelta = new Vector2(descriptionLabel.rectTransform.sizeDelta.x + uiCellComponent.sizeDelta.x, myRect.sizeDelta.y);
        }
    }
}
