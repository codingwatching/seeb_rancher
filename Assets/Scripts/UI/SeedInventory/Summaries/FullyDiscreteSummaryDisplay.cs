using Dman.Utilities;
using Genetics.GeneSummarization;
using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace UI.SeedInventory.Summaries
{
    public class FullyDiscreteSummaryDisplay : MonoBehaviour
    {
        public DiscreteClassificationCell singleDiscreteValuePrefab;
        public RectTransform discreteCellsPrefabParent;

        public void SetSummaryDisplay(DiscretSummary summary)
        {
            discreteCellsPrefabParent.gameObject.DestroyAllChildren();

            float maxBucketSize = summary.allClassifications.Max(x => x.totalClassifications);
            float totalBucketSize = summary.allClassifications.Sum(x => x.totalClassifications);

            for (int i = 0; i < summary.allClassifications.Length; i++)
            {
                var bucket = summary.allClassifications[i];
                Color color;
                if (summary.sourceDriver is BooleanGeneticDriver boolDriver)
                {
                    color = i == 1 ? boolDriver.trueColor : boolDriver.falseColor;
                }
                else if (summary.sourceDriver is DiscreteFloatGeneticDriver floatDriver)
                {
                    color = i < floatDriver.possibleColors.Length ? floatDriver.possibleColors[i] : Color.black;
                }
                else
                {
                    Debug.LogError("Trying to summarize a summary with no associated driver");
                    return;
                }

                var newCell = Instantiate(singleDiscreteValuePrefab, discreteCellsPrefabParent);
                newCell.SetCellDisplay(bucket.totalClassifications / totalBucketSize, color, bucket.description);
            }

            // TODO
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