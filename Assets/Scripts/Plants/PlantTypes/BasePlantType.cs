using Assets.Scripts.DataModels;
using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [System.Serializable]
    public class PlantState: INativeDisposable
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

        public virtual JobHandle Dispose(JobHandle inputDeps)
        {
            return inputDeps;
        }

        public virtual void Dispose()
        {
        }
    }
    public abstract class BasePlantType : IDableObject
    {
        public GenomeEditor genome;
        public bool selfPollinating;
        public string plantName;
        public Sprite seedIcon;

        public GeneticDriver[] summaryDrivers;

        public abstract PlantState GenerateBaseStateAndHookTo();

        public abstract void AddGrowth(int phaseDiff, PlantState currentState);

        public abstract bool HasFlowers(PlantState currentState);

        public abstract void BuildPlantInto(
            Transform parent,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination);

        public abstract bool CanHarvest(PlantState state);
        public abstract bool IsMature(PlantState state);

        public Seed GenerateRandomSeed(System.Random randomProvider = null)
        {
            if(randomProvider == null)
            {
                randomProvider = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            }
            var mother = new Seed(
                genome.GenerateBaseGenomeData(new System.Random(randomProvider.Next(int.MinValue, int.MaxValue))),
                this,
                null);
            var father = new Seed(
                genome.GenerateBaseGenomeData(new System.Random(randomProvider.Next(int.MinValue, int.MaxValue))),
                this,
                null);

            var motherDrivers = genome.CompileGenome(mother.genes);

            return new Seed(
                new Genome(mother.genes, father.genes),
                this,
                motherDrivers);
        }

        public abstract IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed);

        protected abstract int GetHarvestedSeedNumber(PlantState currentState);
        public int TotalNumberOfSeedsInState(PlantState currentState)
        {
            return GetHarvestedSeedNumber(currentState);
        }

        public Seed[] HarvestSeeds(
            PollinationState sourcePollination,
            PlantState currentState,
            CompiledGeneticDrivers drivers)
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
                seedResult[i] = sourcePollination.GetChildSeed(drivers);
            }
            return seedResult;
        }
    }
}
