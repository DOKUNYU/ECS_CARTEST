using Unity.Entities;

namespace CharacterController
{
    /// <summary>
    /// 英雄功率优先底盘
    /// </summary>
    public struct HeroPowerPriorityChassis : IComponentData
    {
        public int MaxBlood;
        public int MaxPower;
    }
    
    /// <summary>
    /// 英雄血量优先底盘
    /// </summary>
    public struct HeroBloodPriorityChassis : IComponentData
    {
        public int MaxBlood;
        public int MaxPower;
    }
    
    /// <summary>
    /// 步兵功率优先底盘
    /// </summary>
    public struct InfantryPowerPriorityChassis : IComponentData
    {
        public int MaxBlood;
        public int MaxPower;
    }
    
    /// <summary>
    /// 步兵血量优先底盘
    /// </summary>
    public struct InfantryBloodPriorityChassis : IComponentData
    {
        public int MaxBlood;
        public int MaxPower;
    }
    
    /// <summary>
    /// 步兵爆发优先发射机构
    /// </summary>
    public struct InfantryBrustPriorityGun : IComponentData
    {
        public int Maxheat;
        public int MaxCooling;
    }
    
    /// <summary>
    /// 步兵冷却优先发射机构
    /// </summary>
    public struct InfantryCoolingPriorityGun : IComponentData
    {
        public int Maxheat;
        public int MaxCooling;
    }
    
    /// <summary>
    /// 英雄默认发射机构
    /// </summary>
    public struct HeroDefaultGun : IComponentData
    {
        public int Maxheat;
        public int MaxCooling;
    }
}