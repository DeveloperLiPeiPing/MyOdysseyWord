using UnityEngine;

namespace Odyssey.Script
{
	[RequireComponent(typeof(WaypointManager))]
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("Misc/Moving Platform")]
	public class MovingPlatform : MonoBehaviour
	{
		public float speed = 3f;

		public WaypointManager waypoints { get; protected set; }

		protected virtual void Awake()
		{
			tag = GameTags.Platform;
			waypoints = GetComponent<WaypointManager>();
		}

		protected virtual void Update()
		{
			var position = transform.position;
			var target = waypoints.current.position;
			position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
			transform.position = position;

			if (Vector3.Distance(transform.position, target) == 0)
			{
				waypoints.Next();
			}
		}
	}
}
