using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using Collider = Unity.Physics.Collider;
using Random = Unity.Mathematics.Random;

namespace SPH.Spawners
{
    // NOTE: Updating a manually-created system in FixedUpdate() as demonstrated below
    // is intended as a short-term workaround; the entire `SimulationSystemGroup` will
    // eventually use a fixed timestep by default.
    public class SpawnRandomBalls : MonoBehaviour
    {

        public GameObject prefab;
        public float3 range;
        public int count;

        void OnEnable()
        {
            if (this.enabled)
            {
                // Create entity prefab from the game object hierarchy once
                Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, BasePhysicsDemo.DefaultWorld);
                var entityManager = BasePhysicsDemo.DefaultWorld.EntityManager;

                var positions = new NativeArray<float3>(count, Allocator.Temp);
                RandomPointsOnCircle(transform.position, range, ref positions);

                for (int i = 0; i < count; i++)
                {
                    var instance = entityManager.Instantiate(sourceEntity);
                    entityManager.SetComponentData(instance, new Translation { Value = positions[i] });
                }

                positions.Dispose();

            }
        }

        public static void RandomPointsOnCircle(float3 center, float3 range, ref NativeArray<float3> positions)
        {
            var count = positions.Length;
            // initialize the seed of the random number generator 
            Unity.Mathematics.Random random = new Unity.Mathematics.Random();
            random.InitState(10);
            for (int i = 0; i < count; i++)
            {
                positions[i] = center + random.NextFloat3(-range, range);
            }
        }

    }
}