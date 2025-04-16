using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[AddComponentMenu("Level/LevelPause")]
	public class LevelPause : Singleton<LevelPause>
	{
		/// <summary>
		/// 当关卡暂停时触发的事件
		/// </summary>
		public UnityEvent OnPause;

		/// <summary>
		/// 当关卡取消暂停时触发的事件
		/// </summary>
		public UnityEvent OnUnpause;

		public UIAnimator pauseScreen;

		/// <summary>
		/// 获取或设置是否可以暂停关卡
		/// </summary>
		public bool canPause { get; set; }

		/// <summary>
		/// 获取当前关卡是否处于暂停状态
		/// </summary>
		public bool paused { get; protected set; }

		/// <summary>
		/// 设置关卡的暂停状态
		/// </summary>
		/// <param name="value">要设置的暂停状态</param>
		public virtual void Pause(bool value)
		{
			if (paused != value)
			{
				if (!paused)
				{
					if (canPause)
					{
						Game.LockCursor(false);
						paused = true;
						Time.timeScale = 0;
						pauseScreen.SetActive(true);
						pauseScreen?.Show();
						OnPause?.Invoke();
					}
				}
				else
				{
					Game.LockCursor();
					paused = false;
					Time.timeScale = 1;
					pauseScreen?.Hide();
					OnUnpause?.Invoke();
				}
			}
		}
	}
}