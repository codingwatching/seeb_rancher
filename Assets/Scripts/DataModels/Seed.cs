using Genetics;
using System;

namespace Assets.Scripts.DataModels
{
    [Serializable]
    public class Seed
    {
        public Seed()
        {
        }
        public Seed(Genome genes, Plants.BasePlantType plantType)
        {
            this.genes = genes;
            this.plantType = plantType.myId;
        }
        public int plantType;
        public Genome genes;
    }
}
