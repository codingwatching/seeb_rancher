using Unity.Entities;
using UnityEngine;

namespace Simulation.DOTS.PlantWeapons.Health
{
    public class HealthAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float health = 10f;
        public bool destroyWhenNoHealth = true;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new HealthComponent
            {
                currentHealth = health
            });

            if (destroyWhenNoHealth)
            {
                dstManager.AddComponent<DestroyWhenHealthOut>(entity);
            }

        }
    }

    public struct HealthComponent : IComponentData
    {
        public float currentHealth;
    }
    public struct DestroyWhenHealthOut : IComponentData
    {
    }
}