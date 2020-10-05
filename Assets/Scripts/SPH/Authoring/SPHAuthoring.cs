using Unity.Entities;
using UnityEngine;

namespace SPH.Spawners.Authoring
{

    [RequiresEntityConversion]
    public class SPHAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float radius;
        public float smoothingRadius;
        public float smoothingRadiusSquare;
        public float mass;
        public float density;
        public float viscosity;

        // Lets you convert the editor data representation to the entity optimal runtime representation
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new SPH
            {
                radius = radius,
                smoothingRadius = smoothingRadius,
                smoothingRadiusSquare = smoothingRadiusSquare,
                mass = mass,
                density = density,
                viscosity = viscosity
            };
            dstManager.AddComponentData(entity, data);
        }
    }

}