using Assets.Scripts.PlantPathing;
using Assets.Scripts.PlantPathing.PathNavigaton;
using Assets.Scripts.PlantWeapons.Health;
using System.Collections;
using Unity.Burst;
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

        private EntityCommandBufferSystem commandBufferSystem;
        private SurfaceDefinitionSingleton surfaceDefinition;
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        StepPhysicsWorld m_StepPhysicsWorldSystem;
        EntityQuery m_TriggerGravityGroup;

        protected override void OnCreate()
        {
            
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
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
                typeof(DamageOnCollisionTriggerComponent),
                typeof(HealthComponent)
                }
            });
        }

        [BurstCompile]
        struct TriggerDamageCollisionJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<DamageOnCollisionTriggerComponent> TriggerDamageGroup;
            [ReadOnly] public ComponentDataFromEntity<DamageSourceComponent> DamageSourceData;
            [ReadOnly] public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;
            public ComponentDataFromEntity<HealthComponent> HealthComponentGroup;
            public EntityCommandBuffer ecb;


            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                bool isBodyATrigger = TriggerDamageGroup.HasComponent(entityA);
                bool isBodyBTrigger = TriggerDamageGroup.HasComponent(entityB);

                // Ignoring Triggers overlapping other Triggers
                if (isBodyATrigger && isBodyBTrigger)
                    return;

                bool isBodyADynamic = PhysicsVelocityGroup.HasComponent(entityA);
                bool isBodyBDynamic = PhysicsVelocityGroup.HasComponent(entityB);

                // Ignoring overlapping static bodies
                if ((isBodyATrigger && !isBodyBDynamic) ||
                    (isBodyBTrigger && !isBodyADynamic))
                    return;

                var damageReciever = isBodyATrigger ? entityA : entityB;
                var damageGiver = isBodyATrigger ? entityB : entityA;

                

                var damageSource = DamageSourceData[damageGiver];
                var damagedHealth = HealthComponentGroup[damageReciever];

                // damage the trigger
                {
                    damagedHealth.currentHealth -= damageSource.baseDamage;
                    HealthComponentGroup[damageReciever] = damagedHealth;
                }
                // destroy the damager
                {
                    ecb.DestroyEntity(damageGiver);
                }
            }
        }

        protected override void OnUpdate()
        {
            if (m_TriggerGravityGroup.CalculateEntityCount() == 0)
            {
                return;
            }
            var ecb = commandBufferSystem.CreateCommandBuffer();//.AsParallelWriter();

            Dependency = new TriggerDamageCollisionJob
            {
                ecb = ecb,

                TriggerDamageGroup = GetComponentDataFromEntity<DamageOnCollisionTriggerComponent>(true),
                DamageSourceData = GetComponentDataFromEntity<DamageSourceComponent>(true),
                PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(true),
                HealthComponentGroup = GetComponentDataFromEntity<HealthComponent>(false),
            }.Schedule(m_StepPhysicsWorldSystem.Simulation,
                ref m_BuildPhysicsWorldSystem.PhysicsWorld, Dependency);

            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}