using Dman.ReactiveVariables;
using Simulation.DOTS.PlantWeapons.Health;
using Unity.Entities;
using UnityEngine;

namespace Gameplay.DOTSDataBinders
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