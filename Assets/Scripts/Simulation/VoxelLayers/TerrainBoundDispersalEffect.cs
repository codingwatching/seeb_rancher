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
    [CreateAssetMenu(fileName = "TerrainBoundDispersalEffect", menuName = "LSystem/Resource Layers/TerrainBoundDispersalEffect")]
    public class TerrainBoundDispersalEffect : VolumetricLayerEffect
    {
        public float globalDiffusionConstant = 1f;

        public PerlinSampler terrainSampler;

        [Range(0,1)]
        [Tooltip("this value can be adjusted at runtime to globally adjust the dispersal happening throughout the system. All other dispersal factors are baked once per play session.")]
        public float dispersalAdjustmentFactor = 1f;
        [Range(0, 1)]
        public float underTerrainDispersal = 0f;
        [Range(0, 1)]
        public float intersectTerrainDispersal = 0f;
        [Range(0, 1)]
        public float aboveTerrainDispersal = 0f;


        private NativeArray<float> dispersalFactorByVoxelIndex;
        private JobHandle? dispersalWeightComputeJob = null;
        public override void SetupInternalData(VolumetricWorldVoxelLayout layout)
        {
            dispersalFactorByVoxelIndex = new NativeArray<float>(layout.totalVoxels, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var sampler = terrainSampler.AsNativeCompatible();

            var weightAssignJob = new PerlinSampledVoxelAssignment
            {
                perVoxelIdOutput = dispersalFactorByVoxelIndex,
                sampler = sampler,
                layout = layout,

                underTerrainValue = underTerrainDispersal,
                intersectTerrainValue = intersectTerrainDispersal,
                overTerrainValue = aboveTerrainDispersal
            };
            var dep = weightAssignJob.Schedule(layout.totalTiles, 100);
            sampler.Dispose(dep);

            dispersalWeightComputeJob = dep;
        }

        public override void CleanupInternalData(VolumetricWorldVoxelLayout layout)
        {
            dispersalFactorByVoxelIndex.Dispose();
        }

        public override bool ApplyEffectToLayer(DoubleBuffered<float> layerData, VoxelWorldVolumetricLayerData readonlyLayerData, float deltaTime, ref JobHandleWrapper dependecy)
        {
            if (dispersalWeightComputeJob.HasValue)
            {
                dispersalWeightComputeJob.Value.Complete();
                dispersalWeightComputeJob = null;
            }

            Disperse(layerData, deltaTime, ref dependecy);

            return true;
        }

        protected void Disperse(DoubleBuffered<float> layerData, float deltaTime, ref JobHandleWrapper dependency)
        {
            var dispersal = new DisperseByFactor
            {
                reductionByVoxelIndex = dispersalFactorByVoxelIndex,
                resourceData = layerData.CurrentData,
                expReduction = math.min(math.exp(deltaTime * dispersalAdjustmentFactor) / math.exp(1), 1f) // safeguard against bad behavior if framerate gets too low
            };
            dependency = dispersal.Schedule(layerData.CurrentData.Length, 10000, dependency);
        }

        [BurstCompile]
        struct DisperseByFactor : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> reductionByVoxelIndex;
            public NativeArray<float> resourceData;
            public float expReduction;
            public void Execute(int index)
            {
                resourceData[index] = resourceData[index] * (1 - reductionByVoxelIndex[index] * expReduction);
            }
        }

    }
}