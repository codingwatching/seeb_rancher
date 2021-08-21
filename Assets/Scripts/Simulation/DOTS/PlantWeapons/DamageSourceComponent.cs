using Unity.Entities;

namespace Assets.Scripts.PlantWeapons
{
    [GenerateAuthoringComponent]
    public struct DamageSourceComponent : IComponentData
    {
        public float baseDamage;
    }
}