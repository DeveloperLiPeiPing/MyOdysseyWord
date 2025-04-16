using UnityEngine;

namespace Odyssey.Script
{
	[AddComponentMenu("Level/Level Controller")]
	public class LevelController : MonoBehaviour
	{
		protected LevelFinisher m_finisher => LevelFinisher.instance;
		protected LevelRespawn m_respawner => LevelRespawn.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPause m_pauser => LevelPause.instance;

		public virtual void Finish() => m_finisher.Finish();
		public virtual void Exit() => m_finisher.Exit();

		public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);
		public virtual void Restart() => m_respawner.Restart();

		public virtual void AddCoins(int amount) => m_score.coins += amount;
		public virtual void CollectStar(int index) => m_score.CollectStar(index);
		public virtual void ConsolidateScore() => m_score.Consolidate();

		public virtual void Pause(bool value) => m_pauser.Pause(value);
	}
}
