using Assets.Scripts.PlantPathing;
using Assets.Scripts.PlantWeapons;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
{
    public class ApproachWaypointSystem : SystemBase
    {
        private SurfaceDefinitionSingleton surfaceDefinition;

        protected override void OnCreate()
        {
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
        }

        protected override void OnUpdate()
        {
            var simSpeed = surfaceDefinition.gameSpeed.CurrentValue;
            Entities
                .ForEach((Entity entity, int entityInQueryIndex,
                ref Translation position,
                ref SurfaceWaypointTarget target, 
                ref WaypointFollowerComponent follower,
                ref VelocityComponent velocity) =>
                {
                    Vector3 diff = target.target - position.Value;
                    var normalized = diff.normalized;
                    velocity.velocity = diff.normalized * follower.movementSpeed * simSpeed;
                }).ScheduleParallel();
        }
    }
}