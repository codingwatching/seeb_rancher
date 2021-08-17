using Assets.Scripts.PlantWeapons.Enemies;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    public class FindEnemySystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private Unity.Mathematics.Random random;
        private EntityQuery enemies;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            random = new Unity.Mathematics.Random(1928437918);
            enemies = GetEntityQuery(typeof(EnemyComponent), typeof(Translation));
        }

        protected override void OnUpdate()
        {
            var enemyEntities = enemies.ToEntityArray(Allocator.TempJob);

            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            if (enemyEntities.Length > 0)
            {
                var enemyEntitiesReadable = enemyEntities.AsReadOnly();
                Entities
                    .WithAll<SeekEnemyComponent>()
                    .WithNone<FoundTargetEntity>()
                    .ForEach((Entity entity, int entityInQueryIndex, in Translation position) =>
                    {
                        Entity targetEntity = enemyEntitiesReadable[0];
                        float3 targetPosition = GetComponent<Translation>(targetEntity).Value;
                        float distToTarget = math.distancesq(targetPosition, position.Value);

                        for (int i = 1; i < enemyEntitiesReadable.Length; i++)
                        {
                            var nextTarget = enemyEntitiesReadable[i];
                            var nextTargetPosition = GetComponent<Translation>(nextTarget);
                            var dist = math.distancesq(position.Value, nextTargetPosition.Value);
                            if (dist < distToTarget)
                            {
                                targetPosition = nextTargetPosition.Value;
                                distToTarget = dist;
                                targetEntity = nextTarget;
                            }
                        }
                        ecb.AddComponent(entityInQueryIndex, entity, new FoundTargetEntity
                        {
                            target = targetEntity
                        });
                    }).ScheduleParallel();
            }
            else
            {
                random.NextFloat();
                var rand = random;
                Entities
                    .WithAll<SeekEnemyComponent>()
                    .WithNone<FoundTargetEntity, NoTargetComponent>()
                    .ForEach((Entity entity, int entityInQueryIndex, ref Translation position) =>
                    {
                        var randLocal = Unity.Mathematics.Random.CreateFromIndex((uint)entityInQueryIndex);
                        randLocal.state = randLocal.state ^ rand.state;
                        ecb.AddComponent(entityInQueryIndex, entity, new NoTargetComponent
                        {
                            randomTarget = position.Value + randLocal.NextFloat3(new float3(-3, -3, -3), new float3(3, 3, 3))
                        });
                    }).ScheduleParallel();
            }
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
            enemyEntities.Dispose(this.Dependency);
        }
    }
}