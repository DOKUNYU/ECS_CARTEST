using Unity.Entities;

namespace Event
{
    //状态------------------------------------------------------------
    
    /// <summary>
    /// 脱战状态
    /// </summary>
    public struct OutOfFightStatus : IComponentData {}
    
    /// <summary>
    /// 回血状态
    /// </summary>
    public struct ReviveStatus : IComponentData {}
    
    //死亡类型----------------------------------------------------------
    
    /// <summary>
    /// 死亡类型
    /// <para name="overheat">超热量</para>
    /// <para name="kill">击杀</para>
    /// </summary>
    public enum DeadType
    {
        overheat,
        kill,
        abnormalOffline,
        yellowCard,
        redCard
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public struct Dead : IComponentData
    {
        public DeadType deadType;
    }
    
    //游戏进程---------------------------------------------------------
    /// <summary>
    /// 游戏进程
    /// <para name="prepare">准备</para>
    /// <para name="Start">开始</para>
    /// <para name="stop">暂停</para>
    ///  <para name="end">结束</para>
    /// </summary>
    public enum GameSratusType
    {
        prepare,
        start,
        stop,
        end
    }
    
}