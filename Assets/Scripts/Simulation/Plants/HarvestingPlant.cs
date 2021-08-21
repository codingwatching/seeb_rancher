using Dman.LSystem.UnityObjects;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Simulation.Plants
{
    public class HarvestingPlant : MonoBehaviour
    {
        public GameObject prefabRoot;

        public void StartHarvestEffect(float plantHeight, PlantedLSystem plant)
        {
            var currentState = plant.GetComponent<LSystemBehavior>().steppingHandle.currentState;
            this.GetComponent<MeshFilter>().mesh = plant.GetComponent<MeshFilter>().mesh;
            var renderer = this.GetComponent<MeshRenderer>();


            var harvestEffectMaterialProperties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(harvestEffectMaterialProperties);

            harvestEffectMaterialProperties.SetFloat("startTime", Time.time);
            renderer.SetPropertyBlock(harvestEffectMaterialProperties);


            var waitTime = renderer.material.GetFloat("timeToDissolve");
            Debug.Log("waiting " + waitTime);


            var harvestEffect = this.GetComponent<VisualEffect>();

            // assuming the mesh has been rotated 90 degrees around z axis.
            harvestEffect.SetFloat("height", 10 * plantHeight);
            harvestEffect.Play();

            this.StartCoroutine(HarvestEffect(waitTime));
        }
        public void SetHarvestEffectColor(Color color)
        {
            var renderer = this.GetComponent<MeshRenderer>();
            renderer.material.SetColor("EdgeColor", color);
        }

        private IEnumerator HarvestEffect(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            Destroy(prefabRoot);
        }
    }
}