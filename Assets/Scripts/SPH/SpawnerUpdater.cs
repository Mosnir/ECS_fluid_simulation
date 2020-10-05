using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace SPH.Spawners
{
    // NOTE: Updating a manually-created system in FixedUpdate() as demonstrated below
    // is intended as a short-term workaround; the entire `SimulationSystemGroup` will
    // eventually use a fixed timestep by default.
    public class SpawnerUpdater : MonoBehaviour
    {
        private SpawnerSystem spawnerSystem;

        private void FixedUpdate()
        {
            if (spawnerSystem == null)
            {
                spawnerSystem = World.Active.GetOrCreateSystem<SpawnerSystem>();
            }
            spawnerSystem.go = gameObject;
            spawnerSystem.Update();
        }
    }
}