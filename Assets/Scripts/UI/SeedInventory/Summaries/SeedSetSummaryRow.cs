using Dman.Utilities;
using Genetics.GeneSummarization;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{

    public class SeedSetSummaryRow : MonoBehaviour
    {
        public TMPro.TMP_Text attributeDescriptor;
        public GameObject summaryObjectContainer;

        public HistogramSummaryDisplay continuousSummaryPrefab;
        public FullyDiscreteSummaryDisplay discretSummaryPrefab;


        public void DisplaySummary(KeyValuePair<string, AbstractSummary> summaryByName)
        {
            attributeDescriptor.text = summaryByName.Key;

            summaryObjectContainer.DestroyAllChildren();
            if (summaryByName.Value is ContinuousSummary contSum && continuousSummaryPrefab != null)
            {
                var summarizer = Instantiate(continuousSummaryPrefab, summaryObjectContainer.transform);
                summarizer.SetSummaryDisplay(contSum);
            }
            else if (summaryByName.Value is DiscretSummary distSum && discretSummaryPrefab != null)
            {
                var summarizer = Instantiate(discretSummaryPrefab, summaryObjectContainer.transform);
                summarizer.SetSummaryDisplay(distSum);
            }
        }
    }
}
