using Unity.Entities;

namespace Event
{
    //判罚-----------------------------------------------
    
    /// <summary>
    /// 黄牌判罚类型
    /// <para name="Bumping">故意冲撞</para>
    /// <para name="RestrictedArea">进入禁区</para>
    /// <para name="Disturbance">干扰复活</para> 
    /// </summary>
    public enum YellowCardEventType
    {
        Bumping,
        RestrictedArea,
        Disturbance,
    }
    
    /// <summary>
    /// 红牌判罚类型
    /// <para name="Ejected">红牌</para>
    /// <para name="CampAwardNegative">阵营判负</para>
    /// <para name="StopGame">终止比赛</para> 
    /// </summary>
    public enum RedCardEventType
    {
        Ejected,
        CampAwardNegative,
        StopGame,
    }
    
    /// <summary>
    /// 黄牌事件
    /// </summary>
    public struct YellowCardEvent : IComponentData
    {
        public YellowCardEventType EventType;
    }
    
    /// <summary>
    /// 红牌事件
    /// </summary>
    public struct RedCardEvent : IComponentData
    {
        public RedCardEventType EventType;
    }
    
}