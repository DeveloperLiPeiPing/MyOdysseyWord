using UnityEngine;

namespace Odyssey.Script
{
	public abstract class EntityStatsManager<T> : MonoBehaviour where T : EntityStats<T>
	{
		public T[] states;

		/// <summary>
		/// 当前激活状态的实例
		/// </summary>
		public T current { get; protected set; }

		/// <summary>
		/// 从当前状态更改为所需状态
		/// </summary>
		/// <param name="to">状态编号的索引</param>
		public virtual void Change(int to)
		{
			if (to >= 0 && to < states.Length)
			{
				if (current != states[to])
				{
					current = states[to];
				}
			}
		}

		protected virtual void Start()
		{
			if (states.Length > 0)
			{
				current = states[0];
			}
		}
	}
}
