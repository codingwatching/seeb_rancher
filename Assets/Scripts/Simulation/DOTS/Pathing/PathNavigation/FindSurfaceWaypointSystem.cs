using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.SceneSaveSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Simulation.DOTS.Pathing.PathNavigaton
{
    public class FindSurfaceWaypointSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private Unity.Mathematics.Random random;
        private PlantSurfacePathingWorld pathingWorld;

        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            random = new Unity.Mathematics.Random(987614298);

            RefreshGameObjectReferences();
            SaveSystemHooks.Instance.PostLoad += RefreshGameObjectReferences;
        }

        private void RefreshGameObjectReferences()
        {
            pathingWorld = GameObject.FindObjectOfType<PlantSurfacePathingWorld>();
        }

        protected override void OnUpdate()
        {
            var waypoints = pathingWorld.activeParentNodeData;
            if (waypoints == null)
            {
                return;
            }
            var parentPointers = waypoints.Value.AsReadOnly();

            var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var voxelLayout = pathingWorld.volumeWorld.VoxelLayout;
            var terrainSampler = pathingWorld.terrainSampler.AsNativeCompatible(Allocator.TempJob);

            Entities
                .WithNone<SurfaceWaypointTarget>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation position, ref SurfaceWaypointFinder finder) =>
                {
                    var currentId = voxelLayout.SurfaceGetTileIndexFromWorldPosition(position.Value);
                    if (!currentId.IsValid)
                    {
                        return;
                    }
                    var nextId = new TileIndex(parentPointers[currentId.Value]);
                    var nextCoordinate = voxelLayout.SurfaceGetTilePositionFromTileIndex(nextId);
                    var noiseSample = terrainSampler.SampleNoise(nextCoordinate);

                    ecb.AddComponent(entityInQueryIndex, entity, new SurfaceWaypointTarget
                    {
                        target = new float3(nextCoordinate.x, noiseSample + finder.waypointOffsetFromSurface, nextCoordinate.y)
                    });
                }).ScheduleParallel();

            Entities
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation position, ref SurfaceWaypointFinder finder, ref SurfaceWaypointTarget target) =>
                {
                    var dist = Vector3.Distance(target.target, position.Value);
                    if (dist > finder.waypointProximityRequirement)
                    {
                        return;
                    }
                    var currentId = voxelLayout.SurfaceGetTileIndexFromWorldPosition(position.Value);
                    if (!currentId.IsValid)
                    {
                        return;
                    }
                    var nextId = new TileIndex(parentPointers[currentId.Value]);
                    if (!nextId.IsValid)
                    {
                        return;
                    }
                    var nextCoordinate = voxelLayout.SurfaceGetTilePositionFromTileIndex(nextId);
                    var noiseSample = terrainSampler.SampleNoise(nextCoordinate);

                    target.target = new float3(nextCoordinate.x, noiseSample + finder.waypointOffsetFromSurface, nextCoordinate.y);
                }).ScheduleParallel();

            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
            pathingWorld.RegisterJobHandleReaderOfActiveData(this.Dependency);

            terrainSampler.Dispose(this.Dependency);
        }
    }
}