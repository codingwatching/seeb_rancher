using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dman.NarrativeSystem
{
    public class PromptController : MonoBehaviour
    {
        public TMP_Text promptText;
        public Button nextPromptButton;
        public Image speakerSprite;
        public TMP_Text speakerName;

        public void Opened(string text, Sprite speaker, Action onClosed)
        {
            speakerName.text = speaker.name;
            speakerSprite.sprite = speaker;
            promptText.text = text;
            if (onClosed == null)
            {
                nextPromptButton.gameObject.SetActive(false);
            }
            else
            {
                nextPromptButton.onClick.AddListener(() => onClosed?.Invoke());
            }
        }
    }
}
