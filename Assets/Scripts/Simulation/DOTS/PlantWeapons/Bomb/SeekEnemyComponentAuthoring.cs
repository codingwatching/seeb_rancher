using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    public class SeekEnemyComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float seekSpeed;
        public float rotateSpeed;
        [Tooltip("In ideal cases, would be set to roughly how many seconds it takes this seeker to traverse 1 unit")]
        public float leadDistanceEstimate = 1;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SeekEnemyComponent
            {
                seekAcceleration = seekSpeed,
                rotateAcceleration = rotateSpeed,
                targetLeadDistancePerDisplacement = leadDistanceEstimate
            });
        }
    }

    public struct FoundTargetEntity : IComponentData
    {
        public Entity target;
    }
    public struct NoTargetComponent : IComponentData
    {
        public float3 randomTarget;
    }

    public struct SeekEnemyComponent : IComponentData
    {
        public float seekAcceleration;
        public float rotateAcceleration;
        public float targetLeadDistancePerDisplacement;
    }
}