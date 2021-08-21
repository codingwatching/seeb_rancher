using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Simulation.DOTS.PlantWeapons
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class LifetimeSystem : SystemBase
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
                    ref LifetimeComponent lifetime) =>
                {
                    lifetime.currentLife += deltaTime;
                    if (lifetime.currentLife > lifetime.maxLifetime)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);

            Entities.ForEach((ref NonUniformScale scale, in LifetimeComponent life, in ScaleWithLifetimeComponent scaling) =>
            {
                var lifeLeftAsProportion = 1 - (life.currentLife / life.maxLifetime);
                var resultScale = scaling.baseScale * math.pow(lifeLeftAsProportion, 1 / 3f);
                scale.Value = new float3(resultScale, resultScale, resultScale);
            }).ScheduleParallel();
        }
    }
}