using UnityEngine;

namespace Reptile
{
	public class TurretSpawner : BasicEnemySpawner
	{
		[SerializeField]
		private GameObject cover;

		protected override void UpdateSpawner()
		{
			base.UpdateSpawner();
			if (spawnedEnemy != null && spawnedEnemy.BasicState != BasicEnemy.BasicEnemyState.UNACTIVE)
			{
				if (cover.activeSelf)
				{
					cover.SetActive(value: false);
				}
			}
			else if (!cover.activeSelf)
			{
				cover.SetActive(value: true);
			}
		}
	}
}
