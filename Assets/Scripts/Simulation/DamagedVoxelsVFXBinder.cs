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
        public Material gridDisplayMaterial;

        public string damageTextureName;
        public string durabilityTextureName;

        public string voxelWorldSizeName;
        public string voxelResolutionName;
        public string voxelWorldOriginName;

        private VisualEffect effect => this.GetComponent<VisualEffect>();


        private Texture3D damageTexture;
        private Texture3D durabilityTexture;

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
                var copyJob = new CopyVoxelToWorkingDataJob
                {
                    layerData = layerData,
                    layerId = 0,
                    targetData = durabilityData
                };
                var dep = copyJob.Schedule(voxelLayout.totalVoxels, 1000);
                
                damageTexture.SetPixelData<float>(damageData, 0);

                // TODO: make this async
                dep.Complete();
                durabilityTexture.SetPixelData<float>(durabilityData, 0);
                durabilityData.Dispose();

                damageTexture.Apply();
                durabilityTexture.Apply();
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


            effect.SetVector3(voxelWorldSizeName, voxels.worldSize);
            effect.SetVector3(voxelResolutionName, voxels.worldResolution);
            effect.SetVector3(voxelWorldOriginName, voxels.voxelOrigin);

            damageShaderMaterial.SetTexture(damageTextureName, damageTexture);
            damageShaderMaterial.SetTexture(durabilityTextureName, durabilityTexture);
            damageShaderMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            damageShaderMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            damageShaderMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);


            gridDisplayMaterial.SetVector(voxelWorldSizeName, voxels.worldSize);
            gridDisplayMaterial.SetVector(voxelResolutionName, (Vector3)voxels.worldResolution);
            gridDisplayMaterial.SetVector(voxelWorldOriginName, voxels.voxelOrigin);
        }
    }
}