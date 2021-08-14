using Dman.LSystem.SystemRuntime.ThreadBouncer;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Scripts.PlantPathing
{
    [RequireComponent(typeof(VisualEffect))]
    public class DamagedVoxelsVFXBinder : MonoBehaviour
    {
        public OrganDamageWorld damageWorld;
        public OrganVolumetricWorld durabilityWorld;
        public Material damageShaderMaterial;

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
                var damageData = damageWorld.GetDamageValuesReadonly();
                var durabilityData = durabilityWorld.nativeVolumeData.openReadData;
                damageTexture.SetPixelData<float>(damageData, 0);
                durabilityTexture.SetPixelData<float>(durabilityData, 0);
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
            var voxels = damageWorld.volumeWorld.voxelLayout;
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
        }
    }
}