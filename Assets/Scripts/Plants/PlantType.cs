using Assets.Scripts.DataModels;
using Assets.Scripts.Utilities;
using Genetics;
using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "PlantType", menuName = "Greenhouse/PlantType", order = 1)]
    public class PlantType : IDableObject
    {
        public GenomeEditor genome;
        public GeneticDrivenModifier[] geneticModifiers;
        [Header("Growth")]
        public GameObject[] growthStagePrefabs;
        public GameObject harvestedPrefab;
        public float growthPerPhase;

        [Header("Seebs")]
        public Sprite seedIcon;
        public int minSeeds;
        public int maxSeeds;

        public int plantID;

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

        public void ApplyGeneticModifiers(GameObject plant, CompiledGeneticDrivers geneticDrivers)
        {

            foreach (var geneticModifier in geneticModifiers)
            {
                geneticModifier.ModifyObject(plant, geneticDrivers);
            }
        }

        public Seed[] HarvestSeeds(Seed sourceSeed)
        {
            var generatedSeeds = Random.Range(minSeeds, maxSeeds);
            var seedResult = new Seed[generatedSeeds];
            for (int i = 0; i < seedResult.Length; i++)
            {
                seedResult[i] = new Seed
                {
                    plantType = plantID,
                    genes = sourceSeed.genes
                };
            }
            return seedResult;
        }
    }
}
