using Dman.ObjectSets;
using Genetics.GeneSummarization;
using Simulation.Plants.PlantData;
using Simulation.Plants.PlantTypes;
using System;
using System.Linq;

namespace UI.SeedInventory
{
    [Serializable]
    public class SeedBucketUI
    {
        public SeedBucket bucket = new SeedBucket();
        public string description;
    }

    [Serializable]
    public class SeedBucket
    {
        private Seed[] AllSeeds = new Seed[0];

        public bool Empty => AllSeeds.Length <= 0;
        public int SeedCount => AllSeeds.Length;
        public int PlantTypeId => AllSeeds[0].plantType;

        private bool shuffledSinceLastAddition;

        public SeedBucket(Seed[] seeds) : this()
        {
            AllSeeds = seeds;
        }
        public SeedBucket()
        {
            shuffledSinceLastAddition = false;
        }

        public GeneticDriverSummarySet SummarizeSeeds()
        {
            if (Empty)
            {
                return null;
            }
            var firstSeed = AllSeeds[0];
            var plantTypeRegistry = RegistryRegistry.GetObjectRegistry<BasePlantType>();
            var plantType = plantTypeRegistry.GetUniqueObjectFromID(firstSeed.plantType);
            return new GeneticDriverSummarySet(
                plantType.summaryDrivers,
                AllSeeds.Select(x => x.parentAttributes));
        }

        public bool TryAddSeedsToSet(Seed[] seeds)
        {
            if (AllSeeds.Length <= 0)
            {
                AllSeeds = seeds;
            }
            else
            {
                var seedType = AllSeeds[0].plantType;
                if (seeds.Any(s => s.plantType != seedType))
                {
                    return false;
                }
                AllSeeds = AllSeeds.Concat(seeds).ToArray();
            }
            shuffledSinceLastAddition = false;
            return true;
        }

        /// <summary>
        /// try to move the seeds from <paramref name="sourceBucket"/> into this bucket
        /// </summary>
        /// <param name="sourceBucket"></param>
        /// <returns>false if no seeds were transferred. true if all seeds were transferred</returns>
        public bool TryTransferSeedsIntoSelf(SeedBucket sourceBucket)
        {
            if (!TryAddSeedsToSet(sourceBucket.AllSeeds))
            {
                return false;
            }
            sourceBucket.AllSeeds = new Seed[0];
            return true;
        }

        public Seed[] TakeN(int n)
        {
            if (AllSeeds.Length < n)
            {
                return null;
            }
            EnsureShuffled();
            var seedSample = AllSeeds.Take(n).ToArray();
            AllSeeds = AllSeeds.Skip(n).ToArray();
            return seedSample;
        }

        public Seed TakeOne()
        {
            if (AllSeeds.Length <= 0)
            {
                return null;
            }
            EnsureShuffled();
            var oneSeed = AllSeeds[0];
            AllSeeds = AllSeeds.Skip(1).ToArray();
            return oneSeed;
        }
        private void EnsureShuffled()
        {
            if (shuffledSinceLastAddition)
            {
                return;
            }
            var randomProvider = new System.Random();
            AllSeeds = AllSeeds.OrderBy(x => randomProvider.NextDouble()).ToArray();
            shuffledSinceLastAddition = true;
        }
    }
}
