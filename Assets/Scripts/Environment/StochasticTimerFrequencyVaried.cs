
using UnityEngine;

namespace Environment
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

        private float lastTriggerTime;
        private float currentDelayTillNextTrigger;

        public StochasticTimerFrequencyVaried(StochasticTimerFrequencyVaried other)
        {
            this.frequency = other.frequency;
            this.frequencyVariance = other.frequencyVariance;
            this.Reset();
        }

        public void Reset()
        {
            this.SetNextTriggerTime();
        }

        public bool Tick(float tickSpeedMultiplier = 1)
        {
            var actualDelayTillNextTrigger = currentDelayTillNextTrigger / tickSpeedMultiplier;
            if (Time.time < lastTriggerTime + actualDelayTillNextTrigger)
            {
                return false;
            }
            this.SetNextTriggerTime();
            return true;
        }

        private void SetNextTriggerTime()
        {
            var nextFrequency = Random.Range(frequency * (1 - frequencyVariance), frequency * (1 + frequencyVariance));

            currentDelayTillNextTrigger = (1 / nextFrequency);
            lastTriggerTime = Time.time;
        }

        public float TimeTillNextTrigger()
        {
            return currentDelayTillNextTrigger;
        }
    }
}
