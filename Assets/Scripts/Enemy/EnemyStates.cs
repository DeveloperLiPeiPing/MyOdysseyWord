using UnityEngine;

namespace Odyssey.Script
{
	[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/New Enemy Stats")]
	public class EnemyStates : EntityStats<EnemyStates>
	{
		[Header("General States")]
		public float gravity = 35f;
		public float snapForce = 15f;
		public float rotationSpeed = 970f;
		public float deceleration = 28f;
		public float friction = 16f;
		public float turningDrag = 28f;

		[Header("Contact Attack States")]
		public bool canAttackOnContact = true;
		public bool contactPushback = true;
		public float contactOffset = 0.15f;
		public int contactDamage = 1;
		public float contactPushBackForce = 18f;
		public float contactSteppingTolerance = 0.1f;

		[Header("View States")]
		public float spotRange = 5f;
		public float viewRange = 8f;

		[Header("Follow States")]
		public float followAcceleration = 10f;
		public float followTopSpeed = 4f;

		[Header("Waypoint States")]
		public bool faceWaypoint = true;
		public float waypointMinDistance = 0.5f;
		public float waypointAcceleration = 10f;
		public float waypointTopSpeed = 2f;
	}
}
