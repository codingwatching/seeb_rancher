using Dman.ReactiveVariables;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SeedInventory.Summaries
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
            uiCellImage.color = colorblindMode.CurrentValue ? Color.black : cellColor;

            var newHeight = maxUiCellImageHeight * proportionalWeight;

            uiCellImage.rectTransform.sizeDelta = new Vector2(uiCellImage.rectTransform.sizeDelta.x, newHeight);

            var hasLabel = colorblindMode.CurrentValue;
            float labelWidth = 0;
            if (hasLabel)
            {
                descriptionLabel.gameObject.SetActive(true);
                descriptionLabel.text = description;
                labelWidth = descriptionLabel.preferredWidth;
                descriptionLabel.rectTransform.sizeDelta = new Vector2(labelWidth, descriptionLabel.rectTransform.sizeDelta.y);
            }
            else
            {
                descriptionLabel.gameObject.SetActive(false);
                labelWidth = 0;
            }

            var pos = uiCellComponent.anchoredPosition;
            pos.x = labelWidth;
            uiCellComponent.anchoredPosition = pos;

            var myRect = this.transform as RectTransform;
            myRect.sizeDelta = new Vector2(labelWidth + uiCellComponent.sizeDelta.x, myRect.sizeDelta.y);
        }
    }
}
