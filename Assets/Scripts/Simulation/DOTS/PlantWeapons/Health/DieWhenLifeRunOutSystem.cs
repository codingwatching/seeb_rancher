using Unity.Entities;

namespace Simulation.DOTS.PlantWeapons.Health
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class DieWhenLifeRunOutSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithAll<DestroyWhenHealthOut>()
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref HealthComponent remainingLife) =>
                {
                    if (remainingLife.currentHealth <= 0)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}