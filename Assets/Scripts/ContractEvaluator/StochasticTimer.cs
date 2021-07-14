
using UnityEngine;

namespace Assets.Scripts.ContractEvaluator
{
    [System.Serializable]
    public class StochasticTimer
    {
        /// <summary>
        /// average frequency of planting in Hz
        /// </summary>
        [Range(float.Epsilon, 10)]
        public float frequency;
        /// <summary>
        /// random variance applied to planting frequency, in percentage points
        /// </summary>
        [Range(float.Epsilon, 1)]
        public float frequencyVariance;

        private float nextTriggerTime;

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
