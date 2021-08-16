using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    public class SeekEnemyComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float seekSpeed;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SeekEnemyComponent
            {
                seekAcceleration = seekSpeed
            });
            dstManager.AddComponent<VelocityComponent>(entity);
        }
    }

    public struct FoundTargetEntity : IComponentData
    {
        public float3 target;
    }

    public struct SeekEnemyComponent : IComponentData
    {
        public float seekAcceleration;
    }
}