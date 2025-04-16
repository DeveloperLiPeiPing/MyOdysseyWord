using System;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[Serializable]
	public class EntityEvents
	{
		/// <summary>
		/// 当实体着陆时调用
		/// </summary>
		public UnityEvent OnGroundEnter;

		/// <summary>
		/// 当实体离开地面时调用。
		/// </summary>
		public UnityEvent OnGroundExit;

		/// <summary>
		/// 当实体进入轨道时调用。
		/// </summary>
		public UnityEvent OnRailsEnter;

		/// <summary>
		/// 当实体存在轨道滑行时调用。
		/// </summary>
		public UnityEvent OnRailsExit;
	}
}
