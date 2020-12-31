using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public class Genome
    {
        public Chromosome[] allChromosomes;

        public static Genome GetBaseGenes(ChromosomeEditor[] chromosomeEditors)
        {
            return new Genome
            {
                allChromosomes = chromosomeEditors.Select(x => x.GenerateChromosomeData()).ToArray()
            };
        }

        /// <summary>
        /// Select all genes randomly from the breeding genomes
        /// </summary>
        /// <param name="breedingGenome"></param>
        public Genome(params Genome[] breedingGenome)
        {
            if(breedingGenome.Length == 0)
            {
                return;
            }

            allChromosomes = new Chromosome[breedingGenome[0].allChromosomes.Length];
            if(breedingGenome.Any(x => x.allChromosomes.Length != allChromosomes.Length))
            {
                throw new System.ArgumentException("breeding genomes must have equal number of chromosomes");
            }
            for (int chromosomeIndex = 0; chromosomeIndex < allChromosomes.Length; chromosomeIndex++)
            {
                // generate a new chromosome, using all copies of this chromosome
                allChromosomes[chromosomeIndex] = new Chromosome(breedingGenome.Select(x => x.allChromosomes[chromosomeIndex]).ToArray());
            }
        }
    }

    [CreateAssetMenu(fileName = "Genome", menuName = "Genetics/Genome", order = 1)]
    public class GenomeEditor : ScriptableObject
    {
        public ChromosomeEditor[] chromosomes;

        public GeneEditor[] geneInterpretors;

        public Genome GenerateBaseGenomeData()
        {
            return Genome.GetBaseGenes(chromosomes);
        }

        public CompiledGeneticDrivers CompileGenome(Genome genomeData)
        {
            var drivers = new CompiledGeneticDrivers();

            if(genomeData.allChromosomes.Length != chromosomes.Length) { 
                Debug.LogError($"Chromosome number mismatch! Chromosomes in data: {genomeData.allChromosomes.Length}, current chromosome count: {chromosomes.Length}.");
            }
            if(geneInterpretors.Any(x => x.GeneSize > 0))
            {
                Debug.LogError($"All genes with any backing genetic information must be in a chromosome. ${geneInterpretors.First(x => x.GeneSize > 0)} has a non-zero gene size");
            }

            for (int chromosomeIndex = 0; chromosomeIndex < chromosomes.Length; chromosomeIndex++)
            {
                var chromosome = chromosomes[chromosomeIndex];
                var chromosomeData = genomeData.allChromosomes[chromosomeIndex];
                chromosome.CompileChromosomeIntoDrivers(chromosomeData, drivers);
            }

            foreach (var interpretor in geneInterpretors)
            {
                interpretor.Evaluate(drivers, new GeneCopies[0]);
            }

            drivers.Lock();
            return drivers;
        }
    }
}