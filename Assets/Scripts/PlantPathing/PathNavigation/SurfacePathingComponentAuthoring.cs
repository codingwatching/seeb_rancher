using Assets.Scripts.PlantWeapons;
using Assets.Scripts.PlantWeapons.Enemies;
using Assets.Scripts.PlantWeapons.Health;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
{
    [RequireComponent(typeof(HealthAuthoring))]
    public class SurfacePathingComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float waypointProximityRequirement = 0.1f;
        public float patherHeightOffsetFromGround = 0.5f;

        public float movementSpeed = 1f;


        public float damageSpeed = 10f;
        public float ignorableDurability = 5f;


        public float selfDamageDoneByAttacking = 1f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SurfaceWaypointFinder
            {
                waypointProximityRequirement = waypointProximityRequirement,
                waypointOffsetFromSurface = patherHeightOffsetFromGround
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
            dstManager.AddComponent<SimpleVelocityComponent>(entity);
            dstManager.AddComponent<TreatVoxelsAsSurface>(entity);
        }
    }


    public struct SurfaceWaypointFinder : IComponentData
    {
        public float waypointProximityRequirement;
        public float waypointOffsetFromSurface;
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
}