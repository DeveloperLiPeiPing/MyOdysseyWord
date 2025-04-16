using System;
using UnityEngine;

namespace Odyssey.Script
{
    /// <summary>
    /// 游戏关卡数据类
    /// </summary>
    [Serializable]  // 可序列化，支持数据持久化
    public class GameLevel
    {
        public bool locked;          // 关卡是否锁定
        public string scene;         // 关联的场景名称
        public string name;          // 关卡显示名称
        public string description;   // 关卡描述文本
        public Sprite image;         // 关卡预览图

        /// <summary>
        /// 当前关卡收集的金币数量
        /// </summary>
        public int coins { get; set; }

        /// <summary>
        /// 通关该关卡的最短用时(秒)
        /// </summary>
        public float time { get; set; }

        /// <summary>
        /// 星星收集状态数组
        /// </summary>
        public bool[] stars { get; set; } = new bool[StarsPerLevel];

        /// <summary>
        /// 每个关卡包含的星星总数
        /// </summary>
        public static readonly int StarsPerLevel = 3;

        /// <summary>
        /// 从LevelData加载关卡状态
        /// </summary>
        /// <param name="data">包含关卡数据的对象</param>
        public virtual void LoadState(LevelData data)
        {
            locked = data.locked;
            coins = data.coins;
            time = data.time;
            stars = data.stars;
        }

        /// <summary>
        /// 将当前关卡数据转换为LevelData对象
        /// </summary>
        /// <returns>包含当前关卡数据的对象</returns>
        public virtual LevelData ToData()
        {
            return new LevelData()
            {
                locked = this.locked,
                coins = this.coins,
                time = this.time,
                stars = this.stars
            };
        }

        /// <summary>
        /// 将秒数格式化为 00'00"00 的时间字符串
        /// </summary>
        /// <param name="time">以秒为单位的时间值</param>
        /// <returns>格式化后的时间字符串</returns>
        public static string FormattedTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);      // 计算分钟数
            var seconds = Mathf.FloorToInt(time % 60f);     // 计算秒数
            var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);  // 计算毫秒数
            return $"{minutes:0}'{seconds:00}\"{milliseconds:00}";
        }
    }
}