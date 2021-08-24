using Dman.LSystem.SystemRuntime.ThreadBouncer;
using Dman.LSystem.SystemRuntime.VolumetricData;
using MyUtilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Simulation.DOTS.Pathing
{
    [RequireComponent(typeof(OrganVolumetricWorld))]
    public class PlantPathingWorld : MonoBehaviour
    {
        public Vector3Int pathingTargetVoxel;
        public float baseTravelWeight;
        public bool drawGizmos = true;

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
            var voxelLayout = volumeWorld.VoxelLayout;

            // TODO: don't recyle memory, reuse!
            var parentNodePointers = new NativeArray<int>(voxelLayout.totalVoxels, Allocator.Persistent);
            var tmpPathingData = new NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData>(voxelLayout.totalVoxels, Allocator.TempJob);

            var initializerJob = new DijkstrasPathingSolverInitializerJob
            {
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData
            };

            var dep = initializerJob.Schedule(parentNodePointers.Length, 1000);
            var frontNodes = new NativeQueue<VoxelIndex>(Allocator.TempJob);
            var patherJob = new DijkstrasPathingSolverJob
            {
                layerData = volumeWorld.NativeVolumeData.openReadData,
                costAdjustmentLayer = 0,
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData,
                frontNodes = frontNodes,
                failedWithInfiniteLoop = failedWithInfiniteLoop,

                voxelLayout = voxelLayout,
                targetVoxel = pathingTargetVoxel,
                baseTravelWeight = baseTravelWeight
            };

            dep = patherJob.Schedule(dep);

            parentNodePointersSwapper.AssignPending(parentNodePointers);

            volumeWorld.NativeVolumeData.RegisterReadingDependency(dep);
            pathingWorldPendingUpdate = dep;

            tmpPathingData.Dispose(dep);
            frontNodes.Dispose(dep);

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
            failedWithInfiniteLoop = new NativeArray<bool>(1, Allocator.TempJob);
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
            var voxelLayout = volumeWorld.VoxelLayout;

            Gizmos.color = new Color(1, 0, 0, 1);
            for (VoxelIndex i = VoxelIndex.Zero; i.Value < voxelLayout.totalVoxels; i.Value++)
            {
                var parentIndex = new VoxelIndex(parentIndexes[i.Value]);

                var arrowOrigin = voxelLayout.GetWorldPositionFromVoxelIndex(i);
                var arrowDestination = voxelLayout.GetWorldPositionFromVoxelIndex(parentIndex);

                var diff = arrowDestination - arrowOrigin;
                DrawArrow.ForGizmo(arrowOrigin, diff * 0.9f);
            }
        }
    }
}