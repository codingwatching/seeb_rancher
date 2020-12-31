using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "TomatoBuilder", menuName = "Greenhouse/PlantBuilders/TomatoBuilder", order = 1)]
    public class TomatoBuilder : PlantBuilder
    {
        public GameObject[] growthStagePrefabs;
        public GameObject harvestedPrefab;

        public GameObject largeTomatos;
        public GameObject smallTomatos;

        public GameObject flower;
        public GameObject stamens;

        public GeneticDrivenModifier[] geneticModifiers;

        public BooleanGeneticDriver largeOrSmallSwitch;

        public override void BuildPlant(PlantContainer plantParent, CompiledGeneticDrivers geneticDrivers)
        {
            var prefabIndex = GetPrefabIndex(plantParent.Growth);
            var newPlant = plantParent.SpawnPlant(growthStagePrefabs[prefabIndex]);

            if (prefabIndex == growthStagePrefabs.Length - 1)
            {
                if (geneticDrivers.TryGetGeneticData(largeOrSmallSwitch, out var isLarge))
                {
                    if (isLarge)
                    {
                        Instantiate(largeTomatos, newPlant.transform.parent);
                    }
                    else
                    {
                        Instantiate(smallTomatos, newPlant.transform.parent);
                    }
                }
                else
                    Debug.LogError($"Genetic driver unset: {largeOrSmallSwitch}");
            }
            if (plantParent.plantType.IsInPollinationRange(plantParent.Growth))
            {
                Instantiate(flower, newPlant.transform.parent);
                if (plantParent.polliationState.HasAnther)
                {
                    Instantiate(stamens, newPlant.transform.parent);
                }
            }
            newPlant.transform.parent.Rotate(Vector3.up, Random.Range(0f, 360f));
            ApplyGeneticModifiers(newPlant.transform.parent.gameObject, geneticDrivers);
        }
        private int GetPrefabIndex(float growth)
        {
            return Mathf.FloorToInt(growth * (growthStagePrefabs.Length - 1));
        }
        private void ApplyGeneticModifiers(GameObject plant, CompiledGeneticDrivers geneticDrivers)
        {
            foreach (var geneticModifier in geneticModifiers)
            {
                geneticModifier.ModifyObject(plant, geneticDrivers);
            }
        }
    }
}
