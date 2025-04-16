using System;
using UnityEngine.Events;

namespace Odyssey.Script
{
    [Serializable]
    public class PlayerEvents
    {
        /// <summary>
        /// 当玩家跳跃时触发
        /// </summary>
        public UnityEvent OnJump;

        /// <summary>
        /// 当玩家受到伤害时触发
        /// </summary>
        public UnityEvent OnHurt;

        /// <summary>
        /// 当玩家死亡时触发
        /// </summary>
        public UnityEvent OnDie;

        /// <summary>
        /// 当玩家使用旋转攻击时触发
        /// </summary>
        public UnityEvent OnSpin;

        /// <summary>
        /// 当玩家拾取物品时触发
        /// </summary>
        public UnityEvent OnPickUp;

        /// <summary>
        /// 当玩家投掷物品时触发
        /// </summary>
        public UnityEvent OnThrow;

        /// <summary>
        /// 当玩家开始下坠攻击时触发
        /// </summary>
        public UnityEvent OnStompStarted;

        /// <summary>
        /// 当玩家下坠攻击过程中下落时触发
        /// </summary>
        public UnityEvent OnStompFalling;

        /// <summary>
        /// 当玩家下坠攻击落地时触发
        /// </summary>
        public UnityEvent OnStompLanding;

        /// <summary>
        /// 当玩家结束下坠攻击时触发
        /// </summary>
        public UnityEvent OnStompEnding;

        /// <summary>
        /// 当玩家抓住边缘时触发
        /// </summary>
        public UnityEvent OnLedgeGrabbed;

        /// <summary>
        /// 当玩家攀爬边缘时触发
        /// </summary>
        public UnityEvent OnLedgeClimbing;

        /// <summary>
        /// 当玩家空中俯冲时触发
        /// </summary>
        public UnityEvent OnAirDive;

        /// <summary>
        /// 当玩家后空翻时触发
        /// </summary>
        public UnityEvent OnBackflip;

        /// <summary>
        /// 当玩家开始滑翔时触发
        /// </summary>
        public UnityEvent OnGlidingStart;

        /// <summary>
        /// 当玩家停止滑翔时触发
        /// </summary>
        public UnityEvent OnGlidingStop;

        /// <summary>
        /// 当玩家开始冲刺时触发
        /// </summary>
        public UnityEvent OnDashStarted;

        /// <summary>
        /// 当玩家结束冲刺时触发
        /// </summary>
        public UnityEvent OnDashEnded;
    }
}