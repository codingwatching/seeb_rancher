using Assets.Scripts.UI.SeedInventory;
using Dman.ReactiveVariables;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    public class SeebPurchaseButton : MonoBehaviour
    {
        public SeebBinContainer seebBin;
        public SeedInventoryDropSlot purchaseSpot;
        public FloatReference money;

        public void PurchaseButtonClicked()
        {
            var seebCount = seebBin.binDescriptor.seedCount >= 0 ? seebBin.binDescriptor.seedCount : 5;
            if (seebCount <= 0)
            {
                return;
            }

            var cost = seebBin.binDescriptor.price;
            if (money.CurrentValue < cost)
            {
                // not enough money
                return;
            }

            StartCoroutine(GenerateSeebs(seebCount, cost));
        }

        private IEnumerator GenerateSeebs(int seebNum, float price)
        {
            var bin = seebBin.binDescriptor;
            yield return StartCoroutine(bin.EvaluateNewSeebs(seebNum));

            var seeds = bin.GeneratedSeebs;
            var seebUi = purchaseSpot.dataModel;
            seebUi.description = "generated";
            var success = seebUi.bucket.TryAddSeedsToSet(seeds.ToArray());
            purchaseSpot.MySeedsUpdated();
            if (success)
            {
                if (seebBin.binDescriptor.seedCount != -1)
                {
                    seebBin.binDescriptor.seedCount -= seeds.Count;
                }
                seebBin.BinAndSeebSlotStateUpdated();
                money.SetValue(money.CurrentValue - price);
            }
        }
    }
}