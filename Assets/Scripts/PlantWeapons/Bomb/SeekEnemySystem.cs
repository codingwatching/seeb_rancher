using Assets.Scripts.PlantPathing;
using Assets.Scripts.PlantPathing.PathNavigaton;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    [UpdateBefore(typeof(ApplyVelocitySystem))]
    [UpdateAfter(typeof(FindEnemySystem))]
    public class SeekEnemySystem : SystemBase
    {

        private SurfaceDefinitionSingleton surfaceDefinition;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        // TODO: this system scales acceleration by timescale, but velocity will still execute at a normal timescale
        //  might have to just use global time.timeScale to set it up
        protected override void OnUpdate()
        {
            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var simSpeed = surfaceDefinition.gameSpeed.CurrentValue;
            var deltaTime = Time.DeltaTime * simSpeed;
            Entities
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref PhysicsVelocity velocity,
                    in FoundTargetEntity target,
                    in SeekEnemyComponent seeker,
                    in Translation selfPosition,
                    in PhysicsMass massData) =>
                {
                    if (HasComponent<Translation>(target.target))
                    {
                        var targetPos = GetComponent<Translation>(target.target);
                        float3 diff = Vector3.Normalize(targetPos.Value - selfPosition.Value);
                        velocity.Linear += diff * seeker.seekAcceleration * massData.InverseMass * deltaTime;
                        // TODO: do something to the angular velocity too. make it point towards the target probably.
                    }else
                    {
                        ecb.RemoveComponent<FoundTargetEntity>(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);

            Entities
                .WithNone<FoundTargetEntity>()
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref PhysicsVelocity velocity,
                    in Translation selfPosition,
                    in NoTargetComponent target,
                    in SeekEnemyComponent seeker,
                    in PhysicsMass massData) =>
                {
                    float3 diff = Vector3.Normalize(target.randomTarget - selfPosition.Value);
                    velocity.Linear += diff * seeker.seekAcceleration * massData.InverseMass * deltaTime;
                }).ScheduleParallel();
        }
    }
}