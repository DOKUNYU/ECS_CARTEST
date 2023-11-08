using Unity.Entities;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 
    /// </summary>
    public struct DisplayUITag : IComponentData
    {
    }
    
    /// <summary>
    /// 存放基地UI组件
    /// </summary>
    public class BaseBloodUI : IComponentData
    {
        public Image BaseBlood;
    }
}