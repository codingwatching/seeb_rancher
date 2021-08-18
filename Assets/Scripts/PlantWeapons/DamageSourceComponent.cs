﻿using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons
{
    [GenerateAuthoringComponent]
    public struct DamageSourceComponent : IComponentData
    {
        public float baseDamage;
    }
}