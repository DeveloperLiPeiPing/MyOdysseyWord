using System;
using UnityEngine.Events;

namespace Odyssey.Script
{
    /// <summary>
    /// 实体状态管理器事件类
    /// 用于处理状态切换相关事件
    /// </summary>
    [Serializable] 
    public class EntityStateManagerEvents
    {
        /// <summary>
        /// 当状态发生改变时触发的事件
        /// </summary>
        public UnityEvent onChange;

        /// <summary>
        /// 当进入某个状态时触发的事件
        /// </summary>
        public UnityEvent<Type> onEnter;

        /// <summary>
        /// 当退出某个状态时触发的事件
        /// </summary>
        public UnityEvent<Type> onExit;
    }
}