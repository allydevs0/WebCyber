using UnityEngine;

namespace Reptile
{
	public class EnemyVelocityAiming : MonoBehaviour
	{
		[SerializeField]
		private float aimAcc = 3f;

		[SerializeField]
		private float maxAimSpeed = 20f;

		private float aimMaxSpeed;

		private Vector3 targetPos;

		private Vector3 aimVel;

		private Player player;

		public void Init(Player playerToTarget)
		{
			Core.OnUpdate += AimingUpdate;
			player = playerToTarget;
		}

		public virtual void OnDestroy()
		{
			Core.OnUpdate -= AimingUpdate;
		}

		public void ResetAiming()
		{
			aimMaxSpeed = maxAimSpeed;
			targetPos = base.transform.position;
			Vector3 vector = player.tf.position - targetPos;
			vector.y = 0f;
			Vector3 normalized = vector.normalized;
			if (vector.magnitude < 10f)
			{
				targetPos = player.tf.position;
			}
			else
			{
				targetPos += normalized * 10f;
			}
			targetPos.y = base.transform.position.y;
			aimVel = normalized * aimMaxSpeed * 0.25f;
		}

		public void AimingUpdate()
		{
			Vector3 vector = player.tf.position - targetPos;
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
			Vector3 normalized = vector2.normalized;
			if (vector2.magnitude < 3f)
			{
				aimMaxSpeed = Mathf.Max(player.GetForwardSpeed() + 1f, 7f);
				normalized = vector.normalized;
			}
			else
			{
				aimMaxSpeed = maxAimSpeed / 2f;
			}
			aimVel += normalized * aimAcc;
			if (aimVel.magnitude > aimMaxSpeed)
			{
				aimVel = aimVel.normalized * aimMaxSpeed;
			}
			targetPos += aimVel * Core.dt;
		}

		public Vector3 GetTargetPos()
		{
			return targetPos;
		}

		public Vector3 GetSpread()
		{
			return Vector3.zero;
		}
	}
}
