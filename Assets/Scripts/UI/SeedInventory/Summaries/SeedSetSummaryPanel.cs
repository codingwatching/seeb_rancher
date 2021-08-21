using Dman.ReactiveVariables;
using Genetics.GeneSummarization;
using UniRx;
using UnityEngine;

namespace UI.SeedInventory.Summaries
{
    public class SeedSetSummaryPanel : MonoBehaviour
    {
        public GameObjectVariable hoveredDropSlot;

        public SeedSetSummaryRow summaryRowPrefab;
        public GameObject summaryRowParent;

        public GeneticDriverSummarySet summarization;

        private void Awake()
        {
            hoveredDropSlot.Value.TakeUntilDestroy(gameObject)
                .StartWith((GameObject)null)
                .Subscribe(x =>
                {
                    if (x == null) x = null;
                    var summary = x?.GetComponent<SeedInventoryDropSlot>()?.summarization;
                    if (summary == null)
                    {
                        summaryRowParent.SetActive(false);
                    }
                    else
                    {
                        summaryRowParent.SetActive(true);
                        this.DisplaySummary(summary);
                    }
                }).AddTo(gameObject);
        }

        public void DisplaySummary(GeneticDriverSummarySet summary)
        {
            foreach (Transform child in summaryRowParent.transform)
            {
                if (child.GetComponent<SeedSetSummaryRow>())
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var summaryItem in summary.summaries)
            {
                var newRow = Instantiate(summaryRowPrefab, summaryRowParent.transform);
                newRow.DisplaySummary(summaryItem);
            }
        }
    }
}
