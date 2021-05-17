using Genetics.GeneSummarization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedSetSummaryRow : MonoBehaviour
    {
        public TMPro.TMP_Text attributeDescriptor;


        public void DisplaySummary(KeyValuePair<string, AbstractSummary> summaryByName)
        {
            attributeDescriptor.text = summaryByName.Key;
        }
    }
}
