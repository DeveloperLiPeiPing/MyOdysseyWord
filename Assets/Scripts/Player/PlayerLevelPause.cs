using UnityEngine;

namespace Odyssey.Script
{
	[RequireComponent(typeof(Player))]
	[AddComponentMenu("Player/Player Level Pause")]
	public class PlayerLevelPause : MonoBehaviour
	{
		protected Player m_player;
		protected LevelPause m_pauser;

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_pauser = LevelPause.instance;
		}

		protected virtual void Update()
		{
			if (m_player.inputs.GetPauseDown())
			{
				
				var value = m_pauser.paused;
			
				m_pauser.Pause(!value);
			}

			if (m_pauser.paused)
				Cursor.visible = true;
			else
				Cursor.visible = false;
		}
	}
}
