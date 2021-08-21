using Assets.Scripts.GreenhouseLoader;
using Dman.LSystem.SystemRuntime.ThreadBouncer;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{
    [RequireComponent(typeof(OrganVolumetricWorld))]
    public class PlantSurfacePathingWorld : MonoBehaviour
    {
        public Vector2Int targetSurfacePoint;
        public float baseTravelWeight;
        public bool drawGizmos = true;

        public PerlineSampler terrainSampler;
        public float patherHeight = 1.9f;

        private NativeDisposableHotSwap<NativeArrayNativeDisposableAdapter<int>> parentNodePointersSwapper;
        private JobHandle? pathingWorldPendingUpdate;

        private NativeArray<bool> failedWithInfiniteLoop;

        public OrganVolumetricWorld volumeWorld => GetComponent<OrganVolumetricWorld>();
        public NativeArray<int>? activeParentNodeData => parentNodePointersSwapper?.ActiveData?.Data;


        private void UpdatePathingWorld()
        {
            if (pathingWorldPendingUpdate.HasValue)
            {
                CompletePathingJob();
            }

            var volumeWorld = GetComponent<OrganVolumetricWorld>();
            var voxelLayout = volumeWorld.voxelLayout;

            // TODO: don't recyle memory, reuse!
            var parentNodePointers = new NativeArray<int>(voxelLayout.totalSurfaceDataSize, Allocator.Persistent);
            var tmpPathingData = new NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData>(voxelLayout.totalSurfaceDataSize, Allocator.TempJob);

            var initializerJob = new DijkstrasPathingSolverInitializerJob
            {
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData
            };
            var dep = initializerJob.Schedule(parentNodePointers.Length, 1000);

            var caluclatedSurfaceWeights = new NativeArray<float>(voxelLayout.totalSurfaceDataSize, Allocator.TempJob);
            var nativePerlineSampler = terrainSampler.AsNativeCompatible(Allocator.TempJob);

            var surfaceWeightCalculator = new DijkstrasPathingOverSurfaceSolverProprocessingJob
            {
                nodeCostAdjustmentVoxels = volumeWorld.nativeVolumeData.openReadData,
                calculatedSurfaceWeights = caluclatedSurfaceWeights,
                terrainSampler = nativePerlineSampler,
                voxelLayout = voxelLayout,
                patherHeight = patherHeight
            };

            dep = surfaceWeightCalculator.Schedule(dep);

            volumeWorld.nativeVolumeData.RegisterReadingDependency(dep);

            var frontNodes = new NativeQueue<int>(Allocator.TempJob);
            var patherJob = new DijkstrasPathingOverSurfaceSolverJob
            {
                calculatedSurfaceWeights = caluclatedSurfaceWeights,
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData,
                frontNodes = frontNodes,
                failedWithInfiniteLoop = failedWithInfiniteLoop,

                voxelLayout = voxelLayout,
                targetSurface = targetSurfacePoint,
                baseTravelWeight = baseTravelWeight
            };

            dep = patherJob.Schedule(dep);

            parentNodePointersSwapper.AssignPending(parentNodePointers);

            pathingWorldPendingUpdate = dep;

            tmpPathingData.Dispose(dep);
            frontNodes.Dispose(dep);
            caluclatedSurfaceWeights.Dispose(dep);
            nativePerlineSampler.Dispose(dep);
        }

        public void RegisterJobHandleReaderOfActiveData(JobHandle reader)
        {
            parentNodePointersSwapper.ActiveData.RegisterDependencyOnData(reader);
        }

        private void CompletePathingJob()
        {
            pathingWorldPendingUpdate.Value.Complete();
            pathingWorldPendingUpdate = null;

            var infiniteLoopFail = failedWithInfiniteLoop[0];
            if (infiniteLoopFail)
            {
                Debug.LogError("Pathfinding failed with an infinite loop");
            }

            parentNodePointersSwapper.HotSwapToPending();
        }

        private void Awake()
        {
            parentNodePointersSwapper = new NativeDisposableHotSwap<NativeArrayNativeDisposableAdapter<int>>();
            volumeWorld.volumeWorldChanged += UpdatePathingWorld;
            failedWithInfiniteLoop = new NativeArray<bool>(1, Allocator.Persistent);
        }

        private void Update()
        {
            if (pathingWorldPendingUpdate.HasValue && pathingWorldPendingUpdate.Value.IsCompleted)
            {
                CompletePathingJob();
            }
        }

        private void OnDestroy()
        {
            volumeWorld.volumeWorldChanged -= UpdatePathingWorld;
            pathingWorldPendingUpdate?.Complete();
            parentNodePointersSwapper.Dispose();
            failedWithInfiniteLoop.Dispose();
        }

        public void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }
            var dataTracker = parentNodePointersSwapper?.ActiveData;
            if (dataTracker == null)
            {
                return;
            }
            NativeArray<int> parentIndexes = dataTracker.Data;

            var volumeWorld = GetComponent<OrganVolumetricWorld>();
            var voxelLayout = volumeWorld.voxelLayout;

            Gizmos.color = new Color(1, 0, 0, 1);
            for (int i = 0; i < voxelLayout.totalSurfaceDataSize; i++)
            {
                var parentIndex = parentIndexes[i];

                var arrowOrigin = voxelLayout.SurfaceGetTilePositionFromDataIndex(i);
                var arrowDestination = voxelLayout.SurfaceGetTilePositionFromDataIndex(parentIndex);

                var originWithTerrain = new Vector3(arrowOrigin.x, terrainSampler.SampleNoise(arrowOrigin) + 1, arrowOrigin.y);
                var destinationWithTerrain = new Vector3(arrowDestination.x, terrainSampler.SampleNoise(arrowDestination) + 1, arrowDestination.y);

                var diff = destinationWithTerrain - originWithTerrain;
                DrawArrow.ForGizmo(originWithTerrain, diff * 0.9f);
            }
        }
    }
}