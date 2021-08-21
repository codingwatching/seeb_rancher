using Unity.Entities;

namespace Simulation.DOTS.PlantWeapons
{
    [GenerateAuthoringComponent]
    public struct LifetimeComponent : IComponentData
    {
        public float currentLife;
        public float maxLifetime;
    }
}