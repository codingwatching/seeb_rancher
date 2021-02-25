using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public abstract class PlantFormDefinition : ScriptableObject
    {
        public abstract void BuildPlant(
            Transform plantParent,
            BasePlantType plantType,
            CompiledGeneticDrivers geneticDrivers,
            PlantState plantState,
            PollinationState pollination);
    }
}
