using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[AddComponentMenu("Level/Level Finisher")]
	public class LevelFinisher : Singleton<LevelFinisher>
	{
		/// <summary>
		/// 当关卡结束时调用。
		/// </summary>
		public UnityEvent OnFinish;

		/// <summary>
		/// 当关卡退出时调用。
		/// </summary>
		public UnityEvent OnExit;

		public bool unlockNextLevel;
		public string nextScene;
		public string exitScene;
		public float loadingDelay = 1f;

		protected Game m_game => Game.instance;
		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPause m_pauser => LevelPause.instance;
		protected GameLoader m_loader => GameLoader.instance;
		protected Fader m_fader => Fader.instance;

		protected virtual IEnumerator FinishRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_score.stopTime = true;
			m_level.player.inputs.enabled = false;

			yield return new WaitForSeconds(loadingDelay);

			if (unlockNextLevel)
			{
				m_game.UnlockNextLevel();
			}

			Game.LockCursor(false);
			m_score.Consolidate();
			m_loader.Load(nextScene);
			OnFinish?.Invoke();
		}

		protected virtual IEnumerator ExitRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(loadingDelay);
			Game.LockCursor(false);
			m_loader.Load(exitScene);
			OnExit?.Invoke();
		}

		/// <summary>
		/// 调用关卡停止所有携程分数并加载下一个场景
		/// </summary>
		public virtual void Finish()
		{
			StopAllCoroutines();
			StartCoroutine(FinishRoutine());
		}

		/// <summary>
		///调用关卡退出时，分数不保存
		/// </summary>
		public virtual void Exit()
		{
			StopAllCoroutines();
			StartCoroutine(ExitRoutine());
		}
	}
}
