using Assets.Scripts.PlantWeapons;
using Dman.ReactiveVariables;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTSDataBinders
{
    public class HealthToFloatVariableBinder : MonoBehaviour, IConvertGameObjectToEntity
    {
        public FloatVariable target;
        public HealthAuthoring healthEntityTarget;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentObject(entity, target);
        }
    }
}