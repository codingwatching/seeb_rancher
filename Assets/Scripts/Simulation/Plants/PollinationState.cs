using Assets.Scripts.DataModels;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants
{

    [System.Serializable]
    public class PollinationState
    {
        [SerializeField]
        private bool _hasAnther;
        public bool HasAnther { get => _hasAnther; private set => _hasAnther = value; }
        [SerializeField]
        private List<Seed> pollinationSources;
        public bool IsPollinated => pollinationSources.Count > 0;

        [SerializeField]
        private Seed _selfGenes;
        public Seed SelfGenes { get => _selfGenes; private set => _selfGenes = value; }

        public PollinationState(Seed selfGenes)
        {
            HasAnther = true;
            pollinationSources = new List<Seed>();
            SelfGenes = selfGenes;
        }

        public void SelfPollinateIfNotFertile()
        {
            if (IsPollinated)
            {
                return;
            }
            pollinationSources.Add(SelfGenes);
        }

        public Seed GetChildSeed(CompiledGeneticDrivers drivers)
        {
            if (pollinationSources.Any(source => source.plantType != SelfGenes.plantType))
            {
                throw new System.Exception("breeding plants of different types is not supported");
            }
            if (pollinationSources.Count <= 0)
            {
                throw new System.Exception("No pollination source, plant is infertile");
            }
            var selectedSource = pollinationSources[Random.Range(0, pollinationSources.Count)];
            return new Seed(
                new Genetics.Genome(selectedSource.genes, SelfGenes.genes),
                SelfGenes.plantType,
                drivers);
        }

        public bool CanPollinate()
        {
            return HasAnther;
        }

        public void ClipAnthers()
        {
            HasAnther = false;
        }

        public bool RecieveGenes(PollinationState other)
        {
            if (!other.HasAnther)
            {
                return false;
            }
            pollinationSources.Add(other.SelfGenes);
            return true;
        }
    }
}
