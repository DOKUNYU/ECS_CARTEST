using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Stateful;
using UnityEngine;
using static Unity.Physics.PhysicsStep;
using Identity;

namespace CharacterController
{
    [Serializable]
    public class CharacterControllerAuthoring : MonoBehaviour
    {
        //输入设置
        // Speed of movement initiated by user input
        public float MovementSpeed = 2.5f;
        // Maximum speed of movement at any given time
        public float MaxMovementSpeed = 10.0f;
        // Speed of rotation initiated by user input
        public float RotationSpeed = 2.5f;
        // Speed of upwards jump initiated by user input
        public float JumpUpwardsSpeed = 5.0f;
        // Maximum slope angle character can overcome (in degrees)
        public float MaxSlope = 60.0f;
        
        //物理设置
        // Gravity force applied to the character controller body
        public float3 Gravity = Default.Gravity;
        // Maximum number of character controller solver iterations
        public int MaxIterations = 10;
        // Mass of the character (used for affecting other rigid bodies)
        public float CharacterMass = 1.0f;
        // Keep the character at this distance to planes (used for numerical stability)
        public float SkinWidth = 0.02f;
        // Anything in this distance to the character will be considered a potential contact
        // when checking support
        public float ContactTolerance = 0.1f;
        // Whether to affect other rigid bodies
        public bool AffectsPhysicsBodies = true;
        
        //事件设置
        // Note: collision events raised by character controller will always have details calculated
        public bool RaiseCollisionEvents = false;
        // Whether to raise trigger events
        public bool RaiseTriggerEvents = false;
        
        
        void OnEnable()
        {
        }

        class Baker : Baker<CharacterControllerAuthoring>
        {
            public override void Bake(CharacterControllerAuthoring authoring)
            {
                if (authoring.enabled)
                {
                    var componentData = new CharacterController
                    {
                        Gravity = authoring.Gravity,
                        MovementSpeed = authoring.MovementSpeed,
                        MaxMovementSpeed = authoring.MaxMovementSpeed,
                        RotationSpeed = authoring.RotationSpeed,
                        JumpUpwardsSpeed = authoring.JumpUpwardsSpeed,
                        MaxSlope = math.radians(authoring.MaxSlope),
                        MaxIterations = authoring.MaxIterations,
                        CharacterMass = authoring.CharacterMass,
                        SkinWidth = authoring.SkinWidth,
                        ContactTolerance = authoring.ContactTolerance,
                        AffectsPhysicsBodies = (byte)(authoring.AffectsPhysicsBodies ? 1 : 0),
                        RaiseCollisionEvents = (byte)(authoring.RaiseCollisionEvents ? 1 : 0),
                        RaiseTriggerEvents = (byte)(authoring.RaiseTriggerEvents ? 1 : 0)
                    };
                    var internalData = new CharacterControllerInternal
                    {
                        Entity = GetEntity(TransformUsageFlags.Dynamic),
                        Input = new Input(),
                        ShootInit = false
                    };
                    

                    //获取控制示例
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, componentData);
                    AddComponent(entity, internalData);
                    AddComponent<Car>(entity);

                    //碰撞事件
                    /*
                    if (authoring.RaiseCollisionEvents)
                    {
                        AddBuffer<StatefulCollisionEvent>(entity);
                    }

                    if (authoring.RaiseTriggerEvents)
                    {
                        AddBuffer<StatefulTriggerEvent>(entity);
                        AddComponent(entity, new StatefulTriggerEventExclude());
                    }  */
                    
                    

                }

            }
        }
    }
    
    
    
}