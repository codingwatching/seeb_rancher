using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
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
                    narrative.CheckAllConversationTriggers();
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
