using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.PlantPathing;
using Dman.ReactiveVariables;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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