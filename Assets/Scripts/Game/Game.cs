using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey.Script
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
	public class Game : Singleton<Game>
	{
		/// <summary>
		/// 当重试次数发生变化时调用。
		/// </summary>
		public UnityEvent<int> OnRetriesSet;

		/// <summary>
		/// 当请求保存时调用。
		/// </summary>
		public UnityEvent OnSavingRequested;

		public int initialRetries = 3;
		public List<GameLevel> levels;

		protected int m_retries;
		protected int m_dataIndex;
		protected DateTime m_createdAt;
		protected DateTime m_updatedAt;

		/// <summary>
		/// 重试
		/// </summary>
		public int retries
		{
			get { return m_retries; }

			set
			{
				m_retries = value;
				OnRetriesSet?.Invoke(m_retries);
			}
		}

		/// <summary>
		/// 设置光标锁定和隐藏状态。
		/// </summary>
		/// <param name="value">如果为真，光标将被隐藏。</param>
		public static void LockCursor(bool value = true)
		{
#if UNITY_STANDALONE || UNITY_WEBGL
			Cursor.visible = value;
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
		}

		/// <summary>
		/// 从给定的游戏数据加载此游戏状态。
		/// </summary>
		/// <param name="index">游戏数据的索引。</param>
		/// <param name="data">从中读取状态的游戏数据。</param>
		public virtual void LoadState(int index, GameData data)
		{
			m_dataIndex = index;
			m_retries = data.retries;
			m_createdAt = DateTime.Parse(data.createdAt);
			m_updatedAt = DateTime.Parse(data.updatedAt);

			for (int i = 0; i < data.levels.Length; i++)
			{
				levels[i].LoadState(data.levels[i]);
			}
		}

		/// <summary>
		/// 将游戏关卡数组作为关卡数据返回.
		/// </summary>
		public virtual LevelData[] LevelsData()
		{
			return levels.Select(level => level.ToData()).ToArray();
		}

		/// <summary>
		/// 如果当前场景是关卡，则返回游戏关卡。如果不是，则返回null。
		/// </summary>
		public virtual GameLevel GetCurrentLevel()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.Find((level) => level.scene == scene);
		}

		/// <summary>
		/// 从当前场景的级别列表中返回索引
		/// </summary>
		/// <returns></returns>
		public virtual int GetCurrentLevelIndex()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.FindIndex((level) => level.scene == scene);
		}

		/// <summary>
		///将游戏数据保存到其当前索引。
		/// </summary>
		public virtual void RequestSaving()
		{
			GameSaver.instance.Save(ToData(), m_dataIndex);
			OnSavingRequested?.Invoke();
		}

		/// <summary>
		/// 按场景名称解锁给定的游戏关卡
		/// </summary>
		/// <param name="sceneName">要解锁的关卡的场景名称</param>
		public virtual void UnlockLevelBySceneName(string sceneName)
		{
			var level = levels.Find((level) => level.scene == sceneName);

			if (level != null)
			{
				level.locked = false;
			}
		}

		/// <summary>
		/// 从级别列表中解锁下一级别。
		/// </summary>
		public virtual void UnlockNextLevel()
		{
			var index = GetCurrentLevelIndex() + 1;

			if (index >= 0 && index < levels.Count)
			{
				levels[index].locked = false;
			}
		}

		/// <summary>
		/// 返回此游戏的游戏数据，供数据层使用。
		/// </summary>
		public virtual GameData ToData()
		{
			return new GameData()
			{
				retries = m_retries,
				levels = LevelsData(),
				createdAt = m_createdAt.ToString(),
				updatedAt = DateTime.UtcNow.ToString()
			};
		}

		protected override void Awake()
		{
			base.Awake();
			retries = initialRetries;
			DontDestroyOnLoad(gameObject);
		}
	}
}
