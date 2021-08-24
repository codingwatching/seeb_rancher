using Dman.ReactiveVariables;
using Environment;
using UnityEngine;

namespace Simulation.DOTS.Pathing.PathNavigaton
{
    public class SurfaceDefinitionSingleton : MonoBehaviour
    {
        public float patherHeight = 1.5f;
        public PerlinSampler terrainHeights;

        public FloatReference gameSpeed;

    }
}