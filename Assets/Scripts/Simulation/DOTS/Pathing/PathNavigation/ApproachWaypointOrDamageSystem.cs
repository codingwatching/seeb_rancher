using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.SceneSaveSystem;
using Simulation.DOTS.PlantWeapons;
using Simulation.DOTS.PlantWeapons.Health;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Simulation.DOTS.Pathing.PathNavigaton
{
    [UpdateAfter(typeof(FindSurfaceWaypointSystem))]
    public class ApproachWaypointOrDamageSystem : SystemBase
    {
        private SurfaceDefinitionSingleton surfaceDefinition;
        private OrganVolumetricWorld durabilityWorld;

        private CommandBufferModifierHandle universalLayerWriter;

        protected override void OnCreate()
        {
            base.OnCreate();
            RefreshGameObjectReferences();
            SaveSystemHooks.Instance.PostLoad += RefreshGameObjectReferences;

            RefreshVolumetricWriters();
        }

        private void RefreshGameObjectReferences()
        {
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            durabilityWorld = GameObject.FindObjectOfType<OrganVolumetricWorld>();
        }
        private void RefreshVolumetricWriters()
        {
            if (this.universalLayerWriter?.IsDisposed ?? true)
            {
                universalLayerWriter = durabilityWorld.GetCommandBufferWritableHandle();
            }
        }

        protected override void OnDestroy()
        {
            durabilityWorld.DisposeWritableHandle(universalLayerWriter);
        }

        protected override void OnUpdate()
        {
            var simSpeed = surfaceDefinition.gameSpeed.CurrentValue;
            var deltaTime = simSpeed * Time.DeltaTime;
            var voxelLayers = durabilityWorld.NativeVolumeData.openReadData.AsReadOnly();
            var patherHeight = surfaceDefinition.patherHeight;
            var layout = durabilityWorld.VoxelLayout;

            var damageLayerId = durabilityWorld.damageLayer.voxelLayerId;

            RefreshVolumetricWriters();
            var nativeWriter = universalLayerWriter.GetNextNativeWritableHandle(Matrix4x4.identity);

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
                        var voxelIndex = layout.GetVoxelIndexFromCoordinates(voxel);
                        var durability = voxelLayers[voxelIndex, 0];
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
                            var voxelIndex = layout.GetVoxelIndexFromCoordinates(voxel);
                            var durability = voxelLayers[voxelIndex, 0];
                            nativeWriter.AppendAmountChangeToOtherLayer(voxelIndex, durability * damagePerDurability, damageLayerId);
                        }
                        health.currentHealth -= damageInfo.selfDamageDoneByAttacking * deltaTime;
                    }
                }).Schedule();

            universalLayerWriter.RegisterWriteDependency(this.Dependency);
            durabilityWorld.NativeVolumeData.RegisterReadingDependency(this.Dependency);
        }
    }
}