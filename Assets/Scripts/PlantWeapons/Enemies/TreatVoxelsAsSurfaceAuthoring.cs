using Assets.Scripts.PlantWeapons;
using Unity.Entities;
using Unity.Mathematics;
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