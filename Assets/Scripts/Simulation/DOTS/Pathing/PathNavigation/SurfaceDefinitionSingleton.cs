using Dman.ReactiveVariables;
using Environment;
using UnityEngine;

namespace Simulation.DOTS.Pathing.PathNavigaton
{
    public class SurfaceDefinitionSingleton : MonoBehaviour
    {
        public float patherHeight = 1.5f;
        public PerlineSampler terrainHeights;

        public FloatReference gameSpeed;

    }
}