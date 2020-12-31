using Assets.Scripts.DataModels;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using Genetics;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "PlantType", menuName = "Greenhouse/PlantType", order = 1)]
    public class PlantType : IDableObject
    {
        public GenomeEditor genome;
        public bool selfPollinating;
        public string plantName;
        [Header("Growth")]
        public PlantBuilder plantBuilder;
        public float growthPerPhase;
        public float minPollinationGrowthPhase;
        public float maxPollinationGrowthPhase;

        [Header("Seebs")]
        public Sprite seedIcon;
        public int minSeeds;
        public int maxSeeds;

        public float AddGrowth(int phaseDiff, float currentGrowth)
        {
            var extraGrowth = growthPerPhase * phaseDiff;
            return Mathf.Clamp(currentGrowth + extraGrowth, 0, 1);
        }

        public bool IsInPollinationRange(float growth)
        {
            return growth >= minPollinationGrowthPhase && growth <= maxPollinationGrowthPhase;
        }

        public Seed[] HarvestSeeds(PollinationState sourcePollination)
        {
            if (selfPollinating)
            {
                sourcePollination.SelfPollinateIfNotFertile();
            }
            if (!sourcePollination.IsFertilized())
            {
                return new Seed[0];
            }
            var generatedSeeds = Random.Range(minSeeds, maxSeeds);
            var seedResult = new Seed[generatedSeeds];
            for (int i = 0; i < seedResult.Length; i++)
            {
                seedResult[i] = new Seed
                {
                    plantType = myId,
                    genes = sourcePollination.GetChildSeed().genes
                };
            }
            return seedResult;
        }
    }
}
