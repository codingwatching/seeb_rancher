using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [RequireComponent(typeof(Collider))]
    public class PlantContainer : MonoBehaviour, ISpawnable, IManipulatorClickReciever
    {
        public PlantType plantType;

        [SerializeField]
        [HideInInspector]
        private float growth;
        public float Growth => growth;

        public IntReference levelPhase;
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
            UpdateGrowth(plantType.AddGrowth(phaseDiff, growth));
        }

        public void SetupAfterSpawn()
        {
            UpdateGrowth(Mathf.Clamp(Random.Range(0, 1.2f), 0, 1));
        }

        public void UpdateGrowth(float newGrowth)
        {
            var lastPrefab = plantType.getPrefabForGrowth(growth);
            var newPrefab = plantType.getPrefabForGrowth(newGrowth);
            if (lastPrefab != newPrefab)
            {
                for (int i = transform.childCount; i > 0; --i)
                {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
                Instantiate(newPrefab, transform);
            }
            growth = newGrowth;

            var collider = GetComponent<Collider>();
            collider.enabled = growth >= 1 - 1e-5;
        }

        public void SelfHit(RaycastHit hit)
        {
            if (growth < 1 - 1e-5)
            {
                Debug.LogError("Non-grown plant clicked");
                return;
            }
            UpdateGrowth(0);
        }
    }
}
