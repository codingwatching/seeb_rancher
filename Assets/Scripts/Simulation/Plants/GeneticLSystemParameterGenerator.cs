using Dman.LSystem.UnityObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    /// <summary>
    /// this is used when developing l-systems, to render plants in realtime. not during gameplay
    /// </summary>
    public class GeneticLSystemParameterGenerator : MonoBehaviour, ILSystemCompileTimeParameterGenerator
    {
        public LSystemPlantType plantType;
        public Dictionary<string, string> GenerateCompileTimeParameters()
        {
            var newGenome = plantType.genome.GenerateBaseGenomeData(new System.Random(Random.Range(int.MinValue, int.MaxValue)));
            var compiledDrivers = plantType.genome.CompileGenome(newGenome);
            var paramters = plantType.CompileToGlobalParameters(compiledDrivers);
            return paramters;
        }
    }
}