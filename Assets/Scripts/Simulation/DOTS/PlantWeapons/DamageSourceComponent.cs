using Unity.Entities;

namespace Simulation.DOTS.PlantWeapons
{
    [GenerateAuthoringComponent]
    public struct DamageSourceComponent : IComponentData
    {
        public float baseDamage;
    }
}