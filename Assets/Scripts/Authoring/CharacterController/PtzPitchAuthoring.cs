using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;
using Unity.Physics;
using Identity;


namespace CharacterController
{
    [Serializable]
    public class PtzPitchAuthoring : MonoBehaviour
    {
        // Speed of rotation initiated by user input
        public float RotationSpeed = 1f;

        void OnEnable()
        {
        }

        class Baker : Baker<PtzPitchAuthoring>
        {
            public override void Bake(PtzPitchAuthoring authoring)
            {
                if (authoring.enabled)
                {
                    var ptzData = new PtzController
                    {
                        RotationSpeed = authoring.RotationSpeed
                    };
                    
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, ptzData);
                    AddComponent<PtzPitch>(entity);
                }
            }
        }

    }
}

