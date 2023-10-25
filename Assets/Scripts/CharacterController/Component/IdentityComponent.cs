using Unity.Entities;

namespace CharacterController
{
    /// <summary>
    /// 车辆标记组件
    /// </summary>
    public struct Car : IComponentData
    {
    }
    
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
