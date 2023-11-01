using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CharacterController
{
    [Serializable]
    public class HitAuthoring : MonoBehaviour
    {
        public float Damage = 10f;

        class Baker : Baker<HitAuthoring>
        {
            public override void Bake(HitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //todo:根据击打到的对象加伤害
                AddComponent(entity, new Harm()
                {
                    injury = authoring.Damage
                });
            }
        }
    }

    public struct Harm : IComponentData
    {
        public float injury;
        
        //todo:碰撞后反弹（要加么
    }
    
    
    
}
