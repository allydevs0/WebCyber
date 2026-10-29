using UnityEngine;

namespace Reptile
{
	public class EnemyLockOnAiming : MonoBehaviour
	{
		[SerializeField]
		private float lowAlignmentVelocityFactor = 0.6f;

		[SerializeField]
		private float highAlignmentVelocityFactor = 3f;

		private Vector3 targetPos;

		private Player player;

		private EnemyPositioningInfo info;

		private EnemyFirearm firearm;

		private float sampleVelTimer;

		private Vector3 sampleVel;

		private Vector3 currentVel;

		private Vector3 targetVel;

		private Vector3 localVel;

		private bool lockOn;

		private float lockOnTimeOut;

		private float alignmentFactor;

		public bool LockOn => lockOn;

		public void Init(BasicEnemy enemy, Player playerToTarget, EnemyPositioningInfo positionInfo, EnemyFirearm targetedFirearm)
		{
			player = playerToTarget;
			info = positionInfo;
			firearm = targetedFirearm;
			enemy.OnUpdateActive.AddListener(AimingUpdate);
		}

		public void ResetAiming()
		{
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			insideUnitSphere.y = 0f;
			insideUnitSphere = insideUnitSphere.normalized * Mathf.Min(20f, info.DistToPlayer / 2f);
			targetPos = player.tf.position + insideUnitSphere;
			currentVel = Vector3.zero;
			SamplePlayerVelocity();
		}

		public void AimingUpdate()
		{
			Vector3 vector = player.tf.position - targetPos;
			vector.y = 0f;
			_ = vector.normalized;
			sampleVelTimer -= Core.dt;
			if (sampleVelTimer < 0f)
			{
				SamplePlayerVelocity();
			}
			alignmentFactor = 1f - Mathf.Clamp01(player.GetPracticalWorldVelocity().sqrMagnitude * Core.dt / Time.fixedDeltaTime / (Mathf.Pow(player.boostSpeed, 2f) * (player.TreatPlayerAsSortaGrounded() ? 1f : 0.5f)));
			localVel = Vector3.Lerp(sampleVel * lowAlignmentVelocityFactor, vector * highAlignmentVelocityFactor, alignmentFactor);
			targetVel = localVel;
			currentVel = Vector3.Lerp(currentVel, targetVel, player.TreatPlayerAsSortaGrounded() ? 0.1f : 0.04f);
			targetPos += currentVel * Core.dt;
			if (lockOn)
			{
				if (!InLockOnRange(0.35f))
				{
					lockOnTimeOut += Core.dt;
				}
				else
				{
					lockOnTimeOut = 0f;
				}
				if (lockOnTimeOut > 0.5f || !InLockOnRange(0.6f))
				{
					lockOn = false;
				}
			}
			CheckLockOnTarget();
		}

		private void SamplePlayerVelocity()
		{
			sampleVel = player.GetPracticalWorldVelocity() * Core.dt / Time.fixedDeltaTime;
			sampleVel.y = 0f;
			sampleVelTimer = Mathf.Lerp(1f, 0.1f, alignmentFactor);
		}

		private void CheckLockOnTarget()
		{
			if (InLockOnRange() && !lockOn)
			{
				SamplePlayerVelocity();
				currentVel = sampleVel;
				targetPos = player.tf.position;
				lockOn = true;
				lockOnTimeOut = 0f;
			}
		}

		private bool InLockOnRange(float range = 0.5f)
		{
			Vector3 vector = player.tf.position - targetPos;
			vector.y = 0f;
			return vector.sqrMagnitude < range * range;
		}

		public Vector3 GetTargetPos()
		{
			return targetPos;
		}
	}
}
