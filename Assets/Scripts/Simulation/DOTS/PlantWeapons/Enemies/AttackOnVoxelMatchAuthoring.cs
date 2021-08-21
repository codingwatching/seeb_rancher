using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Enemies
{
    public class AttackOnVoxelMatchAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float damageDealt;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AttackOnVoxelMatch
            {
                damageDealt = damageDealt
            });

        }
    }

    public struct AttackOnVoxelMatch : IComponentData
    {
        public float damageDealt;
    }

}