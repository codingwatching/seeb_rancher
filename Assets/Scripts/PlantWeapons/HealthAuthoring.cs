using Assets.Scripts.PlantWeapons;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons
{
    public class HealthAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float health = 10f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new HealthComponent
            {
                currentHealth = health
            });

        }
    }

    public struct HealthComponent : IComponentData
    {
        public float currentHealth;
    }

}