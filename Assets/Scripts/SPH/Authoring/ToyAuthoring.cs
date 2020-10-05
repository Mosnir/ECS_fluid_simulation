using Unity.Entities;
using UnityEngine;

namespace SPH.Spawners.Authoring
{

    [RequiresEntityConversion]
    public class ToyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        // Lets you convert the editor data representation to the entity optimal runtime representation
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Toy {};
            dstManager.AddComponentData(entity, data);
        }
    }

}