using Assets.Scripts.Utilities.Core;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    public class SerializeIntToText : MonoBehaviour
    {
        public IntReference variableToWrite;
        public TextMeshProUGUI textToEdit;
        private void Awake()
        {
            variableToWrite.ValueChanges
                .StartWith(variableToWrite.CurrentValue)
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    textToEdit.text = nextValue.ToString();
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