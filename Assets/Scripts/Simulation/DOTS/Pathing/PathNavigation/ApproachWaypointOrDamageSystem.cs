using Assets.Scripts.PlantWeapons;
using Assets.Scripts.PlantWeapons.Health;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
{
    [UpdateAfter(typeof(FindSurfaceWaypointSystem))]
    public class ApproachWaypointOrDamageSystem : SystemBase
    {
        private SurfaceDefinitionSingleton surfaceDefinition;
        private OrganVolumetricWorld durabilityWorld;
        private OrganDamageWorld damageWorld;

        protected override void OnCreate()
        {
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            durabilityWorld = GameObject.FindObjectOfType<OrganVolumetricWorld>();
            damageWorld = GameObject.FindObjectOfType<OrganDamageWorld>();
        }

        protected override void OnUpdate()
        {
            var simSpeed = surfaceDefinition.gameSpeed.CurrentValue;
            var deltaTime = simSpeed * Time.DeltaTime;
            var durabilityData = durabilityWorld.nativeVolumeData.openReadData.AsReadOnly();
            var patherHeight = surfaceDefinition.patherHeight;
            var layout = durabilityWorld.voxelLayout;

            var damageData = damageWorld.GetDamageValuesReadSafe();

            Entities
                .ForEach((
                ref Translation position,
                ref SurfaceWaypointTarget target,
                ref WaypointFollowerComponent follower,
                ref SimpleVelocityComponent velocity,
                ref BlockingVoxelDamageComponent damageInfo,
                ref HealthComponent health,
                in SurfaceWaypointFinder waypointFinderParams) =>
                {
                    var nextTile = layout.SurfaceGetSurfaceCoordinates(target.target);
                    var minVoxelHeight = (int)math.max(math.floor(target.target.y - waypointFinderParams.waypointOffsetFromSurface), 0);
                    var maxVoxelHeight = (int)math.min(math.floor(target.target.y - waypointFinderParams.waypointOffsetFromSurface + patherHeight), layout.worldResolution.y);

                    var nextTileDurability = 0f;
                    for (int y = minVoxelHeight; y <= maxVoxelHeight; y++)
                    {
                        var voxel = new Vector3Int(nextTile.x, y, nextTile.y);
                        var id = layout.GetDataIndexFromCoordinates(voxel);
                        var durability = durabilityData[id];
                        nextTileDurability += durability;
                    }

                    if (nextTileDurability <= damageInfo.ignorableDurability)
                    {
                        Vector3 diff = target.target - position.Value;
                        var normalized = diff.normalized;
                        velocity.velocity = diff.normalized * follower.movementSpeed * simSpeed;
                    }
                    else
                    {
                        velocity.velocity = float3.zero;

                        var damagePerDurability = damageInfo.damageSpeed * deltaTime / nextTileDurability;
                        for (int y = minVoxelHeight; y <= maxVoxelHeight; y++)
                        {
                            var voxel = new Vector3Int(nextTile.x, y, nextTile.y);
                            var id = layout.GetDataIndexFromCoordinates(voxel);
                            var durability = durabilityData[id];

                            damageData[id] += durability * damagePerDurability;
                        }
                        health.currentHealth -= damageInfo.selfDamageDoneByAttacking * deltaTime;
                    }
                }).Schedule();

            damageWorld.RegisterDamageValuesWriter(this.Dependency);
            durabilityWorld.nativeVolumeData.RegisterReadingDependency(this.Dependency);
        }
    }
}