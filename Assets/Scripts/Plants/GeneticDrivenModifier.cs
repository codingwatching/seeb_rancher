using Genetics.GeneticDrivers;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    public abstract class GeneticDrivenModifier : ScriptableObject
    {
        public abstract void ModifyObject(GameObject target, CompiledGeneticDrivers geneticDrivers);
    }
}
