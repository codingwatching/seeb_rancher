using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Simulation.VoxelLayers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Simulation
{
    public class DamagedVoxelsVFXBinder : MonoBehaviour
    {
        public OrganDamageWorld damageWorld;
        public OrganVolumetricWorld durabilityWorld;

        public VolumetricResourceLayer wetnessLayer;


        public string damageTextureName;
        public string durabilityTextureName;
        public string wetnessTextureName;

        public string rainTextureName;
        public RainToTerrainEffect rainEffect;

        public string terrainHeightTextureName;
        public PerlinSampler terrainHeights;

        public string voxelWorldSizeName;
        public string voxelResolutionName;
        public string voxelWorldOriginName;

        public Material damageShaderMaterial;
        public Material dirtDisplayMaterial;
        public VisualEffect volumetricEffect;
        public VisualEffect surfaceEffect;


        private Texture3D damageTexture;
        private Texture3D durabilityTexture;
        private Texture3D wetnessTexture;
        private Texture2D surfaceRainTexture;
        private Texture2D surfaceTerrainHeightTexture;

        private void Awake()
        {
            SetupVoxelTexture();
            damageWorld.onDamageDataUpdated += DamageDataChanged;
            durabilityWorld.volumeWorldChanged += DamageDataChanged;
        }

        private void Start()
        {
            var voxelLayout = durabilityWorld.VoxelLayout;
            var heightData = new NativeArray<float>(voxelLayout.totalTiles, Allocator.TempJob);
            var sampler = terrainHeights.AsNativeCompatible();
            var heightCalculator = new SamplePerlinToData
            {
                surfaceDataOutput = heightData,
                layout = voxelLayout,
                sampler = sampler,
            };

            var heightCalcDep = heightCalculator.Schedule(voxelLayout.totalTiles, 100);
            heightCalcDep.Complete();
            surfaceTerrainHeightTexture.SetPixelData<float>(heightData, 0);
            surfaceTerrainHeightTexture.Apply();

            heightData.Dispose();
            sampler.Dispose();
        }

        private void Update()
        {
            if (damageDataHasUpdate)
            {
                damageDataHasUpdate = false;
                var damageData = damageWorld.GetDamageValuesReadSafe();
                var layerData = durabilityWorld.NativeVolumeData.openReadData;
                var voxelLayout = durabilityWorld.VoxelLayout;

                var durabilityData = new NativeArray<float>(voxelLayout.totalVoxels, Allocator.TempJob);
                var durabilityCopyJob = new CopyVoxelToWorkingDataJob
                {
                    layerData = layerData,
                    layerId = 0,
                    targetData = durabilityData
                };
                var durabilityDep = durabilityCopyJob.Schedule(voxelLayout.totalVoxels, 1000);

                var wetnessData = new NativeArray<float>(voxelLayout.totalVoxels, Allocator.TempJob);
                var wetnessCopyJob = new CopyVoxelToWorkingDataJob
                {
                    layerData = layerData,
                    layerId = wetnessLayer.voxelLayerId,
                    targetData = wetnessData
                };
                var wetnessDep = wetnessCopyJob.Schedule(voxelLayout.totalVoxels, 1000);


                damageTexture.SetPixelData<float>(damageData, 0);
                damageTexture.Apply();

                // TODO: make this async?
                durabilityDep.Complete();
                durabilityTexture.SetPixelData<float>(durabilityData, 0);
                durabilityTexture.Apply();
                durabilityData.Dispose();

                wetnessDep.Complete();
                wetnessTexture.SetPixelData<float>(wetnessData, 0);
                wetnessTexture.Apply();
                wetnessData.Dispose();

                var rainAmount = rainEffect.GetRainAmountArrayWritable();
                rainEffect.GetDependencyNeededToRead().Complete();

                surfaceRainTexture.SetPixelData(rainAmount, 0);
                surfaceRainTexture.Apply();

            }
        }

        private void OnDestroy()
        {
            damageWorld.onDamageDataUpdated -= DamageDataChanged;
            durabilityWorld.volumeWorldChanged -= DamageDataChanged;
        }

        private bool damageDataHasUpdate;
        private void DamageDataChanged()
        {
            damageDataHasUpdate = true;
        }

        private void SetupVoxelTexture()
        {
            var voxels = damageWorld.volumeWorld.VoxelLayout;
            var textureSize = voxels.worldResolution;

            if (damageTexture != null)
            {
                damageTexture = null;
            }
            damageTexture = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.RFloat, 0);
            damageTexture.filterMode = FilterMode.Point;

            durabilityTexture = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.RFloat, 0);
            durabilityTexture.filterMode = FilterMode.Point;

            wetnessTexture = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.RFloat, 0);
            wetnessTexture.filterMode = FilterMode.Point;

            surfaceRainTexture = new Texture2D(textureSize.x, textureSize.z, TextureFormat.RFloat, false);
            surfaceRainTexture.filterMode = FilterMode.Point;

            surfaceTerrainHeightTexture = new Texture2D(textureSize.x, textureSize.z, TextureFormat.RFloat, false);
            surfaceTerrainHeightTexture.filterMode = FilterMode.Bilinear;


            volumetricEffect.SetTexture(durabilityTextureName, durabilityTexture);
            volumetricEffect.SetTexture(damageTextureName, damageTexture);
            volumetricEffect.SetVector3(voxelWorldSizeName, voxels.worldSize);
            volumetricEffect.SetVector3(voxelResolutionName, voxels.worldResolution);
            volumetricEffect.SetVector3(voxelWorldOriginName, voxels.voxelOrigin);

            surfaceEffect.SetTexture(rainTextureName, surfaceRainTexture);
            surfaceEffect.SetTexture(terrainHeightTextureName, surfaceTerrainHeightTexture);
            surfaceEffect.SetVector3(voxelWorldSizeName, voxels.worldSize);
            surfaceEffect.SetVector3(voxelResolutionName, voxels.worldResolution);
            surfaceEffect.SetVector3(voxelWorldOriginName, voxels.voxelOrigin);

            damageShaderMaterial.SetTexture(damageTextureName, damageTexture);
            damageShaderMaterial.SetTexture(durabilityTextureName, durabilityTexture);
            damageShaderMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            damageShaderMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            damageShaderMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);


            dirtDisplayMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            dirtDisplayMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            dirtDisplayMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);
            dirtDisplayMaterial.SetTexture(wetnessTextureName, wetnessTexture);
        }

        struct SamplePerlinToData : IJobParallelFor
        {
            public NativeArray<float> surfaceDataOutput;
            [ReadOnly]
            public PerlinSamplerNativeCompatable sampler;
            public VolumetricWorldVoxelLayout layout;

            public void Execute(int index)
            {
                var tileIndex = new TileIndex(index);
                var tileCoordinate = layout.SurfaceGetCoordinatesFromTileIndex(tileIndex);
                var tilePosition = layout.SurfaceToCenterOfTile(tileCoordinate);
                var sampleHeight = sampler.SampleNoise(tilePosition);

                surfaceDataOutput[tileIndex.Value] = sampleHeight;
            }
        }
    }
}