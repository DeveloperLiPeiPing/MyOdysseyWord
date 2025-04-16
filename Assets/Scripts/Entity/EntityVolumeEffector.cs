using UnityEngine;

namespace Odyssey.Script
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Entity/Entity Volume Effector")]
    public class EntityVolumeEffector : MonoBehaviour
    {
        [Tooltip("速度转换系数，用于调整实体进入区域时的初始速度")]
        public float velocityConversion = 1f;
        [Tooltip("加速度乘数，影响实体的加速能力")]
        public float accelerationMultiplier = 1f;
        [Tooltip("最大速度乘数，影响实体能达到的最高速度")]
        public float topSpeedMultiplier = 1f;
        [Tooltip("减速度乘数，影响实体的减速能力")]
        public float decelerationMultiplier = 1f;
        [Tooltip("转向阻力乘数，影响实体改变方向的难易程度")]
        public float turningDragMultiplier = 1f;
        [Tooltip("重力乘数，影响实体受到的重力大小")]
        public float gravityMultiplier = 1f;

        // 缓存Collider组件引用
        protected Collider m_collider;

        /// <summary>
        /// 初始化方法，在游戏开始时调用
        /// </summary>
        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }

        /// <summary>
        /// 当其他碰撞体进入触发器时调用
        /// </summary>
        /// <param name="other">进入触发器的碰撞体</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Entity entity))
            {
                // 应用所有物理参数修改
                entity.velocity *= velocityConversion;
                entity.accelerationMultiplier = accelerationMultiplier;
                entity.topSpeedMultiplier = topSpeedMultiplier;
                entity.decelerationMultiplier = decelerationMultiplier;
                entity.turningDragMultiplier = turningDragMultiplier;
                entity.gravityMultiplier = gravityMultiplier;
            }
        }

        /// <summary>
        /// 当其他碰撞体离开触发器时调用
        /// </summary>
        /// <param name="other">离开触发器的碰撞体</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Entity entity))
            {
                // 重置所有物理参数为默认值1
                entity.accelerationMultiplier = 1f;
                entity.topSpeedMultiplier = 1f;
                entity.decelerationMultiplier = 1f;
                entity.turningDragMultiplier = 1f;
                entity.gravityMultiplier = 1f;
            }
        }
    }
}