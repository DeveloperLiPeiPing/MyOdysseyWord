using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey.Script
{
	[AddComponentMenu("Waypoint/Waypoint Manager")]
	public class WaypointManager : MonoBehaviour
	{
		public WaypointMode mode;
		public float waitTime;
		public List<Transform> waypoints;

		protected Transform m_current;

		protected bool m_pong;
		protected bool m_changing;

		/// <summary>
		/// The instance of the current Waypoint.
		/// </summary>
		public Transform current
		{
			get
			{
				if (!m_current)
				{
					m_current = waypoints[0];
				}

				return m_current;
			}

			protected set { m_current = value; }
		}

		/// <summary>
		/// 返回当前航点的索引
		/// </summary>
		public int index => waypoints.IndexOf(current);

		/// <summary>
		/// 根据航点模式将当前航点更改为下一个航点。
		/// </summary>
		public virtual void Next()
		{
			if (m_changing)
			{
				return;
			}

			if (mode == WaypointMode.PingPong)
			{
				if (!m_pong)
				{
					m_pong = (index + 1 == waypoints.Count);
				}
				else
				{
					m_pong = (index - 1 >= 0);
				}

				var next = !m_pong ? index + 1 : index - 1;
				StartCoroutine(Change(next));
			}
			else if (mode == WaypointMode.Loop)
			{
				if (index + 1 < waypoints.Count)
				{
					StartCoroutine(Change(index + 1));
				}
				else
				{
					StartCoroutine(Change(0));
				}
			}
			else if (mode == WaypointMode.Once)
			{
				if (index + 1 < waypoints.Count)
				{
					StartCoroutine(Change(index + 1));
				}
			}
		}

		protected virtual IEnumerator Change(int to)
		{
			m_changing = true;
			yield return new WaitForSeconds(waitTime);
			current = waypoints[to];
			m_changing = false;
		}
	}
}
