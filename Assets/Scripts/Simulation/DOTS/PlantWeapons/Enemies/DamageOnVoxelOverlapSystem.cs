using Assets.Scripts.PlantPathing.PathNavigaton;
using Assets.Scripts.PlantWeapons.Enemies;
using Assets.Scripts.PlantWeapons.Health;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class DamageOnVoxelOverlapSystem : SystemBase
    {
        private SurfaceDefinitionSingleton surfaceDefinition;
        private OrganVolumetricWorld durabilityWorld;
        private EntityCommandBufferSystem commandBufferSystem;

        private EntityQuery surfaceTargets;

        protected override void OnCreate()
        {
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            durabilityWorld = GameObject.FindObjectOfType<OrganVolumetricWorld>();
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            surfaceTargets = GetEntityQuery(typeof(HurtOnVoxelMatch), typeof(Translation), typeof(HealthComponent), typeof(TreatVoxelsAsSurface));
        }

        // TODO: if this system is used more heavily (many targets, many attackers), would be faster to sort into voxel grid probably
        protected override void OnUpdate()
        {
            var targetSurfaceEntities = surfaceTargets.ToEntityArray(Allocator.TempJob);

            var ecb = commandBufferSystem.CreateCommandBuffer();
            var voxelLayout = durabilityWorld.voxelLayout;
            var terrainSampler = surfaceDefinition.terrainHeights.AsNativeCompatible();
            if (targetSurfaceEntities.Length > 0)
            {
                var targetEntitiesReadable = targetSurfaceEntities.AsReadOnly();
                Entities
                    .WithAll<AttackOnVoxelMatch, TreatVoxelsAsSurface>()
                    .ForEach((Entity entity, in Translation position, in AttackOnVoxelMatch attacker) =>
                    {
                        Vector2Int myCoordiante = voxelLayout.SurfaceGetSurfaceCoordinates(position.Value);

                        for (int i = 0; i < targetEntitiesReadable.Length; i++)
                        {
                            Entity targetEntity = targetEntitiesReadable[0];
                            float3 targetPosition = GetComponent<Translation>(targetEntity).Value;
                            Vector2Int targetCoordinate = voxelLayout.SurfaceGetSurfaceCoordinates(targetPosition);

                            if (targetCoordinate == myCoordiante)
                            {
                                var targetHealth = GetComponent<HealthComponent>(targetEntity);
                                targetHealth.currentHealth -= attacker.damageDealt;
                                SetComponent(targetEntity, targetHealth);

                                ecb.DestroyEntity(entity);
                                return;
                            }
                        }
                    }).Schedule();
            }
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
            targetSurfaceEntities.Dispose(this.Dependency);
            terrainSampler.Dispose(this.Dependency);
        }
    }
}