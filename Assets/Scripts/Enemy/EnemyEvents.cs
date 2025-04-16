using System;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[Serializable]
	public class EnemyEvents
	{
		/// <summary>
		/// 当玩家进入敌人范围时调用。
		/// </summary>
		public UnityEvent OnPlayerSpotted;

		/// <summary>
		/// 当玩家离开敌人视线时调用.
		/// </summary>
		public UnityEvent OnPlayerScaped;

		/// <summary>
		/// 当敌人触碰玩家时调用.
		/// </summary>
		public UnityEvent OnPlayerContact;

		/// <summary>
		/// 当敌人受到伤害时调用
		/// </summary>
		public UnityEvent OnDamage;

		/// <summary>
		/// 当这个敌人失去所有健康时调用。
		/// </summary>
		public UnityEvent OnDie;

		/// <summary>
		/// 当敌人复活时调用
		/// </summary>
		public UnityEvent OnRevive;
	}
}
