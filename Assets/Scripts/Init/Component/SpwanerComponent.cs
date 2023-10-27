using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using static CharacterController.Util;
using UnityEngine;

namespace Init
{
    public struct SpwanSetting : IComponentData
    {
        public Entity Prefab { get; set; }
        public float3 Position { get; set; }
        public quaternion Rotation { get; set; }
    }
}
