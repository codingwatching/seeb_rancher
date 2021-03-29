using Assets.Scripts.DataModels;
using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [System.Serializable]
    public class PlantState
    {
        public float growth;
        public int randomSeed;

        public PlantState(float growth)
        {
            this.growth = growth;
            randomSeed = Random.Range(int.MinValue, int.MaxValue);
        }

        public virtual void AfterDeserialized()
        {
        }
    }
    public abstract class BasePlantType : IDableObject
    {
        public GenomeEditor genome;
        public bool selfPollinating;
        public string plantName;
        public Sprite seedIcon;

        public abstract PlantState GenerateBaseSate();

        public abstract void AddGrowth(int phaseDiff, PlantState currentState);

        public abstract bool HasFlowers(PlantState currentState);

        public abstract void BuildPlantInto(
            Transform parent,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination);

        public abstract bool CanHarvest(PlantState state);

        public Seed GenerateRandomSeed()
        {
            return new Seed
            {
                genes = genome.GenerateBaseGenomeData(new System.Random(Random.Range(int.MinValue, int.MaxValue))),
                plantType = myId
            };
        }

        public abstract IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed);

        protected abstract int GetHarvestedSeedNumber(PlantState currentState);
        public int TotalNumberOfSeedsInState(PlantState currentState)
        {
            return GetHarvestedSeedNumber(currentState);
        }

        public Seed[] HarvestSeeds(PollinationState sourcePollination, PlantState currentState)
        {
            if (selfPollinating)
            {
                sourcePollination.SelfPollinateIfNotFertile();
            }
            if (!sourcePollination.IsPollinated)
            {
                return new Seed[0];
            }
            var generatedSeeds = GetHarvestedSeedNumber(currentState);
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
