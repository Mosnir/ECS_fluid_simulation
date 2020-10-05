using System;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace SPH.Spawners
{
    [Serializable]
    public struct SPH : IComponentData
    {
        public float radius;
        public float smoothingRadius;
        public float smoothingRadiusSquare;
        public float mass;
        public float density;
        public float viscosity;
    }
}