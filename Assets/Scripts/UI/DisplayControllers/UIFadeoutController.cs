using Dman.ReactiveVariables;
using UnityEngine;

namespace UI.DisplayControllers
{
    public class UIFadeoutController : MonoBehaviour
    {
        public EventGroup beginFadeEvent;
        public string fadeoutTrigger;


        public GameObject fadingGameObject;
        private float lastAnimatorTrigger;
        private bool reachedHalfway;
        public EventGroup CompleteFadeoutReached;
        public float totalFadeoutAnimationLength;


        private void Awake()
        {
            beginFadeEvent.OnEvent += BeginFadeEvent_OnEvent;
            fadingGameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            beginFadeEvent.OnEvent -= BeginFadeEvent_OnEvent;
        }

        private void BeginFadeEvent_OnEvent()
        {
            var animator = fadingGameObject.GetComponent<Animator>();
            fadingGameObject.SetActive(true);
            lastAnimatorTrigger = Time.unscaledTime;
            animator.SetTrigger(fadeoutTrigger);
        }

        private void Update()
        {
            if (!fadingGameObject.activeSelf)
            {
                return;
            }
            var timeSinceTrigger = Time.unscaledTime - lastAnimatorTrigger;
            if (timeSinceTrigger >= totalFadeoutAnimationLength)
            {
                reachedHalfway = false;
                fadingGameObject.SetActive(false);
            }
            else if (!reachedHalfway && timeSinceTrigger >= totalFadeoutAnimationLength / 2)
            {
                reachedHalfway = true;
                CompleteFadeoutReached.TriggerEvent();
            }
        }
    }
}