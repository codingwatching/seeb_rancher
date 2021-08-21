using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    public class SmokeTrailSpawningAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public SmokeTrailAuthoring smokeTrailPrefab;
        public float timeToSpawn = 1f;
        public float smokeVelocityMultiplier = -2f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var smokeEntity = conversionSystem.GetPrimaryEntity(smokeTrailPrefab.gameObject);
            dstManager.AddComponentData(entity, new SmokeTrailSpawnerComponent
            {
                timeToSpawn = timeToSpawn,
                prefab = smokeEntity,
                lastSpawnTime = 0,
                smokeVelocity = smokeVelocityMultiplier
            });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(smokeTrailPrefab.gameObject);
        }
    }
    public struct SmokeTrailSpawnerComponent : IComponentData
    {
        public Entity prefab;
        public float timeToSpawn;
        public float lastSpawnTime;
        public float smokeVelocity;
    }
}