using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class DJBossJumper : BasicEnemy
	{
		public enum DJState
		{
			IDLE = 0,
			JUMPAWAY = 1,
			VINYLATTACK = 2,
			VINYLIMPACT = 3,
			KICKIMPACT = 4,
			DIE = 5
		}

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private EnemyFirearm vinylShooter;

		[SerializeField]
		private EnemyFirearm spiderShooter;

		[SerializeField]
		private LayerMask layerMask;

		[SerializeField]
		private LayerMask ceilignCheckLayerMask;

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyAnimationEventRelay animationEventRelay;

		[SerializeField]
		private GameObject[] dustClouds;

		[SerializeField]
		private AudioSource moveLoopAudioSource;

		private DJState djState;

		private float stateTime;

		private AudioSource oneShotAudioSource;

		private List<Transform> route = new List<Transform>();

		private int closedIdleHash = Animator.StringToHash("closedIdle");

		private int moveHash = Animator.StringToHash("move");

		private int moveUpsideDownHash = Animator.StringToHash("moveUpsideDown");

		private int openIdleHash = Animator.StringToHash("openIdle");

		private int openIdleUpsideDownHash = Animator.StringToHash("openIdlepsideDown");

		private int vinylImpactHash = Animator.StringToHash("vinylImpact");

		private int vinylImpactDropDownHash = Animator.StringToHash("vinylImpactDropDown");

		private int kickImpactHash = Animator.StringToHash("kickImpact");

		private int vinylThrowAHash = Animator.StringToHash("vinylThrowA");

		private int vinylThrowBHash = Animator.StringToHash("vinylThrowB");

		private int vinylThrowUpsideDownHash = Animator.StringToHash("vinylThrowUpsideDown");

		private int jumpLandHash = Animator.StringToHash("jumpLand");

		private int jumpHash = Animator.StringToHash("jump");

		private int jumpDirectionHash = Animator.StringToHash("jumpDirection");

		private int movementSpeedHash = Animator.StringToHash("movementSpeed");

		private float baseOffset;

		private float targetBaseOffset;

		private int nextRouteNode;

		private bool threwWithLeft;

		private int lastClosestNode;

		private int closestNode;

		private bool movingForward = true;

		private bool escapeDirForward = true;

		private float landTime;

		private bool hitByVinyl;

		private bool wasHit;

		protected override void Setup()
		{
			base.Setup();
			aiming.Init(this, player);
			vinylShooter.Init(this, positioningInfo);
			spiderShooter.Init(this, positioningInfo);
			navmeshMovement.Init(this, positioningInfo, player);
			animations.Init(this);
			animations.SetFloatParameter(movementSpeedHash, 9f);
			animationEventRelay.Init();
			oneShotAudioSource = animationEventRelay.oneShotAudioSource;
			GameObject gameObject = GameObject.Find("PathDJJumperVehicle");
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i] != gameObject.transform)
				{
					route.Add(componentsInChildren[i]);
				}
			}
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
		}

		protected override void UpdateSpawning()
		{
			SetBasicState(BasicEnemyState.ACTIVE);
		}

		protected override void EnteringActiveState()
		{
			base.EnteringActiveState();
			HP = fullHP;
			SetState(DJState.VINYLATTACK);
			tf.position = route[0].position;
			tf.rotation = route[0].rotation;
			navmeshMovement.SetRotationSpeed(3f);
			navmeshMovement.Activate();
		}

		private void SetState(DJState targetState)
		{
			ExitState(djState);
			stateTime = 0f;
			djState = targetState;
			switch (djState)
			{
			case DJState.IDLE:
				hitByVinyl = false;
				navmeshMovement.SetMovementSpeed(11f);
				navmeshMovement.LookAtMoveDir(instant: false);
				animations.PlayAnim(closedIdleHash);
				DJBoss.PlayDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.VINYLATTACK:
			{
				hitByVinyl = false;
				navmeshMovement.LookAtCurrentDir(instant: false);
				navmeshMovement.DoKnockBack(tf.forward, 10f, 0f, 1f, lookAtMoveDir: false);
				targetBaseOffset = 0f;
				for (int i = 0; i < dustClouds.Length; i++)
				{
					dustClouds[i].SetActive(value: true);
				}
				DJBoss.StopDrivingLoopSound(moveLoopAudioSource);
				break;
			}
			case DJState.JUMPAWAY:
				UpdateNodes();
				hitByVinyl = false;
				navmeshMovement.SetMovementSpeed(20f);
				navmeshMovement.MoveToStaticTarget(route[nextRouteNode].position, useOffMeshLinks: true);
				navmeshMovement.LookAtMoveDir(instant: false);
				DJBoss.StopDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.VINYLIMPACT:
				animations.PlayAnim(vinylImpactHash);
				navmeshMovement.LookAtCurrentDir(instant: false);
				navmeshMovement.DoKnockBack(-positioningInfo.FlatDirToPlayer, 4f, 0f, 1f, lookAtMoveDir: false);
				hitByVinyl = false;
				targetBaseOffset = 0f;
				DJBoss.StopDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.KICKIMPACT:
				HP--;
				hitByVinyl = false;
				animations.PlayAnim(kickImpactHash);
				navmeshMovement.LookAtCurrentDir(instant: false);
				navmeshMovement.DoKnockBack(-positioningInfo.FlatDirToPlayer, 5f, 0f, 0.5f, lookAtMoveDir: false);
				player.hitBounceAbility.Trigger(vinylShooter.Muzzle.position + Vector3.up * 0.5f, 0.5f);
				DoHitPause(player.hitBounceAbility.hitpauseDuration + 0.1f, enemyOnly: true);
				targetBaseOffset = 0f;
				DJBoss.StopDrivingLoopSound(moveLoopAudioSource);
				break;
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			UpdateState();
			wasHit = false;
		}

		private void UpdateState()
		{
			navmeshMovement.Agent.baseOffset = baseOffset;
			stateTime += Core.dt;
			switch (djState)
			{
			case DJState.IDLE:
				AdjustRequiredBaseOffset();
				baseOffset = Mathf.Lerp(navmeshMovement.Agent.baseOffset, 0f, stateTime * 2f);
				MoveAwayOnGround();
				break;
			case DJState.VINYLATTACK:
				if (stateTime < 1f)
				{
					animations.PlayAnim(openIdleHash);
				}
				if (stateTime > 1f)
				{
					for (int i = 0; i < dustClouds.Length; i++)
					{
						dustClouds[i].SetActive(value: false);
					}
					navmeshMovement.LookAtPlayer(instant: false, oppositeDirection: true);
					if (stateTime % vinylShooter.FireInterval < 0.1f)
					{
						if (threwWithLeft)
						{
							animations.PlayAnim(vinylThrowAHash, restart: false);
						}
						if (!threwWithLeft)
						{
							animations.PlayAnim(vinylThrowBHash, restart: false);
						}
					}
					if (stateTime % vinylShooter.FireInterval > vinylShooter.FireInterval / 2f && vinylShooter.CanShoot)
					{
						vinylShooter.Shoot(vinylShooter.Muzzle.position, ((positioningInfo.SqrDistToPlayer > 100f) ? Vector3.Lerp(Vector3.Lerp(tf.position, player.tf.position + 1.5f * Vector3.up, 0.3f), player.tf.position + 1.5f * Vector3.up, (stateTime - 1f) / 3f) : (player.tf.position + 1.5f * Vector3.up)) - vinylShooter.Muzzle.position, aiming.TargetAbility);
						threwWithLeft = !threwWithLeft;
					}
				}
				if (positioningInfo.DistToPlayer < 10f && stateTime > 2f)
				{
					SetState(DJState.JUMPAWAY);
				}
				else if (hitByVinyl)
				{
					SetState(DJState.VINYLIMPACT);
				}
				break;
			case DJState.JUMPAWAY:
				MovementAnimation();
				navmeshMovement.MoveToStaticTarget(route[nextRouteNode].position, useOffMeshLinks: true);
				if (navmeshMovement.IsOnNavMeshLink())
				{
					navmeshMovement.LookAtPosition(route[nextRouteNode].position, instant: false);
				}
				if (!navmeshMovement.IsOnNavMeshLink() && Vector3.Distance(tf.position, route[nextRouteNode].position) < 10f)
				{
					SetState(DJState.VINYLATTACK);
				}
				else if (hitByVinyl || (wasHit && stateTime > 1f))
				{
					SetState(DJState.VINYLIMPACT);
				}
				break;
			case DJState.VINYLIMPACT:
				baseOffset = 0f;
				if (stateTime > 4f)
				{
					navmeshMovement.LookAtPosition(route[nextRouteNode].position, instant: false);
				}
				if (wasHit && stateTime > 0.5f)
				{
					SetState(DJState.KICKIMPACT);
				}
				else if (stateTime > 4.5f)
				{
					SetState(DJState.JUMPAWAY);
				}
				break;
			case DJState.KICKIMPACT:
				baseOffset = 0f;
				AdjustRequiredBaseOffset();
				if (stateTime > 1.5f)
				{
					SetState(DJState.JUMPAWAY);
				}
				break;
			}
		}

		private void ExitState(DJState state)
		{
			if (djState == DJState.VINYLATTACK)
			{
				for (int i = 0; i < dustClouds.Length; i++)
				{
					dustClouds[i].SetActive(value: false);
				}
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
		}

		private void CalcPlayerDirection()
		{
			lastClosestNode = closestNode;
			float num = float.MaxValue;
			for (int i = 0; i < route.Count; i++)
			{
				float num2 = Vector3.Distance(player.tf.position, route[i].position);
				if (num2 < num)
				{
					closestNode = i;
					num = num2;
				}
			}
			if (closestNode > lastClosestNode || (lastClosestNode == route.Count - 1 && closestNode == 0))
			{
				movingForward = true;
			}
			else if (closestNode < lastClosestNode || (closestNode == route.Count - 1 && lastClosestNode == 0))
			{
				movingForward = false;
			}
		}

		private void CalcForwardDist()
		{
			int num = 0;
			int num2 = 0;
			if (movingForward)
			{
				int num3 = closestNode;
				while (num3 != nextRouteNode)
				{
					num3++;
					if (num3 == route.Count)
					{
						num3 = 0;
					}
					num++;
				}
				num3 = closestNode;
				while (num3 != nextRouteNode)
				{
					num3--;
					if (num3 == -1)
					{
						num3 = route.Count - 1;
					}
					num2++;
				}
				if (num2 < num)
				{
					escapeDirForward = false;
				}
				return;
			}
			int num4 = closestNode;
			while (num4 != nextRouteNode)
			{
				num4--;
				if (num4 == -1)
				{
					num4 = route.Count - 1;
				}
				num++;
			}
			num4 = closestNode;
			while (num4 != nextRouteNode)
			{
				num4++;
				if (num4 == route.Count)
				{
					num4 = 0;
				}
				num2++;
			}
			if (num2 < num)
			{
				escapeDirForward = true;
			}
		}

		private void UpdateNodes()
		{
			nextRouteNode++;
			if (nextRouteNode >= route.Count)
			{
				nextRouteNode = 0;
			}
			if (nextRouteNode < 0)
			{
				nextRouteNode = route.Count - 1;
			}
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.IsOnNavMeshLink())
			{
				if (navmeshMovement.Landed)
				{
					landTime = 0.25f;
					animations.PlayAnim(jumpLandHash, restart: false);
					animations.SetFloatParameter(jumpDirectionHash, 0.75f);
				}
				else
				{
					animations.PlayAnim(jumpHash, restart: false);
					animations.SetFloatParameter(jumpDirectionHash, Mathf.Lerp(0.25f, 0.75f, navmeshMovement.MoveAcrossVaultProgress));
				}
			}
			else if (landTime <= 0f)
			{
				animations.PlayAnim(moveHash);
				animations.SetFloatParameter(jumpDirectionHash, 0f);
			}
			else
			{
				landTime -= Core.dt;
			}
		}

		private void AdjustRequiredBaseOffset()
		{
			if (Physics.Raycast(tf.position - navmeshMovement.Agent.baseOffset * Vector3.up, Vector3.up, out var hitInfo, 10f, ceilignCheckLayerMask, QueryTriggerInteraction.Ignore))
			{
				targetBaseOffset = hitInfo.point.y - (tf.position.y - navmeshMovement.Agent.baseOffset);
			}
			else
			{
				targetBaseOffset = 0f;
			}
		}

		private bool MoveAwayOnGround()
		{
			bool result = false;
			if (targetBaseOffset == 0f)
			{
				SetState(DJState.JUMPAWAY);
				result = true;
			}
			return result;
		}

		public void HitByVinyl()
		{
			if (!navmeshMovement.IsOnNavMeshLink())
			{
				hitByVinyl = true;
			}
		}

		public override bool IsDead()
		{
			return HP <= 0;
		}

		public override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);
			if (other.gameObject.layer == 18 && !navmeshMovement.IsOnNavMeshLink())
			{
				wasHit = true;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			DJBoss.StopDrivingLoopSound(moveLoopAudioSource);
		}
	}
}
