using Genetics.GeneticDrivers;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public struct GeneData
    {
        public ulong Value;
    }

    public abstract class GeneEditor : ScriptableObject
    {
        public abstract IEnumerable<GeneticDriver> GetInputs();
        public abstract IEnumerable<GeneticDriver> GetOutputs();

        public abstract void Evaluate(GeneData gene, CompiledGeneticDrivers editorHandle);

        public virtual GeneData GenerateGeneData()
        {
            ulong part1 = ((ulong)Random.Range(int.MinValue, int.MaxValue)) << 32;
            ulong part2 = (ulong)Random.Range(int.MinValue, int.MaxValue);
            return new GeneData
            {
                Value = part1 | part2
            };
        }
    }
}