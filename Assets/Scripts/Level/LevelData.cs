using System;
using System.Linq;

namespace Odyssey.Script
{
	[Serializable]
	public class LevelData
	{
		public bool locked;
		public int coins;
		public float time;
		public bool[] stars = new bool[GameLevel.StarsPerLevel];

		/// <summary>
		/// 返回已收集的数量。
		/// </summary>
		public int CollectedStars()
		{
			return stars.Where((star) => star).Count();
		}
	}
}
