using Genetics.GeneticDrivers;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public struct SingleGene
    {
        public ulong Value;
    }

    [System.Serializable]
    public class GeneCopies
    {
        public SingleGene[] chromosomalCopies;
    }

    /// <summary>
    /// Represents zero or one genes, and how the gene and other genetic drives should be interpreted
    /// </summary>
    public abstract class GeneEditor : ScriptableObject
    {
        /// <summary>
        /// Used to determine evaluation order of genes. return nothing if this gene is not effected by other genes
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<GeneticDriver> GetInputs();
        /// <summary>
        /// Used to determine evaluation order of genes. This indicates what data the gene editor will set
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<GeneticDriver> GetOutputs();

        public abstract int GeneSize { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorHandle"></param>
        /// <param name="data">gene data including chromosomal duplicates. first dimension is unique genes, length 
        ///     equal to <see cref="GeneSize"/>. Second dimension is duplicate copies of the chromosome</param>
        public abstract void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] data);

        public virtual SingleGene[] GenerateGeneData()
        {
            var result = new SingleGene[GeneSize];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = RandomGene();
            }
            return result;
        }

        private SingleGene RandomGene()
        {
            ulong part1 = ((ulong)Random.Range(int.MinValue, int.MaxValue)) << 32;
            ulong part2 = (ulong)Random.Range(int.MinValue, int.MaxValue);
            return new SingleGene
            {
                Value = part1 | part2
            };
        }
    }
}