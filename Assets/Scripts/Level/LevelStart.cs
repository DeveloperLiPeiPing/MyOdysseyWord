using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[AddComponentMenu("Level/Level Start")]
	public class LevelStart : Singleton<LevelStart>
	{
		/// <summary>
		/// 当启动程序结束时调用。
		/// </summary>
		public UnityEvent OnStart;

		public float enablePlayerDelay = 1f;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPause m_pauser => LevelPause.instance;

		protected Fader m_fader => Fader.instance;

		protected virtual IEnumerator Routine()
		{
			Game.LockCursor();
			m_level.player.controller.enabled = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(enablePlayerDelay);
			m_score.stopTime = false;
			m_level.player.controller.enabled = true;
			m_level.player.inputs.enabled = true;
			m_pauser.canPause = true;
			OnStart?.Invoke();
		}

		protected virtual void Start()
		{
			StartCoroutine(Routine());
		}
	}
}
