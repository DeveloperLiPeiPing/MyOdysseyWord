using System;
using System.Linq;
using UnityEngine;

namespace Odyssey.Script
{
	[Serializable]
	public class GameData
	{
		public int retries;
		public LevelData[] levels;
		public string createdAt;
		public string updatedAt;

		/// <summary>
		/// 在运行时返回游戏数据的新实例。
		/// </summary>
		public static GameData Create()
		{
			return new GameData()
			{
				retries = Game.instance.initialRetries,
				createdAt = DateTime.UtcNow.ToString(),
				updatedAt = DateTime.UtcNow.ToString(),
				levels = Game.instance.levels.Select((level) =>
				{
					return new LevelData()
					{
						locked = level.locked
					};
				}).ToArray()
			};
		}

		/// <summary>
		///返回所有关卡中收集的星星的总和。
		/// </summary>
		public virtual int TotalStars()
		{
			return levels.Aggregate(0, (acc, level) =>
			{
				var total = level.CollectedStars();
				return acc + total;
			});
		}

		/// <summary>
		/// .返回所有关卡中收集的硬币的总和。
		/// </summary>
		/// <returns></returns>
		public virtual int TotalCoins()
		{
			return levels.Aggregate(0, (acc, level) => acc + level.coins);
		}

		public virtual string ToJson()
		{
			return JsonUtility.ToJson(this);
		}

		public static GameData FromJson(string json)
		{
			return JsonUtility.FromJson<GameData>(json);
		}
	}
}
