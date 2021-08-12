using Dman.LSystem.SystemRuntime.VolumetricData;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{
    [RequireComponent(typeof(OrganVolumetricWorld))]
    public class PlantPathingWorld : MonoBehaviour
    {

        private void UpdatePathingWorld()
        {

        }

        private void Awake()
        {
            GetComponent<OrganVolumetricWorld>().volumeWorldChanged += UpdatePathingWorld;
        }

        private void OnDestroy()
        {
            GetComponent<OrganVolumetricWorld>().volumeWorldChanged -= UpdatePathingWorld;
        }
    }
}