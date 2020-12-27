using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public class PlantContainer : MonoBehaviour, ISpawnable
    {
        public GameObject[] growthStagePrefabs;
        public GameObject harvestedPrefab;

        [SerializeField]
        [HideInInspector]
        private float growth;
        public float Growth => growth;

        public IntReference levelPhase;
        public float growthPerPhase;
        private void Start()
        {
            levelPhase.ValueChanges
                .TakeUntilDisable(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    AdvanceGrowPhase(pair.Current - pair.Previous);
                }).AddTo(this);
        }

        private void AdvanceGrowPhase(int phaseDiff)
        {
            var extraGrowth = growthPerPhase * phaseDiff;
            UpdateGrowth(Mathf.Clamp(growth + extraGrowth, 0, 1));
        }

        public void SetupAfterSpawn()
        {
            UpdateGrowth(Mathf.Clamp(Random.Range(0, 1.2f), 0, 1));
        }

        public void UpdateGrowth(float newGrowth)
        {
            var lastIndex = getPrefabIndex(growth);
            var newIndex = getPrefabIndex(newGrowth);
            if (lastIndex != newIndex)
            {
                for (int i = transform.childCount; i > 0; --i)
                {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
                Instantiate(growthStagePrefabs[newIndex], transform);
            }
            growth = newGrowth;
        }

        private int getPrefabIndex(float growth)
        {
            return Mathf.FloorToInt(growth * (growthStagePrefabs.Length - 1));
        }
    }
}
