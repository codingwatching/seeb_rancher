using Assets.Scripts.Utilities.Core;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    public class FormatFloat : MonoBehaviour
    {
        public FloatReference variableToWrite;
        public TextMeshProUGUI textToEdit;
        public string textFormat = "{0:F2}";

        private void Awake()
        {
            variableToWrite.ValueChanges
                .StartWith(variableToWrite.CurrentValue)
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    textToEdit.text = string.Format(textFormat, nextValue);
                })
                .AddTo(this);
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}