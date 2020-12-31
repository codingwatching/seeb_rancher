using Assets;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public class Chromosome
    {
        /// <summary>
        /// array to represent all genes, and copies of genes inside this chromosome
        /// first dimension is for each unique gene. each gene contains all of the
        ///     chromosomal copies of that same gene
        /// </summary>
        public GeneCopies[] allGeneData;

        public Chromosome(params Chromosome[] parentalChromosomes)
        {
            if(parentalChromosomes.Length == 0)
            {
                return;
            }

            var chromosomeSize = parentalChromosomes[0].allGeneData.Length;
            if (parentalChromosomes.Any(x => x.allGeneData.Length != chromosomeSize))
            {
                throw new System.ArgumentException("all chromosomes must be equal in size");
            }
            var chromosomalCopies = parentalChromosomes[0].allGeneData[0].chromosomalCopies.Length;
            var parentSelections = ArrayExt.SelectIndexSources(chromosomalCopies, parentalChromosomes.Length);
            var geneDataPivoted = parentSelections
                .Select(parentIndex => parentalChromosomes[parentIndex].SampleSingleCopyFromChromosome())
                .ToArray();
            allGeneData = GenePivot(geneDataPivoted);
        }

        /// <summary>
        /// take in a list of individual chromosomes, pivot them to be a list of genes
        ///     each gene containing all of it's copy data
        /// </summary>
        /// <param name="geneDataPivoted"></param>
        /// <returns></returns>
        private static GeneCopies[] GenePivot(SingleGene[][] geneDataPivoted)
        {
            var geneData = geneDataPivoted.Pivot();
            return geneData.Select(x => new GeneCopies
            {
                chromosomalCopies = x
            }).ToArray();
        }

        /// <summary>
        /// Get a single copy of every individual gene. This step simulates the recombination which happens during
        ///     meosis: taking all genes which were originally passed down from the parents, and combining them into
        ///     a single instance of each gene to be passed on to children
        /// </summary>
        /// <returns></returns>
        private SingleGene[] SampleSingleCopyFromChromosome()
        {
            var chromosomeCopies = allGeneData[0].chromosomalCopies.Length;
            return allGeneData.Select(geneCopies => geneCopies.chromosomalCopies[Random.Range(0, chromosomeCopies)]).ToArray();
        }

        public static Chromosome GetBaseGenes(ChromosomeEditor geneGenerators)
        {
            var pivotedGeneData = new SingleGene[geneGenerators.chromosomeCopies][];
            for (int chromosomeCopy = 0; chromosomeCopy < pivotedGeneData.Length; chromosomeCopy++)
            {
                pivotedGeneData[chromosomeCopy] = geneGenerators.genes.SelectMany(x => x.GenerateGeneData()).ToArray();
            }

            return new Chromosome
            {
                allGeneData = GenePivot(pivotedGeneData.Pivot())
            };
        }
    }

    [CreateAssetMenu(fileName = "Chromosome", menuName = "Genetics/Chromosome", order = 2)]
    public class ChromosomeEditor : ScriptableObject
    {
        public GeneEditor[] genes;
        public int chromosomeCopies = 2;

        public Chromosome GenerateChromosomeData()
        {
            return Chromosome.GetBaseGenes(this);
        }

        private int ChromosomeGeneticSize()
        {
            return genes.Sum(x => x.GeneSize);
        }

        public void CompileChromosomeIntoDrivers(Chromosome chromosome, CompiledGeneticDrivers drivers)
        {
            if (ChromosomeGeneticSize() != chromosome.allGeneData.Length)
            {
                Debug.LogError($"genome does not match current genes! Genome data size: {chromosome.allGeneData.Length}, current gene size: {genes.Length}. Resetting genome data");
            }
            var geneDataIndex = 0;
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                var gene = genes[geneIndex];
                var geneCount = gene.GeneSize;
                var geneData = chromosome.allGeneData.Skip(geneDataIndex).Take(geneCount).ToArray();
                geneDataIndex += geneCount;
                gene.Evaluate(drivers, geneData);
            }
        }
    }
}