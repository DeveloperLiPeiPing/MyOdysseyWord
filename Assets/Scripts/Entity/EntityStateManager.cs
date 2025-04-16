using System;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey.Script
{
    /// <summary>
    /// 实体状态管理器基类
    /// </summary>
    public abstract class EntityStateManager : MonoBehaviour
    {
        public EntityStateManagerEvents events;  // 状态管理事件
    }

    /// <summary>
    /// 泛型实体状态管理器
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
    {
        protected List<EntityState<T>> m_list = new List<EntityState<T>>();  // 状态列表
        protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();  // 状态类型字典

        /// <summary>
        /// 当前状态实例
        /// </summary>
        public EntityState<T> current { get; protected set; }

        /// <summary>
        /// 上一个状态实例
        /// </summary>
        public EntityState<T> last { get; protected set; }

        /// <summary>
        /// 当前状态索引
        /// </summary>
        public int index => m_list.IndexOf(current);

        /// <summary>
        /// 上一个状态索引
        /// </summary>
        public int lastIndex => m_list.IndexOf(last);

        /// <summary>
        /// 关联的实体实例
        /// </summary>
        public T entity { get; protected set; }

        /// <summary>
        /// 获取状态列表（需子类实现）
        /// </summary>
        protected abstract List<EntityState<T>> GetStateList();

        /// <summary>
        /// 初始化关联实体
        /// </summary>
        protected virtual void InitializeEntity() => entity = GetComponent<T>();

        /// <summary>
        /// 初始化所有状态
        /// </summary>
        protected virtual void InitializeStates()
        {
            m_list = GetStateList();

            foreach (var state in m_list)
            {
                var type = state.GetType();

                if (!m_states.ContainsKey(type))
                {
                    m_states.Add(type, state);
                }
            }

            if (m_list.Count > 0)
            {
                current = m_list[0];  // 默认第一个状态
            }
        }

        /// <summary>
        /// 根据索引切换状态
        /// </summary>
        /// <param name="to">目标状态索引</param>
        public virtual void Change(int to)
        {
            if (to >= 0 && to < m_list.Count)
            {
                Change(m_list[to]);
            }
        }

        /// <summary>
        /// 根据类型切换状态
        /// </summary>
        /// <typeparam name="TState">目标状态类型</typeparam>
        public virtual void Change<TState>() where TState : EntityState<T>
        {
            var type = typeof(TState);

            if (m_states.ContainsKey(type))
            {
                Change(m_states[type]);
            }
        }

        /// <summary>
        /// 切换状态核心方法
        /// </summary>
        /// <param name="to">目标状态实例</param>
        public virtual void Change(EntityState<T> to)
        {
            if (to != null && Time.timeScale > 0)
            {
                if (current != null)
                {
                    current.Exit(entity);  // 退出当前状态
                    events.onExit.Invoke(current.GetType());
                    last = current;  // 记录上一个状态
                }

                current = to;  // 设置新状态
                current.Enter(entity);  // 进入新状态
                events.onEnter.Invoke(current.GetType());
                events.onChange?.Invoke();  // 触发状态改变事件
            }
        }

        /// <summary>
        /// 检查当前状态是否匹配指定类型
        /// </summary>
        /// <param name="type">要比较的类型</param>
        public virtual bool IsCurrentOfType(Type type)
        {
            if (current == null)
            {
                return false;
            }

            return current.GetType() == type;
        }

        /// <summary>
        /// 检查是否包含指定类型的状态
        /// </summary>
        /// <param name="type">要查找的状态类型</param>
        public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);

        /// <summary>
        /// 每帧更新当前状态
        /// </summary>
        public virtual void Step()
        {
            if (current != null && Time.timeScale > 0)
            {
                current.Step(entity);
            }
        }

        /// <summary>
        /// 处理碰撞接触事件
        /// </summary>
        /// <param name="other">碰撞的碰撞体</param>
        public virtual void OnContact(Collider other)
        {
            if (current != null && Time.timeScale > 0)
            {
                current.OnContact(entity, other);
            }
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        protected virtual void Start()
        {
            InitializeEntity();
            InitializeStates();
        }
    }
}