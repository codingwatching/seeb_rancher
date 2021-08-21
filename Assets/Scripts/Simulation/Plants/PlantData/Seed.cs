using Genetics;
using Genetics.GeneticDrivers;
using System;

namespace Assets.Scripts.DataModels
{
    [Serializable]
    public class Seed
    {
        public int plantType;
        public Genome genes;
        public CompiledGeneticDrivers parentAttributes;
        public Seed(Genome genes, Plants.BasePlantType plantType, CompiledGeneticDrivers parentAttributes) : this(genes, plantType.myId, parentAttributes)
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
