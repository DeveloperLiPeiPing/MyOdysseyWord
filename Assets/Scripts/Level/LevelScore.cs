using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
    [AddComponentMenu("Level/LevelScore")]
    public class LevelScore : Singleton<LevelScore>
    {
        /// <summary>
        /// 当收集金币数量变化时触发
        /// </summary>
        public UnityEvent<int> OnCoinsSet;

        /// <summary>
        /// 当星星收集状态变化时触发
        /// </summary>
        public UnityEvent<bool[]> OnStarsSet;

        /// <summary>
        /// 当关卡分数数据加载完成时触发
        /// </summary>
        public UnityEvent OnScoreLoaded;

        /// <summary>
        /// 获取或设置当前关卡收集的金币数量
        /// </summary>
        public int coins
        {
            get { return m_coins; }

            set
            {
                m_coins = value;
                OnCoinsSet?.Invoke(m_coins);
            }
        }

        /// <summary>
        /// 获取当前关卡星星收集状态的副本数组
        /// </summary>
        public bool[] stars => (bool[])m_stars.Clone();

        /// <summary>
        /// 获取当前关卡已进行的时间(秒)
        /// </summary>
        public float time { get; protected set; }

        /// <summary>
        /// 获取或设置是否停止计时器更新
        /// </summary>
        public bool stopTime { get; set; } = true;

        protected int m_coins;
        protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];

        protected Game m_game;
        protected GameLevel m_level;

        /// <summary>
        /// 重置关卡分数数据为默认值
        /// </summary>
        public virtual void Reset()
        {
            time = 0;
            coins = 0;

            if (m_level != null)
            {
                m_stars = (bool[])m_level.stars.Clone();
            }
        }

        /// <summary>
        /// 收集指定索引的星星
        /// </summary>
        /// <param name="index">要收集的星星索引(0-based)</param>
        public virtual void CollectStar(int index)
        {
            m_stars[index] = true;
            OnStarsSet?.Invoke(m_stars);
        }

        /// <summary>
        /// 将当前分数数据保存到游戏存档并请求持久化
        /// </summary>
        public virtual void Consolidate()
        {
            if (m_level != null)
            {
                // 更新最佳通关时间
                if (m_level.time == 0 || time < m_level.time)
                {
                    m_level.time = time;
                }

                // 更新最高金币数
                if (coins > m_level.coins)
                {
                    m_level.coins = coins;
                }

                // 更新星星收集状态
                m_level.stars = (bool[])stars.Clone();
                m_game.RequestSaving();
            }
        }

        protected virtual void Start()
        {
            m_game = Game.instance;
            m_level = m_game?.GetCurrentLevel();

            if (m_level != null)
            {
                m_stars = (bool[])m_level.stars.Clone();
            }

            OnScoreLoaded?.Invoke();
        }

        protected virtual void Update()
        {
            if (!stopTime)
            {
                time += Time.deltaTime;
            }
        }
    }
}