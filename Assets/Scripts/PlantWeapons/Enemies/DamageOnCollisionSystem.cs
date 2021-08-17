using Assets.Scripts.PlantPathing;
using Assets.Scripts.PlantPathing.PathNavigaton;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Enemies
{
    // This system sets the PhysicsGravityFactor of any dynamic body that enters a Trigger Volume.
    // A Trigger Volume is defined by a PhysicsShapeAuthoring with the `Is Trigger` flag ticked and a
    // TriggerGravityFactor behaviour added.
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class DamageOnCollisionSystem : SystemBase
    {

        private SurfaceDefinitionSingleton surfaceDefinition;
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        StepPhysicsWorld m_StepPhysicsWorldSystem;
        EntityQuery m_TriggerGravityGroup;

        protected override void OnCreate()
        {
            
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

            var group = World.GetOrCreateSystem<FixedStepSimulationSystemGroup>();
            Debug.Log("Physics timestep: " + group.Timestep);
            group.Timestep = 0.05f;
            Debug.Log("new timestep: " + group.Timestep);

            m_TriggerGravityGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                typeof(DamageOnCollisionTriggerComponent)
                }
            });
        }

        // TODO: this system scales acceleration by timescale, but velocity will still execute at a normal timescale
        //  might have to just use global time.timeScale to set it up
        protected override void OnUpdate()
        {
        }
    }
}