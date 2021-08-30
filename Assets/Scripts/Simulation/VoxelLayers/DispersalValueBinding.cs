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
    public class DispersalValueBinding : MonoBehaviour
    {
        [Range(0, 1)]
        public float dispersalFactor;
        public TerrainBoundDispersalEffect dispersalEffect;

        private void Update()
        {
            dispersalEffect.dispersalAdjustmentFactor = dispersalFactor;
        }
    }
}