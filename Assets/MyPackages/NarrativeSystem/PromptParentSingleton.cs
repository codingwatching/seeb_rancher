using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    public class PromptParentSingleton : MonoBehaviour, ISaveableData
    {
        public GameNarrative narrative;
        public EventGroup conversationCheckTrigger;

        public static PromptParentSingleton Instance;

        private void Awake()
        {
            conversationCheckTrigger.OnEvent += ConversationCheckTriggered;
            narrative.Init();
            Instance = this;
        }
        private void OnDestroy()
        {
            conversationCheckTrigger.OnEvent -= ConversationCheckTriggered;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ConversationCheckTriggered()
        {
            StartCoroutine(CheckTriggersOnNextFrame());
        }

        IEnumerator CheckTriggersOnNextFrame()
        {
            yield return new WaitForEndOfFrame();
            narrative.CheckAllConversationTriggers();
        }

        public string UniqueSaveIdentifier => ((ISaveableData)narrative).UniqueSaveIdentifier;
        public object GetSaveObject()
        {
            return ((ISaveableData)narrative).GetSaveObject();
        }

        public void SetupFromSaveObject(object save)
        {
            ((ISaveableData)narrative).SetupFromSaveObject(save);
        }
    }
}
