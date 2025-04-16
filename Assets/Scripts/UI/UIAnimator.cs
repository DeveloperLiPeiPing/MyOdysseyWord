using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[RequireComponent(typeof(Animator))]
	[AddComponentMenu("UI/UI Animator")]
	public class UIAnimator : MonoBehaviour
    {
        /// <summary>
        /// 当播放显示动画时触发
        /// </summary>
        public UnityEvent OnShow;

        /// <summary>
        /// 当播放隐藏动画时触发
        /// </summary>
        public UnityEvent OnHide;

        [Header("动画参数设置")]
        public bool hidenOnAwake;          // 是否在初始化时隐藏
        public string normalTrigger = "Normal";  // 默认状态触发器名称
        public string showTrigger = "Show";     // 显示动画触发器名称
        public string hideTrigger = "Hide";     // 隐藏动画触发器名称

        protected Animator m_animator;     // 动画组件缓存

        /// <summary>
        /// 触发UI显示动画
        /// </summary>
        public virtual void Show()
        {
            m_animator.SetTrigger(showTrigger);
            OnShow?.Invoke();
        }

        /// <summary>
        /// 触发UI隐藏动画
        /// </summary>
        public virtual void Hide()
        {
            m_animator.SetTrigger(hideTrigger);
            OnHide?.Invoke();
        }

        /// <summary>
        /// 设置游戏对象激活状态
        /// </summary>
        /// <param name="value">是否激活</param>
        public virtual void SetActive(bool value) => gameObject.SetActive(value);

        protected virtual void Awake()
        {
            m_animator = GetComponent<Animator>();

            // 初始化时直接播放隐藏动画
            if (hidenOnAwake)
            {
                m_animator.Play(hideTrigger, 0, 1);
            }
        }
    }
}
