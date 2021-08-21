using Unity.Entities;

namespace Assets.Scripts.PlantWeapons
{
    [GenerateAuthoringComponent]
    public struct LifetimeComponent : IComponentData
    {
        public float currentLife;
        public float maxLifetime;
    }
}