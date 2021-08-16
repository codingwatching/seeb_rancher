using Assets.Scripts.PlantPathing;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    public class FindEnemySystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private Unity.Mathematics.Random random;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            random = new Unity.Mathematics.Random(1928437918);
        }

        protected override void OnUpdate()
        {
            var enemies = GameObject.FindObjectsOfType<DamageWhenVoxelTileReachedComponent>();
            var enemyPositions = new NativeArray<float3>(enemies.Length, Allocator.TempJob);
            for (int i = 0; i < enemies.Length; i++)
            {
                enemyPositions[i] = enemies[i].transform.position;
            }

            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            if (enemyPositions.Length > 0)
            {
                var enemyPositionsReadable = enemyPositions.AsReadOnly();
                Entities
                    .WithAll<SeekEnemyComponent>()
                    .WithNone<FoundTargetEntity>()
                    .ForEach((Entity entity, int entityInQueryIndex, ref Translation position) =>
                    {
                        float3 targetPosition = enemyPositionsReadable[0];
                        float distToTarget = math.distancesq(position.Value, targetPosition);
                        for (int i = 1; i < enemyPositionsReadable.Length; i++)
                        {
                            var nextTargetPosition = enemyPositionsReadable[i];
                            var dist = math.distancesq(position.Value, nextTargetPosition);
                            if (dist < distToTarget)
                            {
                                targetPosition = nextTargetPosition;
                                distToTarget = dist;
                            }
                        }
                        ecb.AddComponent(entityInQueryIndex, entity, new FoundTargetEntity
                        {
                            target = targetPosition
                        });
                    }).ScheduleParallel();
            }else
            {
                random.NextFloat();
                var rand = random;
                Entities
                    .WithAll<SeekEnemyComponent>()
                    .WithNone<FoundTargetEntity>()
                    .ForEach((Entity entity, int entityInQueryIndex, ref Translation position) =>
                    {
                        var randLocal = Unity.Mathematics.Random.CreateFromIndex((uint)entityInQueryIndex);
                        randLocal.state = randLocal.state ^ rand.state;
                        ecb.AddComponent(entityInQueryIndex, entity, new FoundTargetEntity
                        {
                            target = position.Value + randLocal.NextFloat3(new float3(-3, -3, -3), new float3(3, 3, 3))
                        });
                    }).ScheduleParallel();
            }
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
            enemyPositions.Dispose(this.Dependency);
        }
    }
}