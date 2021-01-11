using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    [RequireComponent(typeof(Animator))]
    public class UIFadeoutController : MonoBehaviour
    {
        public EventGroup beginFadeEvent;
        public string fadeoutTrigger;

        private float lastAnimatorTrigger;
        private bool reachedHalfway;
        public EventGroup CompleteFadeoutReached;
        public float totalFadeoutAnimationLength;
        private void Start()
        {
            beginFadeEvent.OnEvent += BeginFadeEvent_OnEvent;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            beginFadeEvent.OnEvent -= BeginFadeEvent_OnEvent;
        }

        private void BeginFadeEvent_OnEvent()
        {
            var animator = GetComponent<Animator>();
            gameObject.SetActive(true);
            lastAnimatorTrigger = Time.time;
            animator.SetTrigger(fadeoutTrigger);
        }

        private void Update()
        {
            var timeSinceTrigger = Time.time - lastAnimatorTrigger;
            if (timeSinceTrigger >= totalFadeoutAnimationLength)
            {
                reachedHalfway = false;
                gameObject.SetActive(false);
            }
            else if (!reachedHalfway && timeSinceTrigger >= totalFadeoutAnimationLength / 2)
            {
                reachedHalfway = true;
                CompleteFadeoutReached.TriggerEvent();
            }
        }
    }
}