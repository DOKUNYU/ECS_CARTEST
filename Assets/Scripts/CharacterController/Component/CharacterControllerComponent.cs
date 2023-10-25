using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using static CharacterController.Util;
using UnityEngine;

namespace CharacterController
{
    //存放角色整体控制参数
    public struct CharacterController : IComponentData
    {
        //输入设置
        public float MovementSpeed;
        public float MaxMovementSpeed;
        public float RotationSpeed;
        public float JumpUpwardsSpeed;
        public float MaxSlope; // radians

        //物理设置
        public float3 Gravity;
        public int MaxIterations;
        public float CharacterMass;
        public float SkinWidth;
        public float ContactTolerance;
        public byte AffectsPhysicsBodies;
        
        //事件设置
        public byte RaiseCollisionEvents;
        public byte RaiseTriggerEvents;
    }
    
    /// <summary>
    ///存放pitch控制参数
    /// </summary>
    public struct PtzController : IComponentData
    {
        public Entity Entity;//存放pitch实体
        public float RotationSpeed;//速度
    }
    
    /// <summary>
    /// 存放用户输入参数
    /// </summary>
    public struct Input : IComponentData
    {
        public float2 Movement;
        public float2 Looking;
    }
    
    /// <summary>
    /// 详细的控制参数，给其他sys引用用
    /// </summary>
    //[WriteGroup(typeof(PhysicsGraphicalInterpolationBuffer))]
    [WriteGroup(typeof(PhysicsGraphicalSmoothing))]
    public struct CharacterControllerInternal : IComponentData
    {
        public float CurrentRotationAngle;
        public CharacterSupportState SupportedState;
        public float3 UnsupportedVelocity;
        public PhysicsVelocity Velocity;
        public Entity Entity;
        public bool IsJumping;
        public Input Input;
        
        public Entity Pitch;
        public PtzController PtzController;
        public LocalTransform PitchTransform;
        public float CurrentPitchRotationAngle;
    }

    /// <summary>
    /// 存放摄像机实体
    /// </summary>
    public struct CameraProxy : IComponentData
    {
    }
    
    /// <summary>
    /// 存放摄像机运动？
    /// </summary>
    public class MainCamera : IComponentData
    {
        public Transform Transform;
    }

    
}
