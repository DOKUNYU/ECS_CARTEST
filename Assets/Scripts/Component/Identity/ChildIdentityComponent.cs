using Unity.Entities;

namespace Identity
{
    /// <summary>
    /// 机器人装甲板
    /// </summary>
    public struct RoboArmor: IComponentData {}
    
    /// <summary>
    /// 基地装甲板
    /// </summary>
    public struct BaseArmor: IComponentData {}
    
    /// <summary>
    /// 机器人灯光
    /// </summary>
    public struct RoboLight : IComponentData{}
    
    /// <summary>
    /// 地面灯光
    /// </summary>
    public struct GroundLight : IComponentData{}
    
    /// <summary>
    /// 指示灯灯光
    /// </summary>
    public struct IndicatorLight : IComponentData{}
    
    /// <summary>
    /// 小陀螺指示灯灯光
    /// </summary>
    public struct SpinIndicator : IComponentData{}
    
    /// <summary>
    /// 枪口锁定指示灯灯光
    /// </summary>
    public struct GunLockedIndicator : IComponentData{}

    /// <summary>
    /// 无敌指示灯灯光
    /// </summary>
    public struct BulletproofIndicator : IComponentData{}
    
    /// <summary>
    /// 弹舱指示灯光
    /// </summary>
    public struct MuzzleLightBar : IComponentData{}
    
    /// <summary>
    /// 超级电容指示灯光
    /// </summary>
    public struct SuperBatteryIndicator : IComponentData{}
    
    /// <summary>
    /// 弹舱
    /// </summary>
    public struct Magazine : IComponentData{}
    
    
    
    /// <summary>
    /// 云台Yaw轴标记组件
    /// </summary>
    public struct PtzYaw : IComponentData
    {
    }
    
    /// <summary>
    /// 云台Pitch轴标记组件
    /// </summary> 
    public struct PtzPitch : IComponentData
    {
    }
    
    
    
    
}

