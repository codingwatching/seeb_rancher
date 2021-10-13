using Dman.ReactiveVariables;
using Environment;
using System;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class SpawnableConfiguration
    {
        public GameObject spawned;
        public StochasticTimerFrequencyVaried spawnRate;
        public AnimationCurve spawnRateAccelerationByWave;

        public GameObject TrySpawn(int wave)
        {
            var spawnSpeed = spawnRateAccelerationByWave.Evaluate(wave);
            if (spawnSpeed > 0 && spawnRate.Tick(spawnSpeed))
            {
                return GameObject.Instantiate(spawned);
            }
            return null;
        }

    }

    public class AssaultWaveSpawner : MonoBehaviour
    {
        public BooleanVariable isWaveActive;
        public IntReference currentWave;

        public Vector3 spawnSize;

        public SpawnableConfiguration[] spawnables;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
        }

        private void Update()
        {
            if (!isWaveActive.CurrentValue)
            {
                return;
            }
            foreach (var spawnable in spawnables)
            {
                var spawned = spawnable.TrySpawn(currentWave.CurrentValue);
                if (spawned != null)
                {
                    spawned.transform.position = GetRandomSpawnPosition();
                }
            }
        }
        private Vector3 GetRandomSpawnPosition()
        {
            return new Vector3(
                UnityEngine.Random.Range(-spawnSize.x / 2, spawnSize.x / 2),
                UnityEngine.Random.Range(-spawnSize.y / 2, spawnSize.y / 2),
                UnityEngine.Random.Range(-spawnSize.z / 2, spawnSize.z / 2))
                + transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(.5f, .5f, .5f, .5f);
            Gizmos.DrawCube(transform.position, spawnSize);
        }

    }
}