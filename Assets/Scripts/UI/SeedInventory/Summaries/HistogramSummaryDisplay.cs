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

            var image = histogramImage;
            image.material.mainTexture = texture;

            var histogramResult = summary.RenderContinuousHistogram(
                texture.width,
                x => 1 - x);

            for (int x = 0; x < histogramResult.Length; x++)
            {
                var histogramIntensity = histogramResult[x];
                var color = new Color(1 - histogramIntensity, 1 - histogramIntensity, 1 - histogramIntensity);
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}