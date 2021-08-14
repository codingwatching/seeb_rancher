using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using System;
using System.Linq;
using UnityEngine;

using UniRx;
using Assets.Scripts.ContractEvaluator;

namespace Assets.Scripts.GreenhouseLoader
{
    [Serializable]
    public class SpawnableConfiguration
    {
        public GameObject spawned;
        public StochasticTimerFrequencyVaried spawnRate;
        public AnimationCurve spawnRateAccelerationByWave;

        public GameObject TrySpawn(int wave, float simSpeed)
        {
            var spawnSpeed = spawnRateAccelerationByWave.Evaluate(wave);
            if (spawnSpeed > 0 && spawnRate.Tick(spawnSpeed * simSpeed))
            {
                return GameObject.Instantiate(spawned);
            }
            return null;
        }
        
    }

    public class AssaultWaveSpawner : MonoBehaviour
    {
        public BooleanVariable isWaveActive;
        public FloatVariable gameSpeed;
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
                var spawned = spawnable.TrySpawn(currentWave.CurrentValue, gameSpeed.CurrentValue);
                if(spawned != null)
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