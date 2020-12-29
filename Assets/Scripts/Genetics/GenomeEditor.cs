using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public class Genome
    {
        public GeneData[] allGeneData;

        public static Genome GetBaseGenes(GeneEditor[] geneGenerators)
        {
            return new Genome
            {
                allGeneData = geneGenerators.Select(x => x.GenerateGeneData()).ToArray()
            };
        }
    }

    [CreateAssetMenu(fileName = "Genome", menuName = "Genetics/Genome", order = 1)]
    public class GenomeEditor : ScriptableObject
    {
        public GeneEditor[] genes;

        public Genome GenerateBaseGenomeData()
        {
            return Genome.GetBaseGenes(genes);
        }

        public CompiledGeneticDrivers CompileGenome(Genome genomeData)
        {
            var drivers = new CompiledGeneticDrivers();
            if (genomeData.allGeneData.Length != genes.Length)
            {
                Debug.LogError($"genome does not match current genes! Genome data size: {genomeData.allGeneData.Length}, current gene size: {genes.Length}. Resetting genome data");
            }
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                var gene = genes[geneIndex];
                var geneData = genomeData.allGeneData[geneIndex];
                gene.Evaluate(geneData, drivers);
            }

            drivers.Lock();
            return drivers;
        }
    }
}