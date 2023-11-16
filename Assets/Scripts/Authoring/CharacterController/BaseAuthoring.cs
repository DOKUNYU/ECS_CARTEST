using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace CharacterController
{
    public class BaseAuthoring : MonoBehaviour
    {
        public bool isRed;
        class Baker : Baker<BaseAuthoring>
        {
            public override void Bake(BaseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.isRed)
                {
                    AddComponent(entity, new BaseRed());
                }
                else
                {
                    AddComponent(entity, new BaseBlue());
                }
        
            }
        }
    }   

}
