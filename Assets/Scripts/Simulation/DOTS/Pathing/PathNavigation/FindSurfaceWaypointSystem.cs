using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
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
            var voxelLayout = pathingWorld.volumeWorld.voxelLayout;
            var terrainSampler = pathingWorld.terrainSampler.AsNativeCompatible(Allocator.TempJob);

            Entities
                .WithNone<SurfaceWaypointTarget>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation position, ref SurfaceWaypointFinder finder) =>
                {
                    var currentId = voxelLayout.SurfaceGetDataIndexFromWorldPosition(position.Value);
                    if (currentId < 0)
                    {
                        return;
                    }
                    var nextId = parentPointers[currentId];
                    var nextCoordinate = voxelLayout.SurfaceGetTilePositionFromDataIndex(nextId);
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
                    var currentId = voxelLayout.SurfaceGetDataIndexFromWorldPosition(position.Value);
                    if (currentId < 0)
                    {
                        return;
                    }
                    var nextId = parentPointers[currentId];
                    if (nextId < 0)
                    {
                        return;
                    }
                    var nextCoordinate = voxelLayout.SurfaceGetTilePositionFromDataIndex(nextId);
                    var noiseSample = terrainSampler.SampleNoise(nextCoordinate);

                    target.target = new float3(nextCoordinate.x, noiseSample + finder.waypointOffsetFromSurface, nextCoordinate.y);
                }).ScheduleParallel();

            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
            pathingWorld.RegisterJobHandleReaderOfActiveData(this.Dependency);

            terrainSampler.Dispose(this.Dependency);
        }
    }
}