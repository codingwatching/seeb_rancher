using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons
{
    public struct VelocityComponent : IComponentData
    {
        public float3 velocity;
    }
}