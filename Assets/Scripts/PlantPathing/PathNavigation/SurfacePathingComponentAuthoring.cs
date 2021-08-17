using Assets.Scripts.PlantWeapons;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
{
    public class SurfacePathingComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float waypointProximityRequirement = 0.1f;

        public float movementSpeed = 1f;


        public float damageSpeed = 10f;
        public float ignorableDurability = 5f;


        public float selfDamageDoneByAttacking = 1f;
        public float health = 10f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SurfaceWaypointFinder
            {
                waypointProximityRequirement = waypointProximityRequirement
            });
            dstManager.AddComponentData(entity, new WaypointFollowerComponent
            {
                movementSpeed = movementSpeed
            });
            dstManager.AddComponentData(entity, new BlockingVoxelDamageComponent
            {
                damageSpeed = damageSpeed,
                ignorableDurability = ignorableDurability,
                selfDamageDoneByAttacking = selfDamageDoneByAttacking
            });
            dstManager.AddComponentData(entity, new HealthComponent
            {
                currentHealth = health
            });
            dstManager.AddComponent<VelocityComponent>(entity);
        }
    }

    public struct SurfaceWaypointFinder : IComponentData
    {
        public float waypointProximityRequirement;
    }

    public struct SurfaceWaypointTarget : IComponentData
    {
        public float3 target;
    }

    public struct WaypointFollowerComponent : IComponentData
    {
        public float movementSpeed;
    }

    public struct BlockingVoxelDamageComponent : IComponentData
    {
        public float damageSpeed;
        public float ignorableDurability;
        public float selfDamageDoneByAttacking;
    }
    public struct HealthComponent : IComponentData
    {
        public float currentHealth;
    }
}