using Dman.ReactiveVariables;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts.ChildCycler
{

    public class PhaseLifetimeExpiration : MonoBehaviour
    {
        public IntReference levelPhase;
        [Tooltip("phases till this gameobject gets destroyed. -1 is infinite")]
        public int timeTillExpiration;

        public string expirationNumberFormat = "# weeks";
        public TMP_Text expirationText;

        void Start()
        {
            UpdateText();
            levelPhase.ValueChanges
                .TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current - pair.Previous != 1)
                    {
                        return;
                    }
                    DetractPhase();
                }).AddTo(this);
        }

        private void DetractPhase()
        {
            if(timeTillExpiration == -1)
            {
                UpdateText();
                return;
            }
            timeTillExpiration--;
            if (timeTillExpiration < 0)
            {
                Destroy(gameObject);
            }else
            {
                UpdateText();
            }
        }

        private void UpdateText()
        {
            var replacement = timeTillExpiration == -1 ? "∞" : timeTillExpiration.ToString();

            expirationText.text = expirationNumberFormat
                .Replace("#", replacement);
        }
    }

}