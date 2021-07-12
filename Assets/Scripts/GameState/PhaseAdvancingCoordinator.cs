using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    public class PhaseAdvancingCoordinator : MonoBehaviour, ISaveableData
    {
        public static PhaseAdvancingCoordinator instance;

        public EventGroup beginPhaseTransition;
        public EventGroup phaseTransitionCompleted;

        public float defaultTimeTillPhaseCompletion = 3f;
        private float timeTillPhaseCompletion = -1;


        public string UniqueSaveIdentifier => "PhaseAdvancingCoordinator";


        private void Awake()
        {
            instance = this;
            this.beginPhaseTransition.OnEvent += PhaseTransitionTriggered;
        }

        private void OnDestroy()
        {
            this.beginPhaseTransition.OnEvent -= PhaseTransitionTriggered;
        }

        private void PhaseTransitionTriggered()
        {
            this.timeTillPhaseCompletion = defaultTimeTillPhaseCompletion;
        }

        /// <summary>
        /// called from the l-systems which are updating themselves
        ///     will ensure that the phase will not complete advancing for the next
        ///     <paramref name="delayTime"/> seconds
        /// </summary>
        public void DelayPhaseComplete(float delayTime)
        {
            if(timeTillPhaseCompletion <= -1)
            {
                Debug.LogError("trying to delay phase completion when phase is not transitioning");
                return;
            }

            timeTillPhaseCompletion = Mathf.Max(timeTillPhaseCompletion, delayTime);
        }

        private void Update()
        {
            if(timeTillPhaseCompletion <= -1)
            {
                return;
            }
            timeTillPhaseCompletion -= Time.deltaTime;
            if(timeTillPhaseCompletion <= 0)
            {
                timeTillPhaseCompletion = -1;
                phaseTransitionCompleted.TriggerEvent();
            }
        }

        #region Saving
        [System.Serializable]
        class LevelStateSaved
        {
            float timeTillPhaseCompletion;
            public LevelStateSaved(PhaseAdvancingCoordinator source)
            {
                timeTillPhaseCompletion = source.timeTillPhaseCompletion;
            }

            public void Apply(PhaseAdvancingCoordinator target)
            {
                target.timeTillPhaseCompletion = timeTillPhaseCompletion;
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