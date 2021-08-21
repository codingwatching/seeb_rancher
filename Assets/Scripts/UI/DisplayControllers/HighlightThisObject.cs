using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace UI.DisplayControllers
{
    [RequireComponent(typeof(RectTransform))]
    public class HighlightThisObject : MonoBehaviour
    {
        public BooleanReference ObjectHighlighted;
        public GameObjectVariable highlightObjectVariable;
        //public RectTransform backdropPrefab;
        //private RectTransform currentBackdrop;


        private void Awake()
        {
            ObjectHighlighted.ValueChanges
                .TakeUntilDestroy(this)
                .StartWith(ObjectHighlighted.CurrentValue)
                .Pairwise()
                .Where(pair => pair.Current != pair.Previous)
                .Subscribe(pair =>
                {
                    if (pair.Current)
                    {
                        highlightObjectVariable.SetValue(gameObject);

                        //SetBackdrop(GetComponent<RectTransform>());
                    }
                    else if (!pair.Current)
                    {
                        highlightObjectVariable.SetValue(null);
                        //ClearBackdrop();
                    }
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