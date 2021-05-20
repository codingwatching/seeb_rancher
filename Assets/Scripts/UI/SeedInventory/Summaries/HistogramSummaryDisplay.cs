using Genetics.GeneSummarization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    [RequireComponent(typeof(Image))]
    public class HistogramSummaryDisplay : MonoBehaviour
    {

        public void SetSummaryDisplay(ContinuousSummary summary)
        {
            var myRect = GetComponent<RectTransform>().rect;
            var texture = new Texture2D((int)myRect.width, (int)myRect.height);
            texture.filterMode = FilterMode.Point;

            var image = GetComponent<Image>();
            image.material.mainTexture = texture;

            var randgen = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            for (int i = 0; i < 1000; i++)
            {
                var pixelPoint = new Vector2Int(randgen.Next(0, texture.width), randgen.Next(0, texture.height));
                var color = new Color(
                    (float)randgen.NextDouble(),
                    (float)randgen.NextDouble(),
                    (float)randgen.NextDouble());

                texture.SetPixel(pixelPoint.x, pixelPoint.y, color);
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