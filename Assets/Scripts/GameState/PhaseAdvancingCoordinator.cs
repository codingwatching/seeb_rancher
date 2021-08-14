using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System;
using System.Linq;
using UnityEngine;

using UniRx;

namespace Assets.Scripts.GreenhouseLoader
{
    public class PhaseAdvancingCoordinator : MonoBehaviour, ISaveableData
    {
        public static PhaseAdvancingCoordinator instance;

        public BooleanVariable isPhaseTransitionActive;
        public FloatReference simulationSpeed;

        public float secondTillPhaseCompletion = 32f;
        private float timeRemainingTillPhaseCompletion = -1;


        public string UniqueSaveIdentifier => "PhaseAdvancingCoordinator";


        private void Awake()
        {
            instance = this;

            isPhaseTransitionActive.Value.TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current == true && pair.Current != pair.Previous)
                    {
                        this.PhaseTransitionTriggered();
                    }
                }).AddTo(this);
        }

        private void OnDestroy()
        {
        }

        private void PhaseTransitionTriggered()
        {
            this.timeRemainingTillPhaseCompletion = secondTillPhaseCompletion;
        }

        private void Update()
        {
            if(timeRemainingTillPhaseCompletion <= -1)
            {
                return;
            }
            timeRemainingTillPhaseCompletion -= Time.deltaTime * simulationSpeed.CurrentValue;
            if (timeRemainingTillPhaseCompletion <= 0)
            {
                timeRemainingTillPhaseCompletion = -1;
                isPhaseTransitionActive.SetValue(false);
            }
        }

        #region Saving
        [System.Serializable]
        class LevelStateSaved
        {
            float timeTillPhaseCompletion;
            public LevelStateSaved(PhaseAdvancingCoordinator source)
            {
                timeTillPhaseCompletion = source.timeRemainingTillPhaseCompletion;
            }

            public void Apply(PhaseAdvancingCoordinator target)
            {
                target.timeRemainingTillPhaseCompletion = timeTillPhaseCompletion;
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