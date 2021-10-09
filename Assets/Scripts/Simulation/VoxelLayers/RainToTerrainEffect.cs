using Dman.LSystem;
using Dman.LSystem.SystemRuntime.NativeCollections.NativeVolumetricSpace;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Simulation.VoxelLayers
{
    [CreateAssetMenu(fileName = "RainToTerrainEffect", menuName = "LSystem/Resource Layers/RainToTerrainEffect")]
    public class RainToTerrainEffect : VolumetricLayerEffect
    {
        public float globalRainMultiplier = 1f;
        public int extraPrecipitateDepth = 1;

        public PerlinSampler terrainSampler;


        private NativeArray<float> rainAmountByTileIndex;
        private JobHandle rainAmountWriteHandle;
        private JobHandle rainReadHandles;
        public override void SetupInternalData(VolumetricWorldVoxelLayout layout)
        {
            rainAmountByTileIndex = new NativeArray<float>(layout.totalTiles, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        public override void CleanupInternalData(VolumetricWorldVoxelLayout layout)
        {
            rainAmountByTileIndex.Dispose();
        }

        public override bool ApplyEffectToLayer(DoubleBuffered<float> layerData, VoxelWorldVolumetricLayerData readonlyLayerData, float deltaTime, ref JobHandleWrapper dependecy)
        {
            var voxelLayout = readonlyLayerData.VoxelLayout;
            var nativeSampler = terrainSampler.AsNativeCompatible();

            var rainJob = new RainEffectJob
            {
                voxelDataToRainTo = layerData.CurrentData,
                rainFactorPerTile = rainAmountByTileIndex,
                rainAmountMultiplier = globalRainMultiplier * deltaTime,
                sampler = nativeSampler,
                layout = voxelLayout,

                extraPrecipitateDepth = extraPrecipitateDepth
            };

            dependecy = rainJob.Schedule(voxelLayout.totalTiles, 100, JobHandle.CombineDependencies(dependecy, rainAmountWriteHandle));
            rainReadHandles = JobHandle.CombineDependencies(rainReadHandles, dependecy);

            nativeSampler.Dispose(dependecy);

            return true;
        }

        public NativeArray<float> GetRainAmountArrayWritable()
        {
            return rainAmountByTileIndex;
        }

        public JobHandle GetDependencyNeededToWrite()
        {
            return JobHandle.CombineDependencies(rainReadHandles, rainAmountWriteHandle);
        }
        public JobHandle GetDependencyNeededToRead()
        {
            return rainAmountWriteHandle;
        }

        public void RegisterWritingDependencyOnRainAmount(JobHandle writer)
        {
            this.rainAmountWriteHandle = JobHandle.CombineDependencies(rainAmountWriteHandle, writer);
        }

        struct RainEffectJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<float> voxelDataToRainTo;

            [ReadOnly]
            public NativeArray<float> rainFactorPerTile;
            [ReadOnly]
            public PerlinSamplerNativeCompatable sampler;
            public VolumetricWorldVoxelLayout layout;

            public float rainAmountMultiplier;
            public int extraPrecipitateDepth;

            public void Execute(int index)
            {
                var tileIndex = new TileIndex(index);
                var baseRainAmount = rainFactorPerTile[tileIndex.Value] * rainAmountMultiplier;
                var rainAtTile = baseRainAmount / (1 + extraPrecipitateDepth);
                if (rainAtTile <= 0)
                {
                    return;
                }

                var tileCoordinate = layout.SurfaceGetCoordinatesFromTileIndex(tileIndex);
                var tilePosition = layout.SurfaceToCenterOfTile(tileCoordinate);
                var sampleHeight = sampler.SampleNoise(tilePosition);

                var pointInSpace = new Vector3(tilePosition.x, sampleHeight, tilePosition.y);
                var rootVoxelCoord = layout.GetVoxelCoordinates(pointInSpace);

                var minHeight = math.max(0, rootVoxelCoord.y - extraPrecipitateDepth);
                var maxHeight = rootVoxelCoord.y;
                for (int height = minHeight; height <= maxHeight; height++)
                {
                    var currentVoxelCoord = new Vector3Int(rootVoxelCoord.x, height, rootVoxelCoord.z);

                    var voxelIndex = layout.GetVoxelIndexFromCoordinates(currentVoxelCoord);

                    voxelDataToRainTo[voxelIndex.Value] += rainAtTile;
                }
            }
        }
    }
}