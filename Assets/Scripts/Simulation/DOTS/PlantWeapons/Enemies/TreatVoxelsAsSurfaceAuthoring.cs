using Simulation.DOTS.PlantWeapons.Health;
using Unity.Entities;
using UnityEngine;

namespace Simulation.DOTS.PlantWeapons.Enemies
{
    [RequireComponent(typeof(HealthAuthoring))]
    public class TreatVoxelsAsSurfaceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TreatVoxelsAsSurface
            {
            });

        }
    }

    public struct TreatVoxelsAsSurface : IComponentData
    {

    }

}