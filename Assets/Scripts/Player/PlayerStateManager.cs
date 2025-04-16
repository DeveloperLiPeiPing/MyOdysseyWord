using System.Collections.Generic;
using UnityEngine;

namespace Odyssey.Script
{
	[RequireComponent(typeof(Player))]
	[AddComponentMenu("Player/Player State Manager")]
	public class PlayerStateManager : EntityStateManager<Player>
	{
		[ClassTypeName(typeof(PlayerState))]
		public string[] states;

		protected override List<EntityState<Player>> GetStateList()
		{
			return PlayerState.CreateListFromStringArray(states);
		}
	}
}
