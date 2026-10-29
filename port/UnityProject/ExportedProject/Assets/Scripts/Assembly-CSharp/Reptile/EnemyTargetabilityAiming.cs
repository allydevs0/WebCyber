using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class EnemyTargetabilityAiming : MonoBehaviour
	{
		[SerializeField]
		private int playerPositionHistoryLength = 15;

		[SerializeField]
		private float sampleInterval = 0.03f;

		[SerializeField]
		private bool targetabilityLossOnBoost;

		private float targetability;

		private List<Vector3> playerPositionHistory = new List<Vector3>();

		private Player player;

		private float lastSampleTime;

		private Vector3 lastPlayerVel;

		private float lastPlayerMovementVariability;

		private int sampleSize = 1;

		public float TargetAbility => targetability;

		public void Init(BasicEnemy enemy, Player playerToTarget)
		{
			player = playerToTarget;
			enemy.OnUpdateActive.AddListener(UpdateAiming);
		}

		public void UpdateAiming()
		{
			if (Time.time > lastSampleTime + sampleInterval)
			{
				AddHistory();
			}
		}

		public Vector3 GetTargetPos()
		{
			if (playerPositionHistory.Count > 0)
			{
				int index = Mathf.RoundToInt(Mathf.Lerp((float)playerPositionHistory.Count - 1f, 0f, targetability));
				return playerPositionHistory[index];
			}
			return base.transform.position += base.transform.forward;
		}

		public Vector3 GetSpread()
		{
			float num = Mathf.Lerp(2f, 10f, targetability);
			return new Vector3(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f), Mathf.Sin(Time.time * 2f)) * num;
		}

		private void AddHistory()
		{
			playerPositionHistory.Add(player.tf.position);
			if (playerPositionHistory.Count > playerPositionHistoryLength)
			{
				playerPositionHistory.RemoveAt(0);
			}
			Vector3 practicalVelocity = player.GetPracticalVelocity();
			float num = Vector3.Distance(practicalVelocity, lastPlayerVel);
			if (targetabilityLossOnBoost)
			{
				if (player.boosting)
				{
					num = 0.6f;
				}
				else if (targetability > 0.5f)
				{
					num = 0f;
				}
			}
			lastPlayerVel = practicalVelocity;
			sampleSize = Mathf.Clamp(sampleSize, 0, 60);
			lastPlayerMovementVariability = lastPlayerMovementVariability * (((float)sampleSize - 1f) / (float)sampleSize) + num * (1f / (float)sampleSize);
			targetability = Mathf.Clamp01((player.GetPracticalVelocity().magnitude / 10f + 1f) * (lastPlayerMovementVariability - 0.5f));
			lastSampleTime = Time.time;
			sampleSize++;
		}
	}
}
