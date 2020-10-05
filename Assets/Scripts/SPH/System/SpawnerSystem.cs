using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;

namespace SPH.Spawners
{
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
    }

    [DisableAutoCreation]
    public class SpawnerSystem : ComponentSystem
    {

        public GameObject go;


        protected override void OnUpdate()
        {

            if (!Input.GetKey(KeyCode.Space)) return;

            var position = new float3(go.transform.position);
            var rotation = new quaternion(
                go.transform.rotation.x,
                go.transform.rotation.y,
                go.transform.rotation.z,
                go.transform.rotation.w
            );

            Entities.ForEach((Entity spawnerEntity, ref Spawner spawnerData, ref Translation translation) =>
            {
                var newEntity = PostUpdateCommands.Instantiate(spawnerData.Prefab);
                PostUpdateCommands.SetComponent(newEntity, new Translation { Value = go.transform.position });
                PostUpdateCommands.SetComponent(newEntity, new PhysicsVelocity { Linear = go.transform.forward * 10.0f });
            });

        }
    }

}