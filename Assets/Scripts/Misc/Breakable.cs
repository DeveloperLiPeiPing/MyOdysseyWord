using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
    [RequireComponent(typeof(Collider), typeof(AudioSource))]
    [AddComponentMenu("Misc/Breakable")]
    public class Breakable : MonoBehaviour
    {
        [Tooltip("物体正常状态下显示的模型")]
        public GameObject display;

        [Tooltip("物体被破坏时播放的音效")]
        public AudioClip clip;

        /// <summary>
        /// 当物体被破坏时触发的事件
        /// </summary>
        public UnityEvent OnBreak;

        protected Collider m_collider;      // 物体的碰撞体组件
        protected AudioSource m_audio;     // 音频源组件
        protected Rigidbody m_rigidBody;   // 刚体组件（可选）

        /// <summary>
        /// 获取物体当前是否已被破坏
        /// </summary>
        public bool broken { get; protected set; }

        /// <summary>
        /// 破坏物体的方法
        /// </summary>
        public virtual void Break()
        {
            if (!broken)
            {
                // 如果存在刚体，则设置为运动学状态
                if (m_rigidBody)
                {
                    m_rigidBody.isKinematic = true;
                }

                broken = true;
                display.SetActive(false);      // 隐藏显示模型
                m_collider.enabled = false;    // 禁用碰撞体
                m_audio.PlayOneShot(clip);     // 播放破坏音效
                OnBreak?.Invoke();             // 触发破坏事件
            }
        }

        protected virtual void Start()
        {
            // 获取必要的组件引用
            m_audio = GetComponent<AudioSource>();
            m_collider = GetComponent<Collider>();
            TryGetComponent(out m_rigidBody);  // 尝试获取刚体组件
        }
    }
}