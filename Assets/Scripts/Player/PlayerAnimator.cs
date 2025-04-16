using System.Collections.Generic;
using UnityEngine;

namespace Odyssey.Script
{
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("Player/PlayerAnimator")]
    public class PlayerAnimator : MonoBehaviour
    {
        [System.Serializable]
        public class ForcedTransition
        {
    
            public int fromStateId;


            public int animationLayer;

    
            public string toAnimationState;
        }


        public Animator animator;

        public string stateName = "State";                                      // 当前状态参数名
        public string lastStateName = "Last State";                         // 上一状态参数名
        public string lateralSpeedName = "Lateral Speed";            // 水平速度参数名
        public string verticalSpeedName = "Vertical Speed";         // 垂直速度参数名
        public string lateralAnimationSpeedName = "Lateral Animation Speed"; // 水平动画速度参数名
        public string healthName = "Health";                    // 生命值参数名
        public string jumpCounterName = "Jump Counter";         // 跳跃计数参数名
        public string isGroundedName = "Is Grounded";            // 是否着地参数名
        public string isHoldingName = "Is Holding";             // 是否持有物品参数名
        public string onStateChangedName = "On State Changed";   // 状态变化触发器名


        public float minLateralAnimationSpeed = 0.5f;           // 最小水平动画速度
        public List<ForcedTransition> forcedTransitions;         // 强制过渡列表

        // 动画参数哈希值缓存
        protected int m_stateHash;
        protected int m_lastStateHash;
        protected int m_lateralSpeedHash;
        protected int m_verticalSpeedHash;
        protected int m_lateralAnimationSpeedHash;
        protected int m_healthHash;
        protected int m_jumpCounterHash;
        protected int m_isGroundedHash;
        protected int m_isHoldingHash;
        protected int m_onStateChangedHash;

        protected Dictionary<int, ForcedTransition> m_forcedTransitions; // 强制过渡字典
        protected Player m_player;                                // 玩家组件引用

        #region 初始化方法
        /// <summary>
        /// 初始化玩家组件引用
        /// </summary>
        protected virtual void InitializePlayer()
        {
            m_player = GetComponent<Player>();
            // 添加状态变化监听器
            m_player.states.events.onChange.AddListener(HandleForcedTransitions);
        }

        /// <summary>
        /// 初始化强制过渡字典
        /// </summary>
        protected virtual void InitializeForcedTransitions()
        {
            m_forcedTransitions = new Dictionary<int, ForcedTransition>();

            foreach (var transition in forcedTransitions)
            {
                if (!m_forcedTransitions.ContainsKey(transition.fromStateId))
                {
                    m_forcedTransitions.Add(transition.fromStateId, transition);
                }
            }
        }

        /// <summary>
        /// 初始化动画触发器
        /// </summary>
        protected virtual void InitializeAnimatorTriggers()
        {
            m_player.states.events.onChange.AddListener(() =>
                animator.SetTrigger(m_onStateChangedHash));
        }

        /// <summary>
        /// 初始化动画参数哈希值
        /// </summary>
        protected virtual void InitializeParametersHash()
        {
            m_stateHash = Animator.StringToHash(stateName);
            m_lastStateHash = Animator.StringToHash(lastStateName);
            m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
            m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
            m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
            m_healthHash = Animator.StringToHash(healthName);
            m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
            m_isGroundedHash = Animator.StringToHash(isGroundedName);
            m_isHoldingHash = Animator.StringToHash(isHoldingName);
            m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
        }
        #endregion

        #region 动画控制方法
        /// <summary>
        /// 处理强制状态过渡
        /// </summary>
        protected virtual void HandleForcedTransitions()
        {
            var lastStateIndex = m_player.states.lastIndex;

            // 检查是否需要强制过渡
            if (m_forcedTransitions.ContainsKey(lastStateIndex))
            {
                var transition = m_forcedTransitions[lastStateIndex];
                animator.Play(transition.toAnimationState, transition.animationLayer);
            }
        }

        /// <summary>
        /// 更新动画参数
        /// </summary>
        protected virtual void HandleAnimatorParameters()
        {
            // 计算速度参数
            var lateralSpeed = m_player.lateralVelocity.magnitude;
            var verticalSpeed = m_player.verticalVelocity.y;
            var lateralAnimationSpeed = Mathf.Max(
                minLateralAnimationSpeed,
                lateralSpeed / m_player.stats.current.topSpeed);

            // 设置动画参数
            animator.SetInteger(m_stateHash, m_player.states.index);
            animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
            animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
            animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
            animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
            animator.SetInteger(m_healthHash, m_player.health.current);
            animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
            animator.SetBool(m_isGroundedHash, m_player.isGrounded);
            animator.SetBool(m_isHoldingHash, m_player.holding);
        }
        #endregion

        #region Unity生命周期
        protected virtual void Start()
        {
            InitializePlayer();
            InitializeForcedTransitions();
            InitializeParametersHash();
            InitializeAnimatorTriggers();
        }

        /// <summary>
        /// 每帧更新动画参数
        /// </summary>
        protected virtual void LateUpdate() => HandleAnimatorParameters();
        #endregion
    }
}