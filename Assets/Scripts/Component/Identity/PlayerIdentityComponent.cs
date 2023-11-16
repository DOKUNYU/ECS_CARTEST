using Unity.Entities;

namespace Identity
{
    /// <summary>
    /// 游戏开始前身份-房主
    /// </summary>
    public struct RoomOwner: IComponentData{}
    
    /// <summary>
    /// 游戏开始前身份-普通玩家
    /// </summary>
    public struct RoomPlayer: IComponentData{}
    
    /// <summary>
    /// 阵营组件 红
    /// </summary>
    public struct CampRed: IComponentData{}
    
    /// <summary>
    /// 阵营组件 蓝
    /// </summary>
    public struct CampBlue: IComponentData{}
    
    /// <summary>
    /// 英雄
    /// </summary>
    public struct Hero: IComponentData{}

    /// <summary>
    /// 步兵
    /// </summary>
    public struct Infantry: IComponentData
    {
        public int serial;
    }
    
    /// <summary>
    /// 哨兵
    /// </summary>
    public struct Sentinel: IComponentData {}
    
    /// <summary>
    /// 观众
    /// </summary>
    public struct Spectator: IComponentData {}
    
    /// <summary>
    /// 裁判
    /// </summary>
    public struct Judge : IComponentData{}
    
    /// <summary>
    /// 车辆标记组件
    /// </summary>
    public struct Car : IComponentData
    {
    }
    
}
