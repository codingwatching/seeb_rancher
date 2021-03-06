using Unity.Entities;
using UnityEngine;

namespace Simulation.DOTS.PlantWeapons.Enemies
{
    public class DamageOnCollisionTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new DamageOnCollisionTriggerComponent
            {
            });

        }
    }

    public struct DamageOnCollisionTriggerComponent : IComponentData
    {
    }

}