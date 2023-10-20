using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Havok;

namespace Init
{
    public class SpwanerAuthoring : MonoBehaviour
    {
        public GameObject prefabs;
        public Transform Trans;

        public class Baker : Baker<SpwanerAuthoring>
        {
            public override void Bake(SpwanerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //transform
                /*AddComponent(entity, new LocalTransform
                {
                    Position = new float3(0,0,0),
                    Rotation =  new quaternion(0,0,0,0),
                    Scale = 1
                });*/
                
                //spwan
                AddComponent(entity,new SpwanSetting
                {
                    Prefab = GetEntity(authoring.prefabs, TransformUsageFlags.Dynamic),
                    Position = new float3(authoring.Trans.position.x,authoring.Trans.position.y,authoring.Trans.position.z),
                    Rotation = new quaternion(authoring.Trans.rotation.x,authoring.Trans.rotation.y,authoring.Trans.rotation.z,authoring.Trans.rotation.w)
                });
                Debug.Log("bake");
                
            }
        }
    }
    public struct SpwanSetting : IComponentData
    {
        public Entity Prefab { get; set; }
        public float3 Position { get; set; }
        public quaternion Rotation { get; set; }
    }
}

