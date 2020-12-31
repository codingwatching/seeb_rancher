using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.Genes
{
    [CreateAssetMenu(fileName = "MendelianSwitchGene", menuName = "Genetics/Genes/MendelianSwitch", order = 2)]
    public class MendelianSwitch : GeneEditor
    {
        public BooleanGeneticDriver switchOutput;

        public override int GeneSize => 1;

        public override void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] genes)
        {
            if(editorHandle.TryGetGeneticData(switchOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {switchOutput} in gene {this}.");
            }
            var gene = genes[0];
            var booleanOutput = gene.chromosomalCopies.Any(x => EvaluateSingleGene(x));

            editorHandle.SetGeneticDriverData(switchOutput, booleanOutput);
        }

        private bool EvaluateSingleGene(SingleGene gene)
        {
            return HammingWeight(gene.Value) > 32;
        }

        /// <summary>
        /// lifted from https://stackoverflow.com/questions/39253221/hamming-weight-of-int64?noredirect=1&lq=1
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int HammingWeight(ulong x)
        {
            x = (x & 0x5555555555555555) + ((x >> 1) & 0x5555555555555555); //put count of each  2 bits into those  2 bits 
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333); //put count of each  4 bits into those  4 bits 
            x = (x & 0x0f0f0f0f0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f0f0f0f0f); //put count of each  8 bits into those  8 bits 
            x = (x & 0x00ff00ff00ff00ff) + ((x >> 8) & 0x00ff00ff00ff00ff); //put count of each 16 bits into those 16 bits 
            x = (x & 0x0000ffff0000ffff) + ((x >> 16) & 0x0000ffff0000ffff); //put count of each 32 bits into those 32 bits 
            x = (x & 0x00000000ffffffff) + ((x >> 32) & 0x00000000ffffffff); //put count of each 64 bits into those 64 bits 
            return (int)x;
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            yield return switchOutput;
        }
    }
}
