using Dman.ReactiveVariables;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    public class SerializeIntToText : MonoBehaviour
    {
        public IntReference variableToWrite;
        public TextMeshProUGUI textToEdit;
        public string textFormat = "#";

        private void Awake()
        {
            variableToWrite.ValueChanges
                .StartWith(variableToWrite.CurrentValue)
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    var numberText = nextValue.ToString();
                    textToEdit.text = textFormat.Replace("#", numberText);
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