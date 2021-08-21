using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class ApplyVelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            Entities
                .ForEach((
                    Entity entity,
                    int entityInQueryIndex,
                    ref Translation selfPosition,
                    ref SimpleVelocityComponent velocity,
                    ref Rotation selfRotation) =>
                {
                    selfPosition.Value = selfPosition.Value + velocity.velocity * deltaTime;

                    if (math.abs(velocity.velocity.x) > 0.01 || math.abs(velocity.velocity.y) > 0.01 || math.abs(velocity.velocity.z) > 0.01)
                    {
                        var rotation = Quaternion.LookRotation(velocity.velocity, Vector3.up);
                        selfRotation.Value = Quaternion.Lerp(selfRotation.Value, rotation, deltaTime);
                    }
                }).ScheduleParallel();
        }
    }
}