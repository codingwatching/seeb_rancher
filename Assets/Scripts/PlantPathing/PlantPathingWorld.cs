using Dman.LSystem.SystemRuntime.ThreadBouncer;
using Dman.LSystem.SystemRuntime.VolumetricData;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

namespace Assets.Scripts.PlantPathing
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
            var voxelLayout = volumeWorld.voxelLayout;

            // TODO: don't recyle memory, reuse!
            var parentNodePointers = new NativeArray<int>(voxelLayout.totalVolumeDataSize, Allocator.Persistent);
            var tmpPathingData = new NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData>(voxelLayout.totalVolumeDataSize, Allocator.TempJob);

            var initializerJob = new DijkstrasPathingSolverInitializerJob
            {
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData
            };

            var dep = initializerJob.Schedule(parentNodePointers.Length, 1000);
            var frontNodes = new NativeQueue<int>(Allocator.TempJob);
            failedWithInfiniteLoop = new NativeArray<bool>(1, Allocator.TempJob);
            var patherJob = new DijkstrasPathingSolverJob
            {
                nodeCostAdjustmentVoxels = volumeWorld.nativeVolumeData.openReadData,
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

            volumeWorld.nativeVolumeData.RegisterReadingDependency(dep);
            pathingWorldPendingUpdate = dep;

            tmpPathingData.Dispose(dep);
            frontNodes.Dispose(dep);

        }

        private void CompletePathingJob()
        {
            pathingWorldPendingUpdate.Value.Complete();
            pathingWorldPendingUpdate = null;

            var infiniteLoopFail = failedWithInfiniteLoop[0];
            failedWithInfiniteLoop.Dispose();
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
            var voxelLayout = volumeWorld.voxelLayout;

            Gizmos.color = new Color(1, 0, 0, 1);
            for (int i = 0; i < voxelLayout.totalVolumeDataSize; i++)
            {
                var parentIndex = parentIndexes[i];
                var targetCoordinate = voxelLayout.GetCoordinatesFromDataIndex(i);
                var parentCoordinate = voxelLayout.GetCoordinatesFromDataIndex(parentIndex);

                var arrowOrigin = voxelLayout.CoordinateToCenterOfVoxel(targetCoordinate);
                var arrowDestination = voxelLayout.CoordinateToCenterOfVoxel(parentCoordinate);

                var diff = arrowDestination - arrowOrigin;
                DrawArrow.ForGizmo(arrowOrigin, diff * 0.9f);
            }
        }
    }
}