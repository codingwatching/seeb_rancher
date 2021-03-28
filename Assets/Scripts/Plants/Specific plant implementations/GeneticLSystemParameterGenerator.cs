using UnityEngine;
using System.Collections;
using Dman.LSystem.UnityObjects;
using System.Collections.Generic;

namespace Assets.Scripts.Plants.Specific_plant_implementations
{
    public class GeneticLSystemParameterGenerator : LSystemCompileTimeParameterGenerator
    {
        public LSystemPlantType plantType;
        public override Dictionary<string, string> GenerateCompileTimeParameters()
        {
            var newGenome = plantType.genome.GenerateBaseGenomeData();
            var compiledDrivers = plantType.genome.CompileGenome(newGenome);
            var paramters = plantType.CompileToGlobalParameters(compiledDrivers);
            return paramters;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}