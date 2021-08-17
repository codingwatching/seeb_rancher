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
    }
}