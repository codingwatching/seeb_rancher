using Assets.Scripts.Utilities.Core;
using Assets.Scripts.Utilities.SaveSystem.Components;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    public class LevelStateOwner : MonoBehaviour, ISaveableData
    {
        public LevelState levelState;
        public EventGroup phaseAdvanceTrigger;

        public string UniqueSaveIdentifier => "LevelState";

        private void Awake()
        {
            phaseAdvanceTrigger.OnEvent += AdvancePhase;
        }
        private void OnDestroy()
        {
            phaseAdvanceTrigger.OnEvent -= AdvancePhase;
        }

        private void AdvancePhase()
        {
            levelState.AdvancePhase();
        }

        [System.Serializable]
        class LevelStateSaved
        {
            int phase;
            public LevelStateSaved(LevelStateOwner source)
            {
                phase = source.levelState.currentPhase.CurrentValue;
            }

            public void Apply(LevelStateOwner target)
            {
                target.levelState.currentPhase.SetValue(phase);
            }
        }

        public object GetSaveObject()
        {
            return new LevelStateSaved(this);
        }

        public void SetupFromSaveObject(object save)
        {
            (save as LevelStateSaved).Apply(this);
        }

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }
    }
}