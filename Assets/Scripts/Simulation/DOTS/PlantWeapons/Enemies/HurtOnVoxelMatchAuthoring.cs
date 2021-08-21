using Assets.Scripts.PlantWeapons.Health;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Enemies
{
    [RequireComponent(typeof(HealthAuthoring))]
    public class HurtOnVoxelMatchAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new HurtOnVoxelMatch
            {
            });

        }
    }

    public struct HurtOnVoxelMatch : IComponentData
    {
    }

}