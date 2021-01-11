using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    public class BackdropUIGameObject : MonoBehaviour
    {
        public GameObjectVariable UISpriteVariableToBackdrop;
        public RectTransform highlightObject;
        public RectTransform backdropObject;

        private RectTransform placeholderObject;
        private RectTransform movedTargetObj;

        public RectTransform placeholderPrefab;

        private void Awake()
        {
            UISpriteVariableToBackdrop.Value
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    var nextRect = nextValue?.GetComponent<RectTransform>();
                    if (nextRect == null)
                    {
                        ClearBackdrop();
                    }
                    else
                    {
                        SetBackdrop(nextRect);
                    }
                })
                .AddTo(this);
        }

        private void SetBackdrop(RectTransform target)
        {
            highlightObject.gameObject.SetActive(true);
            backdropObject.gameObject.SetActive(true);

            movedTargetObj = target;
            var originalIndex = movedTargetObj.GetSiblingIndex();
            placeholderObject = Instantiate(placeholderPrefab, target.parent);

            var realCorners = new Vector3[4];
            target.GetWorldCorners(realCorners);
            highlightObject.sizeDelta = new Vector2(realCorners[3].x - realCorners[0].x, realCorners[1].y - realCorners[0].y);
            var center = (realCorners[0] + realCorners[2]) / 2;
            highlightObject.transform.position = new Vector3(center.x, center.y, center.z);

            //highlightObject.anchorMin = movedTargetObj.anchorMin;
            //highlightObject.anchorMax = movedTargetObj.anchorMax;
            //highlightObject.position = movedTargetObj.position;
            //highlightObject.sizeDelta = movedTargetObj.sizeDelta;

            movedTargetObj.SetParent(highlightObject, true);
            placeholderObject.transform.SetSiblingIndex(originalIndex);
        }
        private void ClearBackdrop()
        {
            if (placeholderObject != null && movedTargetObj != null)
            {
                var targetIndex = placeholderObject.GetSiblingIndex();
                movedTargetObj.SetParent(placeholderObject.parent, true);
                Destroy(placeholderObject.gameObject);
                placeholderObject = null;
                movedTargetObj.SetSiblingIndex(targetIndex);
            }

            highlightObject.gameObject.SetActive(false);
            backdropObject.gameObject.SetActive(false);
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