using Genetics.GeneSummarization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.DataModels;
using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using Genetics.GeneticDrivers;
using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.VFX;

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
