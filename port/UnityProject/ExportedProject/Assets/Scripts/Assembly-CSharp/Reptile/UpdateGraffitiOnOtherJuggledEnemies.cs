using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class UpdateGraffitiOnOtherJuggledEnemies : MonoBehaviour
	{
		private BasicEnemy linkedEnemy;

		private List<BasicEnemy> targetedEnemies = new List<BasicEnemy>();

		public void Init(BasicEnemy enemy)
		{
			linkedEnemy = enemy;
		}

		public void Finish(GraffitiSpotFinisher gSpotFinisher, GraffitiState result)
		{
			if (result == GraffitiState.PAINTING)
			{
				for (int i = 0; i < WorldHandler.instance.subscribedEnemies.Count; i++)
				{
					if ((WorldHandler.instance.subscribedEnemies[i].EnemyType == EnemyType.BASICCOP || WorldHandler.instance.subscribedEnemies[i].EnemyType == EnemyType.SHIELDCOP) && WorldHandler.instance.subscribedEnemies[i] != linkedEnemy && WorldHandler.instance.subscribedEnemies[i].GetComponent<EnemyGraffitiSpotDamager>().SpotsActive && WorldHandler.instance.IsReallyInScreen(WorldHandler.instance.subscribedEnemies[i].tf.position))
					{
						targetedEnemies.Add(WorldHandler.instance.subscribedEnemies[i]);
						WorldHandler.instance.subscribedEnemies[i].GetComponent<EnemyGraffitiSpotDamager>().SetGraffitiSpotsActive(active: false);
					}
				}
			}
			if (result == GraffitiState.PAINTING_COMPLETE && targetedEnemies.Count > 0)
			{
				for (int j = 0; j < targetedEnemies.Count; j++)
				{
					targetedEnemies[j].GetComponent<EnemyGraffitiSpotDamager>().GSpots[0].Paint(Crew.PLAYERS, gSpotFinisher.topGraffitiArt);
					targetedEnemies[j].ReceiveDamage(10);
				}
				targetedEnemies.Clear();
			}
			if (result == GraffitiState.CANCEL)
			{
				targetedEnemies.Clear();
			}
		}
	}
}
