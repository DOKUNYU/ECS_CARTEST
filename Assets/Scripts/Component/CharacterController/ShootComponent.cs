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
        //发射者
        public Entity Shooter;
        //初始化和撞击状态
        public bool IsInit;
        public bool IsCollision;
        //子弹位置方向
        public LocalTransform pos;
        public float3 dir;
    }

    public struct Harm : IComponentData
    {
        //伤害组件
        public float injury;
    }
}
