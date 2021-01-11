using Assets.Scripts.DataModels;
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
        private Seed pollinatedGenes;
        [SerializeField]
        private bool isPollinated;

        [SerializeField]
        private Seed _selfGenes;
        public Seed SelfGenes { get => _selfGenes; private set => _selfGenes = value; }

        public PollinationState(Seed selfGenes)
        {
            HasAnther = true;
            pollinatedGenes = null;
            isPollinated = false;
            SelfGenes = selfGenes;
        }

        public bool IsFertilized()
        {
            return isPollinated;
        }

        public void SelfPollinateIfNotFertile()
        {
            if (IsFertilized())
            {
                return;
            }
            pollinatedGenes = SelfGenes;
            isPollinated = true;
        }

        public Seed GetChildSeed()
        {
            if (pollinatedGenes.plantType != SelfGenes.plantType)
            {
                throw new System.Exception("breeding plants of different types is not supported");
            }
            return new Seed
            {
                plantType = pollinatedGenes.plantType,
                genes = new Genetics.Genome(pollinatedGenes.genes, SelfGenes.genes)
            };
        }

        public bool CanPollinate()
        {
            return HasAnther;
        }

        public bool RecieveGenes(PollinationState other)
        {
            if (isPollinated || !other.HasAnther)
            {
                return false;
            }
            pollinatedGenes = other.SelfGenes;
            isPollinated = true;
            HasAnther = false;
            return true;
        }
    }
}
