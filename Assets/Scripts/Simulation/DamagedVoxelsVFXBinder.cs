using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.VFX;

namespace Simulation
{
    [RequireComponent(typeof(VisualEffect))]
    public class DamagedVoxelsVFXBinder : MonoBehaviour
    {
        public OrganDamageWorld damageWorld;
        public OrganVolumetricWorld durabilityWorld;
        public Material damageShaderMaterial;
        public Material dirtDisplayMaterial;

        public VolumetricResourceLayer wetnessLayer;

        public string damageTextureName;
        public string durabilityTextureName;
        public string wetnessTextureName;

        public string voxelWorldSizeName;
        public string voxelResolutionName;
        public string voxelWorldOriginName;

        private VisualEffect effect => this.GetComponent<VisualEffect>();


        private Texture3D damageTexture;
        private Texture3D durabilityTexture;
        private Texture3D wetnessTexture;

        private void Awake()
        {
            SetupVoxelTexture();
            damageWorld.onDamageDataUpdated += DamageDataChanged;
            durabilityWorld.volumeWorldChanged += DamageDataChanged;
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

                // TODO: make this async
                durabilityDep.Complete();
                durabilityTexture.SetPixelData<float>(durabilityData, 0);
                durabilityTexture.Apply();
                durabilityData.Dispose();

                wetnessDep.Complete();
                wetnessTexture.SetPixelData<float>(wetnessData, 0);
                wetnessTexture.Apply();
                wetnessData.Dispose();

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
            effect.SetTexture(damageTextureName, damageTexture);

            durabilityTexture = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.RFloat, 0);
            durabilityTexture.filterMode = FilterMode.Point;
            effect.SetTexture(durabilityTextureName, durabilityTexture);

            wetnessTexture = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.RFloat, 0);
            wetnessTexture.filterMode = FilterMode.Point;
            dirtDisplayMaterial.SetTexture(wetnessTextureName, wetnessTexture);


            effect.SetVector3(voxelWorldSizeName, voxels.worldSize);
            effect.SetVector3(voxelResolutionName, voxels.worldResolution);
            effect.SetVector3(voxelWorldOriginName, voxels.voxelOrigin);

            damageShaderMaterial.SetTexture(damageTextureName, damageTexture);
            damageShaderMaterial.SetTexture(durabilityTextureName, durabilityTexture);
            damageShaderMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            damageShaderMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            damageShaderMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);


            dirtDisplayMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            dirtDisplayMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            dirtDisplayMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);
        }
    }
}