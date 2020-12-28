using Assets.Scripts.UI.SeedInventory;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "PlantType", menuName = "Greenhouse/PlantType", order = 1)]
    public class PlantType : IDableObject
    {
        public int plantID;

        public GameObject[] growthStagePrefabs;
        public GameObject harvestedPrefab;
        public float growthPerPhase;

        public Sprite seedIcon;

        public int minSeeds;
        public int maxSeeds;

        public override void AssignId(int myNewID)
        {
            plantID = myNewID;
        }

        public float AddGrowth(int phaseDiff, float currentGrowth)
        {
            var extraGrowth = growthPerPhase * phaseDiff;
            return Mathf.Clamp(currentGrowth + extraGrowth, 0, 1);
        }

        public GameObject GetPrefabForGrowth(float growth)
        {
            var prefabIndex = Mathf.FloorToInt(growth * (growthStagePrefabs.Length - 1));
            return growthStagePrefabs[prefabIndex];
        }

        public Seed[] HarvestSeeds()
        {
            var generatedSeeds = Random.Range(minSeeds, maxSeeds);
            var seedResult = new Seed[generatedSeeds];
            for (int i = 0; i < seedResult.Length; i++)
            {
                seedResult[i] = new Seed
                {
                    plantType = this.plantID
                };
            }
            return seedResult;
        }
    }
}
