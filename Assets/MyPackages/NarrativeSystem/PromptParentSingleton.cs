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
        public IntVariable phaseVariable;

        public static PromptParentSingleton Instance;

        private void Awake()
        {
            narrative.Init();
            phaseVariable.Value
                .TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current - pair.Previous != 1)
                    {
                        return;
                    }
                    StartCoroutine(CheckTriggersOnNextFrame());
                }).AddTo(this);
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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

        public ISaveableData[] GetDependencies()
        {
            return ((ISaveableData)narrative).GetDependencies();
        }
    }
}
