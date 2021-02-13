using System;
using System.Linq;

namespace Assets.Scripts.DataModels
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
        public Seed[] AllSeeds = new Seed[0];

        public bool Empty => AllSeeds.Length <= 0;

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
            var oneSeed = AllSeeds[0];
            AllSeeds = AllSeeds.Skip(1).ToArray();
            return oneSeed;
        }
    }
}
