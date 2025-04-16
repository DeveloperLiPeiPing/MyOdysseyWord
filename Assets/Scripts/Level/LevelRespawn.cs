using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[AddComponentMenu("Level/LevelRespawn")]
	public class LevelRespawn : Singleton<LevelRespawn>
	{
		/// <summary>
		/// 重生流程结束后触发的事件
		/// </summary>
		public UnityEvent OnRespawn;

		/// <summary>
		/// 游戏结束流程结束后触发的事件
		/// </summary>
		public UnityEvent OnGameOver;

		public float respawnFadeOutDelay = 1f;      // 重生淡出延迟时间
		public float respawnFadeInDelay = 0.5f;    // 重生淡入延迟时间
		public float gameOverFadeOutDelay = 5f;    // 游戏结束淡出延迟时间
		public float restartFadeOutDelay = 0.5f;   // 重新开始淡出延迟时间

		protected List<PlayerCamera> m_cameras;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPause m_pauser => LevelPause.instance;
		protected Game m_game => Game.instance;
		protected Fader m_fader => Fader.instance;

		/// <summary>
		/// 重生协程
		/// </summary>
		/// <param name="consumeRetries">是否消耗重生次数</param>
		protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
		{
			if (consumeRetries)
			{
				m_game.retries--;
			}

			m_level.player.Respawn();
			m_score.coins = 0;
			ResetCameras();
			OnRespawn?.Invoke();

			yield return new WaitForSeconds(respawnFadeInDelay);

			m_fader.FadeIn(() =>
			{
				m_pauser.canPause = true;
				m_level.player.inputs.enabled = true;
			});
		}

		/// <summary>
		/// 游戏结束协程
		/// </summary>
		protected virtual IEnumerator GameOverRoutine()
		{
			m_score.stopTime = true;
			yield return new WaitForSeconds(gameOverFadeOutDelay);
			GameLoader.instance.Reload();
			OnGameOver?.Invoke();
		}

		/// <summary>
		/// 重新开始协程
		/// </summary>
		protected virtual IEnumerator RestartRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(restartFadeOutDelay);
			GameLoader.instance.Reload();
		}

		/// <summary>
		/// 主流程协程
		/// </summary>
		/// <param name="consumeRetries">是否消耗重生次数</param>
		protected virtual IEnumerator Routine(bool consumeRetries)
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;

			if (consumeRetries && m_game.retries == 0)
			{
				StartCoroutine(GameOverRoutine());
				yield break;
			}

			yield return new WaitForSeconds(respawnFadeOutDelay);

			m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
		}

		/// <summary>
		/// 重置所有摄像机
		/// </summary>
		protected virtual void ResetCameras()
		{
			foreach (var camera in m_cameras)
			{
				camera.Reset();
			}
		}

		/// <summary>
		/// 根据剩余重生次数调用重生或游戏结束流程
		/// </summary>
		/// <param name="consumeRetries">是否消耗重生次数</param>
		public virtual void Respawn(bool consumeRetries)
		{
			StopAllCoroutines();
			StartCoroutine(Routine(consumeRetries));
		}

		/// <summary>
		/// 重新加载当前关卡场景
		/// </summary>
		public virtual void Restart()
		{
			StopAllCoroutines();
			StartCoroutine(RestartRoutine());
		}

		protected virtual void Start()
		{
			m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
			m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
		}
	}
}