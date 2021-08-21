using Genetics.GeneSummarization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    public class HistogramSummaryDisplay : MonoBehaviour
    {
        public Image histogramImage;
        public TMPro.TMP_Text minValueText;
        public TMPro.TMP_Text maxValueText;
        public void SetSummaryDisplay(ContinuousSummary summary)
        {
            minValueText.text = summary.minValue.ToString();
            maxValueText.text = summary.maxValue.ToString();

            var myRect = GetComponent<RectTransform>().rect;
            var texture = new Texture2D((int)myRect.width, (int)myRect.height);
            texture.filterMode = FilterMode.Point;
            var newSprite = Sprite.Create(texture, new Rect(Vector2.zero, myRect.size), Vector2.zero);

            var image = histogramImage;
            image.sprite = newSprite;

            var histogramResult = summary.RenderContinuousHistogram(
                texture.width,
                x => 1 - x);

            //Debug.Log(string.Join(",", histogramResult.Select((x, i) => x > 0.5 ? i.ToString() : "")));

            for (int x = 0; x < histogramResult.Length; x++)
            {
                var histogramIntensity = histogramResult[x];
                var color = new Color(1, 1, 1, histogramIntensity);
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
        }
    }
}