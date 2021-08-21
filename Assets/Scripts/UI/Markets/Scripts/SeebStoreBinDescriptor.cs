using Dman.ObjectSets;
using Genetics.ParameterizedGenomeGenerator;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using MyUtilities;
using Simulation.Plants.PlantTypes;
using Simulation.Plants.PlantData;

namespace UI.Markets
{
    /// <summary>
    /// class used during runtime to describe a bin which will provide seebs
    /// Goes through binary serialization. Intended to be edited inside the unity editor
    /// </summary>
    [System.Serializable]
    public class SeebStoreBinDescriptor : ISerializable
    {
        public BooleanGeneticTarget[] booleanTargets;
        public FloatGeneticTarget[] floatTargets;
        public float price;
        public BasePlantType plantType;
        [Tooltip("How many seebs this slot can generate")]
        public int seedCount;

        public SeebStoreBinDescriptor()
        { }

        public List<Seed> GeneratedSeebs { get; set; }

        public IEnumerator EvaluateNewSeebs(int seedCount)
        {
            var genomeGenerator = new GenomeGenerator()
            {
                booleanTargets = booleanTargets,
                floatTargets = floatTargets,
                genomeTarget = plantType.genome
            };

            GeneratedSeebs = new List<Seed>();

            // take no more than 60 frames, or 1 second, per seed generated
            var totalFrameLimit = 60 * seedCount;
            var randomSource = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            var seedSequence = genomeGenerator.GenerateGenomes(
                10,
                () => plantType.GenerateRandomSeed(randomSource),
                seed => seed.genes
                );
            // Warning: this could be an infinite loop, GenerateGenomes is an infinite generator function
            foreach (var nextSeed in seedSequence)
            {
                if (nextSeed == null)
                {
                    totalFrameLimit--;
                    if (totalFrameLimit <= 0)
                    {
                        throw new System.Exception("Took too many frames to generate next seeb stack");
                    }
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                GeneratedSeebs.Add(nextSeed);
                if (GeneratedSeebs.Count >= seedCount)
                {
                    yield break;
                }
            }
        }

        #region Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("booleanTargets", booleanTargets);
            info.AddValue("floatTargets", floatTargets);
            info.AddValue("reward", price);
            info.AddValue("seedRequirement", seedCount);
            info.AddValue("plantType", new IDableSavedReference(plantType));
        }


        // The special constructor is used to deserialize values.
        private SeebStoreBinDescriptor(SerializationInfo info, StreamingContext context)
        {
            booleanTargets = info.GetValue<BooleanGeneticTarget[]>("booleanTargets");
            floatTargets = info.GetValue<FloatGeneticTarget[]>("floatTargets");

            price = info.GetSingle("reward");
            seedCount = info.GetInt32("seedRequirement");

            var savedReference = info.GetValue("plantType", typeof(IDableSavedReference)) as IDableSavedReference;
            plantType = savedReference?.GetObject<BasePlantType>();
        }

        #endregion
    }
}
