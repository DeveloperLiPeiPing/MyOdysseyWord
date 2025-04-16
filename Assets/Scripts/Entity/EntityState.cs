using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Odyssey.Script
{
    /// <summary>
    /// 实体状态基类（泛型）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class EntityState<T> where T : Entity<T>
    {
        // 状态事件
        public UnityEvent onEnter;  // 进入状态时触发
        public UnityEvent onExit;    // 退出状态时触发

        // 状态持续时间（秒）
        public float timeSinceEntered { get; protected set; }

        /// <summary>
        /// 进入状态
        /// </summary>
        public void Enter(T entity)
        {
            timeSinceEntered = 0;
            onEnter?.Invoke();
            OnEnter(entity);
        }

        /// <summary>
        /// 退出状态
        /// </summary>
        public void Exit(T entity)
        {
            onExit?.Invoke();
            OnExit(entity);
        }

        /// <summary>
        /// 每帧更新状态
        /// </summary>
        public void Step(T entity)
        {
            OnStep(entity);
            timeSinceEntered += Time.deltaTime;
        }

        /// <summary>
        /// 状态进入时调用（需子类实现）
        /// </summary>
        protected abstract void OnEnter(T entity);

        /// <summary>
        /// 状态退出时调用（需子类实现）
        /// </summary>
        protected abstract void OnExit(T entity);

        /// <summary>
        /// 状态每帧更新逻辑（需子类实现）
        /// </summary>
        protected abstract void OnStep(T entity);

        /// <summary>
        /// 实体与碰撞体接触时调用（需子类实现）
        /// </summary>
        public abstract void OnContact(T entity, Collider other);

        /// <summary>
        /// 根据类型名称创建状态实例
        /// </summary>
        /// <param name="typeName">状态类全名</param>
        /// <returns>新建的状态实例</returns>
        public static EntityState<T> CreateFromString(string typeName)
        {
            return (EntityState<T>)System.Activator
                .CreateInstance(System.Type.GetType(typeName));
        }

        /// <summary>
        /// 根据类型名称数组批量创建状态实例列表
        /// </summary>
        /// <param name="array">状态类全名数组</param>
        /// <returns>新建的状态实例列表</returns>
        public static List<EntityState<T>> CreateListFromStringArray(string[] array)
        {
            var list = new List<EntityState<T>>();

            foreach (var typeName in array)
            {
                list.Add(CreateFromString(typeName));
            }

            return list;
        }
    }
}