using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
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

    public struct Harm : IComponentData
    {
        //伤害组件
        public float injury;
    }
}
