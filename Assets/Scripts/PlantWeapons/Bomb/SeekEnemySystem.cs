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
                    ref Translation selfPosition,
                    ref FoundTargetEntity target,
                    ref SeekEnemyComponent seeker,
                    ref VelocityComponent velocity) =>
                {
                    float3 diff = Vector3.Normalize(target.target - selfPosition.Value);
                    velocity.velocity = velocity.velocity + diff * seeker.seekAcceleration * deltaTime;
                }).ScheduleParallel();
        }
    }
}