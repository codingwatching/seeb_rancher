using Assets.Scripts.PlantWeapons;
using Assets.Scripts.PlantWeapons.Health;
using Dman.ReactiveVariables;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTSDataBinders
{
    [RequireComponent(typeof(HealthComponent))]
    public class HealthToFloatVariableBinder : MonoBehaviour, IConvertGameObjectToEntity
    {
        public FloatVariable target;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentObject(entity, this);
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class HealthToFloatVariableBindingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((
                in HealthToFloatVariableBinder binding,
                in HealthComponent health) =>
                {
                    binding.target.SetValue(health.currentHealth);
                }).WithoutBurst().Run();
        }
    }
}