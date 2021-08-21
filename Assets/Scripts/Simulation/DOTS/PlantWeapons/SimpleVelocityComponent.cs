using Unity.Entities;
using Unity.Mathematics;

namespace Simulation.DOTS.PlantWeapons
{
    /// <summary>
    /// custom simple velocity component 
    /// </summary>
    public struct SimpleVelocityComponent : IComponentData
    {
        public float3 velocity;
    }
}