
using UnityEngine;

namespace Assets.Scripts.ContractEvaluator
{
    [System.Serializable]
    public class StochasticTimerFrequencyVaried
    {
        /// <summary>
        /// average frequency of planting in Hz
        /// </summary>
        [Range(float.Epsilon, 10)]
        public float frequency = 1;
        /// <summary>
        /// random variance applied to planting frequency, in percentage points
        /// </summary>
        [Range(0, 1 - float.Epsilon)]
        public float frequencyVariance = 0.1f;

        private float nextTriggerTime;

        public StochasticTimerFrequencyVaried(StochasticTimerFrequencyVaried other)
        {
            this.frequency = other.frequency;
            this.frequencyVariance = other.frequencyVariance;
            this.Reset();
        }

        public void Reset()
        {
            nextTriggerTime = 0;
        }

        public bool Tick()
        {
            if (Time.time < nextTriggerTime)
            {
                return false;
            }

            var nextFrequency = Random.Range(frequency * (1 - frequencyVariance), frequency * (1 + frequencyVariance));

            nextTriggerTime = Time.time + (1 / nextFrequency);
            return true;
        }
    }
}
