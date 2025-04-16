using UnityEngine;

namespace Odyssey.Script
{
	[AddComponentMenu("Enemy/States/Idle Enemy State")]
	public class IdleEnemyState : EnemyState
	{
		protected override void OnEnter(Enemy enemy) { }

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();
			enemy.Friction();
		}

		public override void OnContact(Enemy enemy, Collider other) { }
	}
}
