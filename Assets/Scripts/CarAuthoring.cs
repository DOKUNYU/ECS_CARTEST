using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;


namespace init
{
    public class CarAuthoring : MonoBehaviour
    {
        class Baker : Baker<CarAuthoring>
        {
            public override void Bake(CarAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Car>(entity);
                AddComponent<Input>(entity);
            }
        }
        
    }
    public struct Car : IComponentData
    {
    }
    
    public struct Input : IComponentData
    {
        public float2 Movement;
        public float2 Looking;
        public int Jumped;
    }

}

