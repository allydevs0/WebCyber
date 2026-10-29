using UnityEngine;

namespace Reptile
{
	public class CopterCopSpawner : BasicEnemySpawner, IInitializableSceneObject
	{
		[SerializeField]
		private Transform startOffsetLocation;

		[SerializeField]
		private LayerMask environmentBlockingLayers;

		[SerializeField]
		private float maxVerticalRange = 30f;

		public override bool CanSpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition)
		{
			if (targetEnemyType != EnemyType.COPTER)
			{
				return false;
			}
			if (Mathf.Abs((tf.position - player.tf.position).y) > maxVerticalRange)
			{
				return false;
			}
			if (!WorldHandler.instance.IsReallyInScreen(startOffsetLocation.position))
			{
				return false;
			}
			if (!Physics.Raycast(startOffsetLocation.position, (WorldHandler.instance.CurrentCameraPosition - startOffsetLocation.position).normalized, out var _, Vector3.Distance(startOffsetLocation.position, WorldHandler.instance.CurrentCameraPosition), environmentBlockingLayers, QueryTriggerInteraction.Ignore))
			{
				return false;
			}
			return base.CanSpawnEnemy(targetEnemyType, playerPosition);
		}

		public override bool SpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition, bool forceSpawn = false)
		{
			if (CanSpawnEnemy(targetEnemyType, playerPosition) | forceSpawn)
			{
				for (int i = 0; i < spawnableEnemies.Length; i++)
				{
					if (spawnableEnemies[i].GetComponent<BasicEnemy>().EnemyType != targetEnemyType)
					{
						continue;
					}
					PoolableObjectToName poolableObjectToName = null;
					for (int j = 0; j < enemies.Count; j++)
					{
						if (enemies[j].name == spawnableEnemies[i].name)
						{
							poolableObjectToName = enemies[j];
							break;
						}
					}
					if (poolableObjectToName != null)
					{
						IPoolable poolableFromPoolOrInstantiate = poolableInstantiator.GetPoolableFromPoolOrInstantiate(poolableObjectToName);
						poolableFromPoolOrInstantiate.InitPoolable(startOffsetLocation.position, startOffsetLocation.rotation);
						spawnedEnemy = poolableFromPoolOrInstantiate as BasicEnemy;
						spawnedEnemy.InitFromSpawn(null);
						(spawnedEnemy as CopterCop).SetSpawnDestination(tf.position);
						return true;
					}
				}
			}
			return false;
		}
	}
}
