using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using UniRx;
using UnityEngine;

namespace Gameplay
{
    public class AssualtWaveCoordinator : MonoBehaviour, ISaveableData
    {
        public static AssualtWaveCoordinator instance;

        public BooleanVariable isWaveActive;
        public FloatReference simulationSpeed;

        public float waveLength = 32f;
        public float waveCycleTotalLength = (64f + 32f);

        public FloatReference timeInsideWaveCycle;

        private float WaveStartTimeInsideCycle => waveCycleTotalLength - waveLength;
        private float RecoveryStartTimeInsideCycle => 0;


        public string UniqueSaveIdentifier => "AssualtWaveCoordinator";


        private void Awake()
        {
            instance = this;

            isWaveActive.Value.TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current == true && pair.Current != pair.Previous)
                    {
                        this.WaveTriggered();
                    }else if (pair.Current == false && pair.Current != pair.Previous)
                    {
                        this.WaveEnded(); 
                    }
                }).AddTo(this);

            this.WaveEnded();
        }

        private void OnDestroy()
        {
        }

        private void WaveTriggered()
        {
            this.timeInsideWaveCycle.SetValue(WaveStartTimeInsideCycle);
        }
        private void WaveEnded()
        {
            this.timeInsideWaveCycle.SetValue(RecoveryStartTimeInsideCycle);
        }

        private void Update()
        {
            var oldWaveActive = IsWaveActiveAtTime(timeInsideWaveCycle.CurrentValue);
            var newValue = (timeInsideWaveCycle.CurrentValue + Time.deltaTime * simulationSpeed.CurrentValue) % waveCycleTotalLength;
            timeInsideWaveCycle.SetValue(newValue);

            var nextWaveActive = IsWaveActiveAtTime(newValue);
            if(oldWaveActive ^ nextWaveActive)
            {
                isWaveActive.SetValue(nextWaveActive);
            }
        }
        
        private bool IsWaveActiveAtTime(float currentTimeInWave)
        {
            var standardTime = currentTimeInWave % waveCycleTotalLength;
            return standardTime >= WaveStartTimeInsideCycle;
        }


        #region Saving
        [System.Serializable]
        class LevelStateSaved
        {
            public LevelStateSaved(AssualtWaveCoordinator source)
            {
            }

            public void Apply(AssualtWaveCoordinator target)
            {
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