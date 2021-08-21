using Simulation.DOTS.PlantWeapons.Bomb;
using Simulation.DOTS.PlantWeapons.Enemies;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Simulation.DOTS.PlantWeapons.SmokeTrail
{
    public class SmokeTrailSpawnSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private Unity.Mathematics.Random random;
        private EntityQuery enemies;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            random = new Unity.Mathematics.Random(09818350);
            enemies = GetEntityQuery(typeof(EnemyComponent), typeof(Translation));
        }

        protected override void OnUpdate()
        {
            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var deltaTime = Time.DeltaTime;
            var totalTime = Time.ElapsedTime;
            Entities
                .WithNone<SeekEnemyComponent>()
                .ForEach((Entity entity, int entityInQueryIndex,
                ref SmokeTrailSpawnerComponent spawner,
                in Translation translation,
                in PhysicsVelocity velocity) =>
                {
                    if (spawner.lastSpawnTime + spawner.timeToSpawn < totalTime)
                    {
                        spawner.lastSpawnTime = (float)totalTime;

                        var newSmoke = ecb.Instantiate(entityInQueryIndex, spawner.prefab);
                        ecb.SetComponent(entityInQueryIndex, newSmoke, new SimpleVelocityComponent
                        {
                            velocity = spawner.smokeVelocity
                        });
                        ecb.SetComponent(entityInQueryIndex, newSmoke, new Translation
                        {
                            Value = translation.Value
                        });
                    }
                }).ScheduleParallel();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}