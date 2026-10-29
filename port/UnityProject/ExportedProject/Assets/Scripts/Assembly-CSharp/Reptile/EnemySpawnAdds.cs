using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class EnemySpawnAdds : MonoBehaviour
	{
		[SerializeField]
		private string sequence;

		[SerializeField]
		private int spawnGroup = 1;

		[SerializeField]
		private bool requiredForCompletion = true;

		[SerializeField]
		private int minAdds = 1;

		[SerializeField]
		private int maxAdds = 6;

		private BasicEnemy associatedEnemy;

		private Battle associatedBattle;

		public void Init(BasicEnemy enemy)
		{
			enemy.OnUpdateActive.AddListener(UpdateSpawning);
			associatedEnemy = enemy;
		}

		public void UpdateSpawning()
		{
			if (associatedBattle != null)
			{
				if (associatedBattle.SpawnedEmeiesOfType(associatedBattle.EnemyTypeOfSpawnGroup(spawnGroup)) < minAdds)
				{
					associatedBattle.SpawnAddsFromGroup(spawnGroup, requiredForCompletion, maxAdds);
					GameObject gameObject = GameObject.Find(sequence);
					if (gameObject != null)
					{
						SequenceHandler.instance.StartEnteringSequence(gameObject.GetComponent<PlayableDirector>(), null, setHidePlayer: true, setInterruptPlayer: true, instantly: false, setPausePlayer: false, setAllowPhoneOnAfterSequence: false);
					}
				}
			}
			else
			{
				associatedBattle = associatedEnemy.Battle;
			}
		}
	}
}
