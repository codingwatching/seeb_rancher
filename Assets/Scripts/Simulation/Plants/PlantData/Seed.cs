using Genetics;
using Genetics.GeneticDrivers;
using Simulation.Plants.PlantTypes;
using System;

namespace Simulation.Plants.PlantData
{
    [Serializable]
    public class Seed
    {
        public int plantType;
        public Genome genes;
        public CompiledGeneticDrivers parentAttributes;
        public Seed(Genome genes, BasePlantType plantType, CompiledGeneticDrivers parentAttributes) : this(genes, plantType.myId, parentAttributes)
        {
        }
        public Seed(Genome genes, int plantType, CompiledGeneticDrivers parentAttributes)
        {
            this.genes = genes;
            this.plantType = plantType;
            this.parentAttributes = parentAttributes;
        }
    }
}
