using Assets.Scripts.PlantPathing;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    [UpdateBefore(typeof(ApplyVelocitySystem))]
    [UpdateAfter(typeof(FindEnemySystem))]
    public class SeekEnemySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            Entities
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref VelocityComponent velocity,
                    in FoundTargetEntity target,
                    in SeekEnemyComponent seeker,
                    in Translation selfPosition) =>
                {
                    var targetPos = GetComponent<Translation>(target.target);
                    float3 diff = Vector3.Normalize(targetPos.Value - selfPosition.Value);
                    velocity.velocity = velocity.velocity + diff * seeker.seekAcceleration * deltaTime;
                }).ScheduleParallel();

            Entities
                .WithNone<FoundTargetEntity>()
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref VelocityComponent velocity,
                    in Translation selfPosition,
                    in NoTargetComponent target,
                    in SeekEnemyComponent seeker) =>
                {
                    float3 diff = Vector3.Normalize(target.randomTarget - selfPosition.Value);
                    velocity.velocity = velocity.velocity + diff * seeker.seekAcceleration * deltaTime;
                }).ScheduleParallel();
        }
    }
}