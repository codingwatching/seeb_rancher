using Assets.Scripts.GreenhouseLoader;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.PlantPathing.PathNavigaton
{
    public class SurfaceDefinitionSingleton : MonoBehaviour
    {
        public float patherHeight = 1.5f;
        public PerlineSampler terrainHeights;

        public FloatReference gameSpeed;

    }
}