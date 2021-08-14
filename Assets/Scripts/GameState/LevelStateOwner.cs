using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    public class LevelStateOwner : MonoBehaviour, ISaveableData
    {
        public LevelState levelState;
        public BooleanVariable[] savedBooleans;

        public string UniqueSaveIdentifier => "LevelState";

        public void AdvanceWave()
        {
            levelState.AdvanceWave();
        }

        #region Saving
        [System.Serializable]
        class LevelStateSaved
        {
            int phase;
            float money;
            bool[] booleanSaved;
            public LevelStateSaved(LevelStateOwner source)
            {
                phase = source.levelState.currentWave.CurrentValue;
                money = source.levelState.money.CurrentValue;
                booleanSaved = source.savedBooleans.Select(x => x.CurrentValue).ToArray();
            }

            public void Apply(LevelStateOwner target)
            {
                target.levelState.currentWave.SetValue(phase);
                target.levelState.money.SetValue(money);
                if (target.savedBooleans.Length != booleanSaved.Length)
                {
                    Debug.LogWarning("saved booleans of different length than saved variables. all defaulting to previous value");
                }
                else
                {
                    for (int i = 0; i < booleanSaved.Length; i++)
                    {
                        target.savedBooleans[i].SetValue(booleanSaved[i]);
                    }
                }
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
        #endregion
    }
}