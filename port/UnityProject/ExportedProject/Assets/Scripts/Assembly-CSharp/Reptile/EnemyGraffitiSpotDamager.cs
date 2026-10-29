using UnityEngine;

namespace Reptile
{
	public class EnemyGraffitiSpotDamager : MonoBehaviour
	{
		[SerializeField]
		private GraffitiSpotFinisher[] gSpots;

		[SerializeField]
		private int damageToDeal = 1;

		private BasicEnemy linkedEnemy;

		private bool spotsActive;

		public OnGSpotStateChanged onGSpotStateChanged;

		public GraffitiSpotFinisher[] GSpots => gSpots;

		public bool SpotsActive => spotsActive;

		public void Init(BasicEnemy enemy)
		{
			for (int i = 0; i < gSpots.Length; i++)
			{
				gSpots[i].enemyDamager = this;
				gSpots[i].SetDefaultData(null);
				gSpots[i].gameObject.SetActive(value: false);
			}
			linkedEnemy = enemy;
			if (onGSpotStateChanged == null)
			{
				onGSpotStateChanged = new OnGSpotStateChanged();
			}
		}

		public virtual void SetGraffitiResult(GraffitiSpotFinisher gSpotFinisher, GraffitiState result)
		{
			if (onGSpotStateChanged != null)
			{
				onGSpotStateChanged.Invoke(gSpotFinisher, result);
			}
			if (result == GraffitiState.FINISHED)
			{
				linkedEnemy.ReceiveDamage(damageToDeal);
			}
		}

		public void SetGraffitiSpotsActive(bool active)
		{
			spotsActive = active;
			for (int i = 0; i < gSpots.Length; i++)
			{
				gSpots[i].gameObject.SetActive(active);
			}
		}

		public void ResetGraffitiSpots()
		{
			for (int i = 0; i < gSpots.Length; i++)
			{
				gSpots[i].ClearPaint();
				gSpots[i].SetState(GraffitiState.CANCEL);
				gSpots[i].gameObject.SetActive(value: false);
			}
		}
	}
}
