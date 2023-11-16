using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Identity
{
    /// <summary>
    /// 红基地组件(单例)
    /// </summary> 
    public struct BaseRed : IComponentData
    {
    }
    
    /// <summary>
    /// 蓝基地组件(单例)
    /// </summary> 
    public struct BaseBlue : IComponentData
    {
    }
    
    /// <summary>
    /// 子弹
    /// </summary> 
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
    
    /// <summary>
    /// 中央控制区
    /// </summary> 
    public struct Central : IComponentData
    {
    }
    
    /// <summary>
    /// 蓝兑换站（补给点）
    /// </summary> 
    public struct DepotBlue : IComponentData
    {
    }
    
    /// <summary>
    /// 红兑换站（补给点）
    /// </summary> 
    public struct DepotRed : IComponentData
    {
    }
    
    /// <summary>
    /// 蓝方禁区
    /// </summary> 
    public struct PenaltyAreaBlue : IComponentData
    {
    }
    
    /// <summary>
    /// 红方禁区
    /// </summary> 
    public struct PenaltyAreaRed : IComponentData
    {
    }
}