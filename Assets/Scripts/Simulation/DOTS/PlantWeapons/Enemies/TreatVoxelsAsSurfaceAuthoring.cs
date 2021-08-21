using Assets.Scripts.PlantWeapons.Health;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Enemies
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