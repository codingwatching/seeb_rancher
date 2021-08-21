using Unity.Entities;
using UnityEngine;

namespace Simulation.DOTS.PlantWeapons
{
    public class ScaleWithLifetimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // TODO: detect if component will have non uniform scale, uniform scale, or something else
            dstManager.AddComponentData(entity, new ScaleWithLifetimeComponent
            {
                baseScale = transform.localScale.x
            });
        }
    }

    public struct ScaleWithLifetimeComponent : IComponentData
    {
        public float baseScale;
    }
}