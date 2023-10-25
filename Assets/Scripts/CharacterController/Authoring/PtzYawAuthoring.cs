using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace CharacterController
{
    [Serializable]
    public class PtzYawAuthoring : MonoBehaviour
    {
        void OnEnable()
        {
        }
        class Baker : Baker<PtzYawAuthoring>
        {
            public override void Bake(PtzYawAuthoring authoring)
            {
                if (authoring.enabled)
                {
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    //仅标记用
                    AddComponent<PtzYaw>(entity);
                }
            }
        }

    }
}