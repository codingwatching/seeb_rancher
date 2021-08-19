using Assets.Scripts.PlantWeapons;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Health
{
    [RequireComponent(typeof(HealthAuthoring))]
    public class HealthBarAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Transform healthBarMesh;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var healthBarEntity = conversionSystem.GetPrimaryEntity(healthBarMesh.gameObject);
            var healthComponent = GetComponent<HealthAuthoring>();
            dstManager.AddComponentData(entity, new ScaleHealthBarComponent
            {
                healthBarHolder = healthBarEntity,
                initialHealthScale = healthBarMesh.transform.localScale,
                maxHealth = healthComponent.health
            });

        }
    }

    public struct ScaleHealthBarComponent : IComponentData
    {
        public Entity healthBarHolder;
        public float3 initialHealthScale;
        public float maxHealth;
    }


    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ScaleHealthBarSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref ScaleHealthBarComponent scaler,
                    in HealthComponent remainingLife) =>
                {
                    ecb.SetComponent(entityInQueryIndex, scaler.healthBarHolder, new NonUniformScale
                    {
                        Value = new float3(
                            scaler.initialHealthScale.x * remainingLife.currentHealth / scaler.maxHealth,
                            scaler.initialHealthScale.y,
                            scaler.initialHealthScale.z)
                    });
                }).ScheduleParallel();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}