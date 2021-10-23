using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Dman.SceneSaveSystem;
using Environment;
using Simulation.VoxelLayers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Simulation
{
    [RequireComponent(typeof(Animator))]
    public class TimeOfDaySaver : MonoBehaviour, ISaveableData
    {
        public string SunlightStateName;

        public string UniqueSaveIdentifier => "TimeOfDay";

        private Animator animator => GetComponent<Animator>();

        public object GetSaveObject()
        {
            var currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            return currentTime;
        }

        public void SetupFromSaveObject(object save)
        {
            var time = (float)save;
            animator.Play(SunlightStateName, 0, time);
        }
    }
}