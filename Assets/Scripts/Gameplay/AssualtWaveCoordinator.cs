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
        public float timeBetweenWaves = 64f;
        private float timeRemainingTillPhaseCompletion = -1;
        private float timeRemainingTillForcedWave = -1;


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
            this.timeRemainingTillPhaseCompletion = waveLength;
            timeRemainingTillForcedWave = -1;
        }
        private void WaveEnded()
        {
            timeRemainingTillForcedWave = timeBetweenWaves;
            timeRemainingTillPhaseCompletion = -1;
        }

        private void Update()
        {
            if (timeRemainingTillPhaseCompletion > -1)
            {
                timeRemainingTillPhaseCompletion -= Time.deltaTime * simulationSpeed.CurrentValue;
                if (timeRemainingTillPhaseCompletion <= 0)
                {
                    isWaveActive.SetValue(false);
                }
            }else if(timeRemainingTillForcedWave > -1)
            {
                timeRemainingTillForcedWave -= Time.deltaTime * simulationSpeed.CurrentValue;
                if (timeRemainingTillForcedWave <= 0)
                {
                    isWaveActive.SetValue(true);
                }
            }
        }

        #region Saving
        [System.Serializable]
        class LevelStateSaved
        {
            float timeTillWaveOver;
            public LevelStateSaved(AssualtWaveCoordinator source)
            {
                timeTillWaveOver = source.timeRemainingTillPhaseCompletion;
            }

            public void Apply(AssualtWaveCoordinator target)
            {
                target.timeRemainingTillPhaseCompletion = timeTillWaveOver;
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