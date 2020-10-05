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
    public class ToySystem : JobComponentSystem
    {
        EntityQuery m_ToyQuery;

        public static GameObject toy;

        [BurstCompile]
        [RequireComponentTag(typeof(Toy), typeof(PhysicsCollider))]
        struct UpdateToyPositionJob : IJobForEachWithEntity<Toy,  PhysicsCollider>
        {

            [ReadOnly] public float3 position;
            [ReadOnly] public float radius;

            public unsafe void Execute(Entity entity, int index, [ReadOnly]ref Toy toy, ref PhysicsCollider physicsCollider)
            {

                if (physicsCollider.ColliderPtr->Type != ColliderType.Sphere)
                {
                    return;
                }

                var scPtr = (Unity.Physics.SphereCollider*)physicsCollider.ColliderPtr;
                var sphereGeometry = scPtr->Geometry;
                sphereGeometry.Center = position;
                sphereGeometry.Radius = radius;
                scPtr->Geometry = sphereGeometry;
            }
        }

        UI ui;

        protected override void OnCreate()
        {

            ui = GameObject.FindObjectOfType<UI>();

            m_ToyQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Toy>(),
                    ComponentType.ReadWrite<PhysicsCollider>(),
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            if (toy == null) return inputDependencies;

            var UpdateToyPositionJob = new UpdateToyPositionJob
            {
                position = toy.transform.position,
                radius = ui.radius
            };
            var UpdateToyPositionJobHandle = UpdateToyPositionJob.Schedule(m_ToyQuery, inputDependencies);

            inputDependencies = UpdateToyPositionJobHandle;

            return inputDependencies;
        }
    }
}