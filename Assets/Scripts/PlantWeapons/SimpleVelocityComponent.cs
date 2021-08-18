using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons
{
    /// <summary>
    /// custom simple velocity component 
    /// </summary>
    public struct SimpleVelocityComponent : IComponentData
    {
        public float3 velocity;
    }
}