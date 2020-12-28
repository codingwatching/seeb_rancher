using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Assets.Scripts.UI.SeedInventory;
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

        private Collider harvestCollider => GetComponent<Collider>();
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
            if (plantType == null)
                return;
            UpdateGrowth(plantType.AddGrowth(phaseDiff, growth));
        }

        public void SetupAfterSpawn()
        {
            UpdateGrowth(Mathf.Clamp(Random.Range(0, 1.2f), 0, 1));
        }

        public void UpdateGrowth(float newGrowth)
        {
            if (plantType == null)
            {
                return;
            }
            var lastPrefab = plantType.GetPrefabForGrowth(growth);
            var newPrefab = plantType.GetPrefabForGrowth(newGrowth);
            if (lastPrefab != newPrefab)
            {
                gameObject.DestroyAllChildren();
                Instantiate(newPrefab, transform);
            }
            growth = newGrowth;

            harvestCollider.enabled = growth >= 1 - 1e-5;
        }

        public void SelfHit(RaycastHit hit)
        {
            if (growth < 1 - 1e-5)
            {
                Debug.LogError("Non-grown plant clicked");
                return;
            }
            HarvestPlant();
        }

        private void HarvestPlant()
        {
            var draggingProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
            draggingProvider.TryAddToSeed(plantType.HarvestSeeds());

            growth = 0;
            plantType = null;
            harvestCollider.enabled = false;
            gameObject.DestroyAllChildren();
        }
    }
}
