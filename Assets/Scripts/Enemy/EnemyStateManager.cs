using System.Collections.Generic;
using UnityEngine;

namespace Odyssey.Script
{
	[RequireComponent(typeof(Enemy))]
	[AddComponentMenu("Enemy/Enemy State Manager")]
	public class EnemyStateManager : EntityStateManager<Enemy>
	{
		[ClassTypeName(typeof(EnemyState))]
		public string[] states;

		protected override List<EntityState<Enemy>> GetStateList()
		{
			return EnemyState.CreateListFromStringArray(states);
		}
	}
}
