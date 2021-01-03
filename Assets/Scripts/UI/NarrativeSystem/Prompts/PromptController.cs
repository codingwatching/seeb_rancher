using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.NarrativeSystem
{
    public class PromptController : MonoBehaviour
    {
        public TMP_Text promptText;
        public Button nextPromptButton;
        public Action OnClosed;

        private void Awake()
        {
            nextPromptButton.onClick.AddListener(() =>
            {
                OnClosed?.Invoke();
            });
        }

        public void Opened(string text, Action onClosed)
        {
            promptText.text = text;
            nextPromptButton.onClick.AddListener(() => onClosed?.Invoke());
        }
    }
}
