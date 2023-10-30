using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Stateful;
using UnityEngine;
using static Unity.Physics.PhysicsStep;

namespace CharacterController
{
    [Serializable]
    public class BloodAuthoring : MonoBehaviour
    {
        public float maxHp;
        
        
        void OnEnable()
        {
        }

        class Baker : Baker<BloodAuthoring>
        {
            public override void Bake(BloodAuthoring authoring)
            {
                if (authoring.enabled)
                {
                    var bloodData = new BloodComponent()
                    {
                        maxHp = authoring.maxHp,
                        hp = authoring.maxHp
                    };


                    //获取控制示例
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, bloodData);

                }

            }
        }
    }
    
    
    
}