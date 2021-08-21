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

        public float waveTime = 32f;
        private float timeRemainingTillPhaseCompletion = -1;


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
                    }
                }).AddTo(this);
        }

        private void OnDestroy()
        {
        }

        private void WaveTriggered()
        {
            this.timeRemainingTillPhaseCompletion = waveTime;
        }

        private void Update()
        {
            if (timeRemainingTillPhaseCompletion <= -1)
            {
                return;
            }
            timeRemainingTillPhaseCompletion -= Time.deltaTime * simulationSpeed.CurrentValue;
            if (timeRemainingTillPhaseCompletion <= 0)
            {
                timeRemainingTillPhaseCompletion = -1;
                isWaveActive.SetValue(false);
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