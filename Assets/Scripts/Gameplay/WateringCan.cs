using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Dman.ReactiveVariables;
using Environment;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay
{
    public class WateringCan : MonoBehaviour
    {
        public VolumetricResourceLayer waterLayer;
        public OrganVolumetricWorld volumeWorld;
        public PerlinSampler terrainSampler;

        public float waterFlowRate;
        public float maximumWaterAmount;
        public Vector2Int voxelWaterSize;
        public int wateringDepth = 1;
        public Transform wateringCanRenderer;
        public bool isWatering = true;

        public FloatVariable waterLeft;

        private NativeArray<float> actualAmountWatered;
        private JobHandle? wateringDep;

        private void Awake()
        {
            actualAmountWatered = new NativeArray<float>(1, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            wateringDep?.Complete();
            wateringDep = null;
            actualAmountWatered.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            var layout = volumeWorld.VoxelLayout;
            var minTilePosition = GetMinWateringCoordinate();

            if (wateringDep.HasValue)
            {
                wateringDep.Value.Complete();
                wateringDep = null;
                waterLeft?.Add(-actualAmountWatered[0]);
            }

            if (isWatering)
            {
                var voxelData = volumeWorld.NativeVolumeData.data;
                var nativeSampler = terrainSampler.AsNativeCompatible();

                actualAmountWatered[0] = 0;

                var addJob = new SurfaceAddJob
                {
                    voxelData = voxelData,
                    totalAmountAdded = actualAmountWatered,

                    sampler = nativeSampler,
                    extraWateringDepth = wateringDepth - 1,

                    layout = layout,
                    addLayer = waterLayer.voxelLayerId,
                    minTile = minTilePosition,
                    tileRectSize = voxelWaterSize,
                    amountPerTile = waterFlowRate * Time.deltaTime / (voxelWaterSize.x * voxelWaterSize.y * wateringDepth),
                    capPerTile = maximumWaterAmount,
                };
                wateringDep = addJob.Schedule(volumeWorld.NativeVolumeData.dataWriterDependencies);
                volumeWorld.NativeVolumeData.RegisterWritingDependency(wateringDep.Value);

                nativeSampler.Dispose(wateringDep.Value);
            }

            var tileCenter = minTilePosition + (voxelWaterSize / 2);
            var tilePos = layout.SurfaceToCenterOfTile(tileCenter);
            var sampleAtPoint = terrainSampler.SampleNoise(tilePos);
            var pointInSpace = new Vector3(tilePos.x, sampleAtPoint, tilePos.y);

            wateringCanRenderer.position = pointInSpace;
        }

        private void OnDrawGizmosSelected()
        {
            var minCenter = GetMinWateringCoordinate();
            var layout = volumeWorld.VoxelLayout;

            Gizmos.color = Color.blue;
            for (int x = 0; x < voxelWaterSize.x; x++)
            {
                for (int y = 0; y < voxelWaterSize.y; y++)
                {
                    var tileCoord = minCenter + new Vector2Int(x, y);
                    var tilePos = layout.SurfaceToCenterOfTile(tileCoord);
                    var sampleAtPoint = terrainSampler.SampleNoise(tilePos);

                    var pointInSpace = new Vector3(tilePos.x, sampleAtPoint, tilePos.y);
                    Gizmos.DrawSphere(pointInSpace, 0.1f);

                    var voxelCoord = layout.GetVoxelCoordinates(pointInSpace);
                    var voxelCenter = layout.CoordinateToCenterOfVoxel(voxelCoord);
                    Gizmos.DrawWireCube(voxelCenter, layout.voxelSize);
                }
            }
        }

        private Vector2Int GetMinWateringCoordinate()
        {
            var layout = volumeWorld.VoxelLayout;
            var tilePosition = layout.SurfaceGetSurfaceCoordinates(transform.position) - (voxelWaterSize / 2);
            tilePosition.Clamp(Vector2Int.zero, new Vector2Int(layout.worldResolution.x, layout.worldResolution.z) - voxelWaterSize);
            return tilePosition;
        }


        struct SurfaceAddJob : IJob
        {
            public VoxelWorldVolumetricLayerData voxelData;
            public NativeArray<float> totalAmountAdded;

            [ReadOnly]
            public PerlinSamplerNativeCompatable sampler;

            public VolumetricWorldVoxelLayout layout;
            public int addLayer;

            public Vector2Int minTile;
            public Vector2Int tileRectSize;
            public int extraWateringDepth;
            public float amountPerTile;
            public float capPerTile;
            public void Execute()
            {
                float totalAdded = 0;
                for (int x = 0; x < tileRectSize.x; x++)
                {
                    for (int y = 0; y < tileRectSize.y; y++)
                    {
                        var tileCoord = minTile + new Vector2Int(x, y);
                        var tilePos = layout.SurfaceToCenterOfTile(tileCoord);
                        var sampleAtPoint = sampler.SampleNoise(tilePos);

                        var pointInSpace = new Vector3(tilePos.x, sampleAtPoint, tilePos.y);
                        var rootVoxelCoord = layout.GetVoxelCoordinates(pointInSpace);

                        var minHeight = math.max(0, rootVoxelCoord.y - extraWateringDepth);
                        var maxHeight = rootVoxelCoord.y;
                        for (int height = minHeight; height <= maxHeight; height++)
                        {
                            var currentVoxelCoord = new Vector3Int(rootVoxelCoord.x, height, rootVoxelCoord.z);

                            var voxelIndex = layout.GetVoxelIndexFromCoordinates(currentVoxelCoord);

                            var currentVoxelAmount = voxelData[voxelIndex, addLayer];
                            var nextAmount = math.min(currentVoxelAmount + amountPerTile, capPerTile);
                            if (nextAmount <= currentVoxelAmount)
                            {
                                continue;
                            }

                            voxelData[voxelIndex, addLayer] = nextAmount;
                            totalAdded += nextAmount - currentVoxelAmount;
                        }

                    }
                }
                totalAmountAdded[0] = totalAdded;
            }
        }
    }
}
