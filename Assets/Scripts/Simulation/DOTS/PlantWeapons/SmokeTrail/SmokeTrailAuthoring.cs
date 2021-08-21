using Unity.Entities;
using UnityEngine;

namespace Simulation.DOTS.PlantWeapons.SmokeTrail
{
    public class SmokeTrailAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SmokeTrailComponent
            {
            });
            dstManager.AddComponent<SimpleVelocityComponent>(entity);
        }
    }

    public struct SmokeTrailComponent : IComponentData
    {
    }
}