using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Markets.ChildCycler
{

    public class PhaseLifetimeExpiration : MonoBehaviour, ISaveableData
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
            if (timeTillExpiration == -1)
            {
                UpdateText();
                return;
            }
            timeTillExpiration--;
            if (timeTillExpiration < 0)
            {
                Destroy(gameObject);
            }
            else
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

        #region Saving
        public string UniqueSaveIdentifier => "PhaseLifetimeExpiration";

        [System.Serializable]
        class SaveObject
        {
            int timeTillExpire;
            string expirationFormat;

            public SaveObject(PhaseLifetimeExpiration source)
            {
                timeTillExpire = source.timeTillExpiration;
                expirationFormat = source.expirationNumberFormat;
            }
            public void ApplyTo(PhaseLifetimeExpiration target)
            {
                target.timeTillExpiration = timeTillExpire;
                target.expirationNumberFormat = expirationFormat;

            }
        }
        public object GetSaveObject()
        {
            return new SaveObject(this);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SaveObject data)
            {
                data.ApplyTo(this);
                UpdateText();
            }
        }
        #endregion
    }

}