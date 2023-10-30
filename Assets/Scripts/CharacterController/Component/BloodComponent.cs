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
    /// <summary>
    /// 血量组件
    /// </summary>
    public struct BloodComponent : IComponentData
    {
        public float maxHp;
        public float hp;

    }
    
}
