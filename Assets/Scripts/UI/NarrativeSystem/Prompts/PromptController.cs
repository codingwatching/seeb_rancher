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
        public Image speakerSprite;
        public TMP_Text speakerName;

        private void Awake()
        {
            nextPromptButton.onClick.AddListener(() =>
            {
                OnClosed?.Invoke();
            });
        }

        public void Opened(string text, Sprite speaker, Action onClosed)
        {
            speakerName.text = speaker.name;
            speakerSprite.sprite = speaker;
            promptText.text = text;
            nextPromptButton.onClick.AddListener(() => onClosed?.Invoke());
        }
    }
}
