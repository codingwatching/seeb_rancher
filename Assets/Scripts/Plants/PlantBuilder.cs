using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public abstract class PlantBuilder : ScriptableObject
    {
        public abstract void BuildPlant(PlantContainer plantParent, CompiledGeneticDrivers geneticDrivers);
    }
}
