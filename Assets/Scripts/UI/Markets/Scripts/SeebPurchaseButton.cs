using Assets.Scripts.UI.SeedInventory;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class SeebPurchaseButton : MonoBehaviour
    {
        public SeebBinContainer seebBin;
        public SeedInventoryDropSlot purchaseSpot;

        public void PurchaseButtonClicked()
        {
            if (seebBin.binDescriptor.seedCount <= 0)
            {
                return;
            }

            StartCoroutine(GenerateSeebs(1));
        }

        private IEnumerator GenerateSeebs(int seebNum)
        {
            var bin = seebBin.binDescriptor;
            yield return StartCoroutine(bin.EvaluateNewSeebs(seebNum));

            var seeds = bin.GeneratedSeebs;
            var seebUi = purchaseSpot.dataModel;
            seebUi.description = "generated";
            seebUi.bucket.TryAddSeedsToSet(seeds.ToArray());
            purchaseSpot.MySeedsUpdated();
        }
    }
}