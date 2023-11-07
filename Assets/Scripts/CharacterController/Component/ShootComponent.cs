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
    [Serializable]
    public struct ShootSetting: IComponentData
    {
        //子弹预制体
        public Entity BulletPrefab;
    }

    public struct Bullet : IComponentData
    {
        public Entity Shooter;
        public Entity Reciever;
        public bool IsInit;
        public LocalTransform pos;
        public float3 dir;
    }
}
