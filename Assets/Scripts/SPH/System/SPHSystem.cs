using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace SPH.Spawners
{

    [UpdateBefore(typeof(BuildPhysicsWorld))]// This system updates all entities in the scene with both a RotationSpeed and Rotation component.
    public class SPHSystem : JobComponentSystem
    {
        EntityQuery m_SPHQuery;

        [BurstCompile]
        [RequireComponentTag(typeof(LocalToWorld), typeof(PhysicsVelocity), typeof(SPH))]
        struct UpdateSPHSettingsJob : IJobForEachWithEntity<SPH>
        {

            [ReadOnly] public int mass;
            [ReadOnly] public int density;
            [ReadOnly] public int viscosity;

            public void Execute(Entity entity, int index, ref SPH sph)
            {
                sph.mass        = mass;
                sph.density     = density;
                sph.viscosity   = viscosity;
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(LocalToWorld), typeof(PhysicsVelocity), typeof(SPH))]
        struct InitJob : IJobForEachWithEntity<LocalToWorld, PhysicsVelocity, SPH>
        {
            public NativeArray<float> densities;
            public NativeArray<float> pressures;
            public NativeArray<float3> positions;
            public NativeArray<float3> velocities;
            public NativeArray<float3> forces;
            public NativeArray<SPH> sphs;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld, [ReadOnly]ref PhysicsVelocity velocity, [ReadOnly]ref SPH sph)
            {
                densities[index]    = 0.0f;
                pressures[index]    = 0.0f;
                forces[index]       = new float3(0, 0, 0);
                positions[index]    = localToWorld.Position;
                velocities[index]   = velocity.Linear;
                sphs[index]         = sph;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(LocalToWorld), typeof(SPH))]
        struct SPHDensityPressureJob : IJobForEachWithEntity<LocalToWorld, SPH>
        {
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public NativeArray<SPH> sphs;
            [ReadOnly] public int count;

            public NativeArray<float> densities;
            public NativeArray<float> pressures;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld, [ReadOnly]ref SPH sph)
            {

                var density     = 0.0f;
                var mySettings  = sphs[index];
                var myPosition  = positions[index];
                var myDensity   = mySettings.density;

                for (int i = 0; i < count; i++)
                {

                    if (positions[i].x != myPosition.x && positions[i].y != myPosition.y && positions[i].z != myPosition.z) continue;

                    var otherSettings       = sphs[i];
                    var otherRadius         = otherSettings.smoothingRadius;
                    var otherRadiusSquare   = otherSettings.smoothingRadiusSquare;
                    var otherMass           = otherSettings.mass;
                    var separation          = positions[i] - myPosition;
                    var distanceSq          = math.lengthsq(separation);
                    var isPressured         = distanceSq < otherRadiusSquare;

                    if (isPressured)
                    {
                        density     += otherMass * (315.0f / (64.0f * math.PI * math.pow(otherRadius, 9.0f)) * math.pow(otherRadiusSquare - distanceSq, 3.0f));
                    }
                }

                densities[index] = density;
                pressures[index] = 2000.0f * (density - myDensity);

            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(LocalToWorld), typeof(PhysicsVelocity), typeof(SPH))]
        struct SPHForceJob : IJobForEachWithEntity<LocalToWorld, PhysicsVelocity, SPH>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> velocities;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> pressures;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> positions;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<SPH> sphs;
            [ReadOnly] public NativeArray<float> densities;
            public NativeArray<float3> forces;

            [ReadOnly] public int count;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld, [ReadOnly]ref PhysicsVelocity velocity, [ReadOnly]ref SPH sph)
            {

                var forcePressure   = new float3(0, 0, 0);
                var forceViscosity  = new float3(0, 0, 0);
                var mySettings      = sphs[index];
                var myPosition      = positions[index];
                var myVelocity      = velocities[index];

                for (int i = 0; i < count; i++)
                {

                    if (positions[i].x != myPosition.x && positions[i].y != myPosition.y && positions[i].z != myPosition.z)
                    {

                        var other               = sphs[i];
                        var otherVelocity       = velocities[i];
                        var otherPosition       = positions[i];
                        var otherRadius         = other.smoothingRadius;
                        var separation          = otherPosition - myPosition;
                        var distanceSq          = math.lengthsq(separation);
                        var distance            = math.sqrt(distanceSq);
                        var maxSeparation       = otherRadius;
                        var halfMaxSeparation   = maxSeparation / 2.0f;
                        var isInContact         = distance < other.smoothingRadius;

                        if (isInContact)
                        {

                            var penetration         = halfMaxSeparation - distance;
                            var squarePenetration   = penetration * penetration;
                            var surfacePenetration  = 45 * math.PI * squarePenetration;
                            var normalRedirection   = -math.normalize(separation);
                            var deltaVelocity       = otherVelocity - myVelocity;

                            forcePressure           += mySettings.mass * pressures[index] / densities[index] * normalRedirection * 45 * math.PI * (mySettings.smoothingRadius - distance) * (mySettings.smoothingRadius - distance);
                            forceViscosity          += mySettings.viscosity * mySettings.mass * deltaVelocity / densities[index] * surfacePenetration;

                        }
                    }
                }

                var forceGravity    = new float3(0, -9.8f, 0) * densities[index] * 10000.0f;
                forces[index]       = forceGravity + forcePressure + forceViscosity;
            }
        }

        [BurstCompile]
        struct IntegrateJob : IJobForEachWithEntity<PhysicsVelocity, SPH>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> forces;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> densities;
            public float dt;

            public void Execute(Entity entity, int index, ref PhysicsVelocity bodyVelocity, [ReadOnly]ref SPH sph)
            {
                bodyVelocity.Linear += dt * forces[index] / densities[index];
            }
        }

        UI ui;

        protected override void OnCreate()
        {

            ui = GameObject.FindObjectOfType<UI>();

            m_SPHQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<SPH>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadWrite<PhysicsVelocity>()
                }
            });

        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            m_SPHQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<SPH>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadWrite<PhysicsVelocity>()
                }
            });

            int SPHCount = m_SPHQuery.CalculateEntityCount();

            var SPHDensity  = new NativeArray<float>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var SPHPressure = new NativeArray<float>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var SPHPosition = new NativeArray<float3>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var SPHVelocity = new NativeArray<float3>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var SPHForce    = new NativeArray<float3>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var SPHSettings = new NativeArray<SPH>(SPHCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var UpdateSPHSettingsJob = new UpdateSPHSettingsJob
            {
                density = ui.density,
                mass = ui.mass,
                viscosity = ui.viscosity
            };
            var UpdateSPHSettingsJobHandle = UpdateSPHSettingsJob.Schedule(m_SPHQuery, inputDependencies);

            inputDependencies = UpdateSPHSettingsJobHandle;

            var initJob = new InitJob
            {
                positions = SPHPosition,
                velocities = SPHVelocity,
                sphs = SPHSettings,
                pressures = SPHPressure,
                densities = SPHDensity,
                forces = SPHForce
            };
            var InitJobHandle = initJob.Schedule(m_SPHQuery, inputDependencies);

            inputDependencies = InitJobHandle;

            var SPHDensityPressureJob = new SPHDensityPressureJob
            {
                densities = SPHDensity,
                pressures = SPHPressure,
                positions = SPHPosition,
                sphs = SPHSettings,
                count = SPHCount
            };
            var SPHDensityPressureJobHandle = SPHDensityPressureJob.Schedule(m_SPHQuery, inputDependencies);

            inputDependencies = SPHDensityPressureJobHandle;

            var SPHForceJob = new SPHForceJob
            {
                velocities = SPHVelocity,
                densities = SPHDensity,
                pressures = SPHPressure,
                positions = SPHPosition,
                forces = SPHForce,
                sphs = SPHSettings,
                count = SPHCount
            };
            var SPHForceJobHandle = SPHForceJob.Schedule(m_SPHQuery, inputDependencies);

            inputDependencies = SPHForceJobHandle;

            var SPHJob = new IntegrateJob
            {
                densities = SPHDensity,
                forces = SPHForce,
                dt = 0.00001f
            };
            var SPHJobHandle = SPHJob.Schedule(this, inputDependencies);

            inputDependencies = SPHJobHandle;

            return inputDependencies;
        }
    }
}