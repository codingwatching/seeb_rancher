using Assets.Scripts.PlantPathing.PathNavigaton;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.PlantWeapons.Bomb
{
    [UpdateBefore(typeof(ApplyVelocitySystem))]
    [UpdateAfter(typeof(FindEnemySystem))]
    public class SeekEnemySystem : SystemBase
    {

        private SurfaceDefinitionSingleton surfaceDefinition;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            surfaceDefinition = GameObject.FindObjectOfType<SurfaceDefinitionSingleton>();
            commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }
        public static void ApplyForce(ref PhysicsVelocity pv, PhysicsMass pm,
       Translation t, Rotation r, float3 force, float3 point, float deltaTime)
        {
            // Linear
            pv.Linear += force * deltaTime;

            // Angular
            {
                // Calculate point impulse
                var worldFromEntity = new RigidTransform(r.Value, t.Value);
                var worldFromMotion = math.mul(worldFromEntity, pm.Transform);
                float3 angularImpulseWorldSpace = math.cross(point, force);
                float3 angularImpulseInertiaSpace = math.rotate(math.inverse(worldFromMotion.rot), angularImpulseWorldSpace);

                //var lerped = math.lerp()

                pv.Angular = angularImpulseInertiaSpace * pm.InverseInertia * 0.1f;// * deltaTime;
            }
        }

        public static void Seek(
            float deltaTime,
                    ref PhysicsVelocity velocity,
                    in SeekEnemyComponent seeker,
                    in Translation selfPosition,
                    in PhysicsMass massData,
                    in Rotation rot,
                    float3 unityVectorToTarget,
                    out float3 resultForwardVector)
        {
            var worldFromEntity = new RigidTransform(rot.Value, selfPosition.Value);
            var worldFromMotion = math.mul(worldFromEntity, massData.Transform);

            var velocityLocalSpace = new float3(1, 0, 0);
            var velocityWorldSpace = math.rotate(worldFromMotion, velocityLocalSpace);

            velocity.Linear += velocityWorldSpace * deltaTime * seeker.seekAcceleration;
            resultForwardVector = velocityWorldSpace;
            // angular velocity
            {
                var toTargetLocalSpace = math.rotate(math.inverse(worldFromMotion), unityVectorToTarget);

                var lookDirectionLocalSpace = math.cross(toTargetLocalSpace, new float3(-1, 0, 0));
                velocity.Angular += lookDirectionLocalSpace * deltaTime * seeker.rotateAcceleration;
            }
        }

        // TODO: this system scales acceleration by timescale, but velocity will still execute at a normal timescale
        //  might have to just use global time.timeScale to set it up
        protected override void OnUpdate()
        {
            var ecb = commandBufferSystem.CreateCommandBuffer();
            var simSpeed = surfaceDefinition.gameSpeed.CurrentValue;
            var deltaTime = Time.DeltaTime * simSpeed;
            var totalTime = Time.ElapsedTime;
            Entities
                .ForEach((
                    Entity entity,
                    ref PhysicsVelocity velocity,
                    ref SmokeTrailSpawnerComponent spawner,
                    in FoundTargetEntity target,
                    in SeekEnemyComponent seeker,
                    in Translation selfPosition,
                    in PhysicsMass massData,
                    in Rotation rot) =>
                {
                    if (HasComponent<Translation>(target.target))
                    {
                        var targetPos = GetComponent<Translation>(target.target);
                        var vectorToTarget = targetPos.Value - selfPosition.Value;
                        if (HasComponent<SimpleVelocityComponent>(target.target))
                        {
                            var distToTarget = math.length(vectorToTarget);
                            var leadMagnitude = distToTarget * seeker.targetLeadDistancePerDisplacement;
                            var targetVelocity = GetComponent<SimpleVelocityComponent>(target.target);
                            var leadVector = targetVelocity.velocity * leadMagnitude;
                            vectorToTarget += leadVector;
                        }

                        Seek(deltaTime,
                            ref velocity,
                            seeker,
                            selfPosition,
                            massData,
                            rot,
                            math.normalize(vectorToTarget),
                            out var forwardVector);

                        // smoke emit
                        {

                            if (spawner.lastSpawnTime + spawner.timeToSpawn < totalTime)
                            {
                                spawner.lastSpawnTime = (float)totalTime;

                                var newSmoke = ecb.Instantiate(spawner.prefab);
                                ecb.SetComponent(newSmoke, new SimpleVelocityComponent
                                {
                                    velocity = velocity.Linear + forwardVector * spawner.timeToSpawn * spawner.smokeVelocity
                                });
                                ecb.SetComponent(newSmoke, new Translation
                                {
                                    Value = selfPosition.Value
                                });
                            }
                        }
                    }
                    else
                    {
                        ecb.RemoveComponent<FoundTargetEntity>(entity);
                    }
                }).Schedule();

            Entities
                .WithNone<FoundTargetEntity>()
                .ForEach((
                    Entity entity,
                    ref PhysicsVelocity velocity,
                    ref SmokeTrailSpawnerComponent spawner,
                    in Translation selfPosition,
                    in NoTargetComponent target,
                    in SeekEnemyComponent seeker,
                    in PhysicsMass massData,
                    in Rotation rot) =>
                {
                    Seek(deltaTime,
                        ref velocity,
                        seeker,
                        selfPosition,
                        massData,
                        rot,
                        Vector3.Normalize(target.randomTarget - selfPosition.Value),
                        out var forwardVector);

                    // smoke emit
                    {

                        if (spawner.lastSpawnTime + spawner.timeToSpawn < totalTime)
                        {
                            spawner.lastSpawnTime = (float)totalTime;

                            var newSmoke = ecb.Instantiate(spawner.prefab);
                            ecb.SetComponent(newSmoke, new SimpleVelocityComponent
                            {
                                velocity = velocity.Linear + forwardVector * spawner.timeToSpawn * spawner.smokeVelocity
                            });
                            ecb.SetComponent(newSmoke, new Translation
                            {
                                Value = selfPosition.Value
                            });
                        }
                    }
                }).Schedule();
            commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}