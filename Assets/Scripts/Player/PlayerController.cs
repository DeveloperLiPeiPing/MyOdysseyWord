using UnityEngine;

namespace Odyssey.Script
{
	[AddComponentMenu("Player/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// 增加玩家生命值1点
        /// </summary>
        /// <param name="player">玩家实例</param>
        public void AddHealth(Player player) => AddHealth(player, 1);

        /// <summary>
        /// 按指定数值增加玩家生命值
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <param name="amount">增加的生命值数量</param>
        public void AddHealth(Player player, int amount) => player.health.Increase(amount);
    }
}
