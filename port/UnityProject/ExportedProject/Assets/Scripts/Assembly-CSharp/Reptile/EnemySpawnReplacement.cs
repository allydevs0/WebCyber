using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class EnemySpawnReplacement : MonoBehaviour
	{
		[SerializeField]
		private string sequence;

		[SerializeField]
		private bool requiredForCompletion = true;

		public void Spawn(int spawnGroup)
		{
			BasicEnemy component = GetComponent<BasicEnemy>();
			if (component != null)
			{
				Battle battle = component.Battle;
				if (battle != null)
				{
					battle.SpawnAddsFromGroup(spawnGroup, requiredForCompletion);
				}
			}
			GameObject gameObject = GameObject.Find(sequence);
			if (gameObject != null)
			{
				SequenceHandler.instance.StartEnteringSequence(gameObject.GetComponent<PlayableDirector>(), null, setHidePlayer: true, setInterruptPlayer: true, instantly: false, setPausePlayer: false, setAllowPhoneOnAfterSequence: false);
			}
		}
	}
}
