﻿using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons
{
    public class ScaleWithLifetimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // TODO: detect if component will have non uniform scale, uniform scale, or something else
            dstManager.AddComponentData(entity, new ScaleWithLifetimeComponent
            {
                 baseScale = transform.localScale.x
            });
        }
    }

    public struct ScaleWithLifetimeComponent : IComponentData
    {
        public float baseScale;
    }
}