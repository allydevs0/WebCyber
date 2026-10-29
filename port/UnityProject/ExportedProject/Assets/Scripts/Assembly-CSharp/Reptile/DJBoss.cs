using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class DJBoss : BasicEnemy
	{
		public enum DJState
		{
			IDLE = 0,
			MINEATTACK = 1,
			VINYLATTACK = 2,
			VINYLIMPACT = 3,
			KICKIMPACT = 4,
			JUMPUP = 5,
			JUMPDOWN = 6,
			DIE = 7,
			NONE = 8
		}

		public enum MovementPosition
		{
			GROUND = 0,
			CEILING = 1
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
		private Transform muzzleLeft;

		[SerializeField]
		private Transform muzzleRight;

		[SerializeField]
		private AnimationCurve jumpDownCompensationCurve;

		[SerializeField]
		private AudioSource moveLoopAudioSource;

		private DJState lastState;

		private DJState djState;

		private DJState nextState;

		private float stateTime;

		private MovementPosition currentMovementPosition;

		private MovementPosition targetMovementPosition;

		private List<Transform> route = new List<Transform>();

		private float getAwaySpeed = 30f;

		private float tooCloseSpeed = 16f;

		private float normalSpeed = 12f;

		private float tooFarSpeed = 9f;

		private AudioSource oneShotAudioSource;

		private int closedIdleHash = Animator.StringToHash("closedIdle");

		private int moveHash = Animator.StringToHash("move");

		private int moveUpsideDownHash = Animator.StringToHash("moveUpsideDown");

		private int openIdleHash = Animator.StringToHash("openIdle");

		private int openIdleUpsideDownHash = Animator.StringToHash("openIdleUpsideDown");

		private int vinylImpactHash = Animator.StringToHash("vinylImpact");

		private int vinylImpactDropDownHash = Animator.StringToHash("vinylImpactDropDown");

		private int kickImpactHash = Animator.StringToHash("kickImpact");

		private int vinylThrowAHash = Animator.StringToHash("vinylThrowA");

		private int vinylThrowBHash = Animator.StringToHash("vinylThrowB");

		private int vinylThrowUpsideDownAHash = Animator.StringToHash("vinylThrowUpsideDownA");

		private int vinylThrowUpsideDownBHash = Animator.StringToHash("vinylThrowUpsideDownB");

		private int jumpLandHash = Animator.StringToHash("jumpLand");

		private int jumpHash = Animator.StringToHash("jump");

		private int jumpDirectionHash = Animator.StringToHash("jumpDirection");

		private int jumpToCeilingHash = Animator.StringToHash("jumpToCeiling");

		private int dropToGroundHash = Animator.StringToHash("dropToGround");

		private int movementSpeedHash = Animator.StringToHash("movementSpeed");

		private int kickImpactDeathHash = Animator.StringToHash("kickImpactDeath");

		private float baseOffset;

		private float targetBaseOffset;

		private float lastEscapeTime;

		private float dropDownHeight;

		private bool startedAttackFromFar;

		private int nextRouteNode = 1;

		private bool threwWithLeft;

		private Vector3 lastMinePosition;

		private float headHeightPrev;

		private int lastClosestNode;

		private int closestNode;

		private bool movingForward = true;

		private bool escapeDirForward = true;

		private int currentRouteNode;

		private float timeTooFar;

		private float timeTooNear;

		private float timeMiddle;

		private float attackTimer;

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
			animationEventRelay.Init();
			oneShotAudioSource = animationEventRelay.oneShotAudioSource;
			GameObject gameObject = GameObject.Find("PathDJVehicle");
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
			SetState(DJState.IDLE);
			tf.position = route[0].position;
			tf.rotation = route[0].rotation;
			navmeshMovement.SetRotationSpeed(3f);
			navmeshMovement.Activate();
		}

		internal static void PlayDrivingLoopSound(AudioSource audioSource)
		{
			if (audioSource != null && !audioSource.isPlaying)
			{
				Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.StorySfx, AudioClipID.ch1s12_DJDrivingLoop, audioSource);
			}
		}

		internal static void StopDrivingLoopSound(AudioSource audioSource)
		{
			if (audioSource != null && audioSource.isPlaying)
			{
				Core.Instance.AudioManager.StopLoopingSfx(audioSource);
			}
		}

		private void SetState(DJState targetState)
		{
			ExitState(djState);
			stateTime = 0f;
			lastState = djState;
			djState = targetState;
			switch (djState)
			{
			case DJState.IDLE:
				hitByVinyl = false;
				navmeshMovement.SetMovementSpeed(normalSpeed);
				navmeshMovement.LookAtMoveDir(instant: false);
				animations.PlayAnim(closedIdleHash);
				ResetRubberBand();
				PlayDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.VINYLATTACK:
			{
				for (int i = 0; i < dustClouds.Length; i++)
				{
					dustClouds[i].SetActive(value: true);
				}
				hitByVinyl = false;
				startedAttackFromFar = positioningInfo.SqrFlatDistToPlayer > 1600f;
				navmeshMovement.DoKnockBack(tf.forward, 10f, 0f, 1f, lookAtMoveDir: false, forceKnockBack: true);
				navmeshMovement.LookAtCurrentDir(instant: false);
				if (currentMovementPosition == MovementPosition.CEILING)
				{
					animations.PlayAnim(openIdleUpsideDownHash);
				}
				else
				{
					animations.PlayAnim(openIdleHash);
				}
				ResetRubberBand();
				StopDrivingLoopSound(moveLoopAudioSource);
				break;
			}
			case DJState.MINEATTACK:
				hitByVinyl = false;
				navmeshMovement.SetMovementSpeed(normalSpeed);
				navmeshMovement.MoveToStaticTarget(route[nextRouteNode].position);
				navmeshMovement.LookAtMoveDir(instant: false);
				PlayDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.VINYLIMPACT:
				if (lastState != DJState.JUMPDOWN)
				{
					animations.PlayAnim(vinylImpactHash);
					navmeshMovement.LookAtCurrentDir(instant: false);
					navmeshMovement.DoKnockBack(-positioningInfo.FlatDirToPlayer, 4f, 0f, 1f, lookAtMoveDir: false);
				}
				hitByVinyl = false;
				targetBaseOffset = 0f;
				ResetRubberBand();
				lastEscapeTime = Time.time;
				ResetAttackTimer();
				StopDrivingLoopSound(moveLoopAudioSource);
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
				ResetRubberBand();
				lastEscapeTime = Time.time;
				StopDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.JUMPUP:
				hitByVinyl = false;
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				currentMovementPosition = MovementPosition.CEILING;
				animations.PlayAnim(jumpToCeilingHash, restart: false);
				StopDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.JUMPDOWN:
				hitByVinyl = false;
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				currentMovementPosition = MovementPosition.GROUND;
				dropDownHeight = navmeshMovement.Agent.baseOffset;
				baseOffset = navmeshMovement.Agent.baseOffset;
				if (nextState == DJState.VINYLIMPACT)
				{
					animations.PlayAnim(vinylImpactDropDownHash);
				}
				else
				{
					animations.PlayAnim(dropToGroundHash);
				}
				StopDrivingLoopSound(moveLoopAudioSource);
				break;
			case DJState.DIE:
				animations.PlayAnim(kickImpactDeathHash);
				StopDrivingLoopSound(moveLoopAudioSource);
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
				AdjustRequiredMovementPosition();
				if (!MoveAwayOnCeiling())
				{
					MoveAwayOnGround();
				}
				break;
			case DJState.VINYLATTACK:
				UpdateNodes();
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
							if (currentMovementPosition == MovementPosition.CEILING)
							{
								animations.PlayAnim(vinylThrowUpsideDownAHash, restart: false);
							}
							else
							{
								animations.PlayAnim(vinylThrowAHash, restart: false);
							}
						}
						if (!threwWithLeft)
						{
							if (currentMovementPosition == MovementPosition.CEILING)
							{
								animations.PlayAnim(vinylThrowUpsideDownBHash, restart: false);
							}
							else
							{
								animations.PlayAnim(vinylThrowBHash, restart: false);
							}
						}
					}
					if (stateTime % vinylShooter.FireInterval > vinylShooter.FireInterval / 2f && vinylShooter.CanShoot)
					{
						vinylShooter.Shoot(vinylShooter.Muzzle.position, ((positioningInfo.SqrDistToPlayer > 100f) ? Vector3.Lerp(Vector3.Lerp(tf.position, player.tf.position + 1.5f * Vector3.up, 0.3f), player.tf.position + 1.5f * Vector3.up, (stateTime - 1f) / 3f) : (player.tf.position + 1.5f * Vector3.up)) - vinylShooter.Muzzle.position, aiming.TargetAbility);
						threwWithLeft = !threwWithLeft;
					}
				}
				AdjustRequiredBaseOffset();
				if (currentMovementPosition == MovementPosition.CEILING)
				{
					baseOffset = targetBaseOffset;
				}
				if ((((!(positioningInfo.DistToPlayer < 5f) || !(stateTime > 3f)) && (!(positioningInfo.DistToPlayer < 8f) || !(stateTime > 20f)) && (!startedAttackFromFar || !(positioningInfo.SqrFlatDistToPlayer < 900f))) || (!MoveAwayOnCeiling() && !MoveAwayOnGround())) && hitByVinyl)
				{
					if (currentMovementPosition == MovementPosition.CEILING)
					{
						nextState = DJState.VINYLIMPACT;
						SetState(DJState.JUMPDOWN);
					}
					else
					{
						SetState(DJState.VINYLIMPACT);
					}
				}
				break;
			case DJState.MINEATTACK:
				SetAnimationMovementSpeed();
				UpdateAttackTimer();
				UpdateNodes();
				AdjustRequiredMovementPosition();
				if (Time.time - lastEscapeTime < 3f)
				{
					navmeshMovement.SetMovementSpeed(getAwaySpeed);
				}
				else
				{
					RubberBand();
				}
				if (currentMovementPosition == MovementPosition.CEILING)
				{
					animations.PlayAnim(moveUpsideDownHash);
				}
				else
				{
					animations.PlayAnim(moveHash);
				}
				if (currentMovementPosition == MovementPosition.CEILING)
				{
					baseOffset = targetBaseOffset;
				}
				navmeshMovement.MoveToStaticTarget(route[nextRouteNode].position);
				if ((tf.position - lastMinePosition).sqrMagnitude > 9f)
				{
					float num = Random.Range(-5, 5);
					if (num < 0f)
					{
						if (currentMovementPosition == MovementPosition.GROUND)
						{
							spiderShooter.SetMuzzle(muzzleLeft);
						}
						else
						{
							spiderShooter.SetMuzzle(muzzleRight);
						}
					}
					else if (currentMovementPosition == MovementPosition.GROUND)
					{
						spiderShooter.SetMuzzle(muzzleRight);
					}
					else
					{
						spiderShooter.SetMuzzle(muzzleLeft);
					}
					if (Physics.Raycast(spiderShooter.Muzzle.position + tf.back() * 5f + tf.right * num, -Vector3.up, out var hitInfo, 20f, layerMask, QueryTriggerInteraction.Ignore))
					{
						spiderShooter.Shoot(spiderShooter.Muzzle.position, (hitInfo.point - spiderShooter.Muzzle.position).normalized, 0f);
						lastMinePosition = tf.position;
					}
				}
				if (MoveAwayOnCeiling() || MoveAwayOnGround())
				{
					break;
				}
				if ((positioningInfo.SqrFlatDistToPlayer > 1600f && timeTooFar > 12f) || timeMiddle > 7f)
				{
					SetState(DJState.VINYLATTACK);
				}
				else if (hitByVinyl || (wasHit && Time.time - lastEscapeTime > 5f))
				{
					if (currentMovementPosition == MovementPosition.CEILING)
					{
						nextState = DJState.VINYLIMPACT;
						SetState(DJState.JUMPDOWN);
					}
					else
					{
						SetState(DJState.VINYLIMPACT);
					}
				}
				break;
			case DJState.VINYLIMPACT:
				baseOffset = 0f;
				UpdateNodes();
				AdjustRequiredMovementPosition();
				if (stateTime > 4f)
				{
					navmeshMovement.LookAtPosition(route[nextRouteNode].position, instant: false);
					navmeshMovement.MoveToStaticTarget(route[nextRouteNode].position);
				}
				if (wasHit && stateTime > 0.5f)
				{
					SetState(DJState.KICKIMPACT);
				}
				else if (hitByVinyl)
				{
					SetState(DJState.VINYLIMPACT);
				}
				else if ((double)stateTime > 4.5 && !MoveAwayOnCeiling())
				{
					MoveAwayOnGround();
				}
				break;
			case DJState.KICKIMPACT:
				baseOffset = 0f;
				AdjustRequiredMovementPosition();
				UpdateNodes();
				if ((!(stateTime > 1.5f) || (!MoveAwayOnCeiling() && !MoveAwayOnGround())) && HP <= 0)
				{
					SetState(DJState.DIE);
				}
				break;
			case DJState.JUMPUP:
				if (stateTime > 0.41f)
				{
					baseOffset = Mathf.Lerp(0f, targetBaseOffset, (stateTime - 0.41f) * 4f);
				}
				if (stateTime > 1f)
				{
					if (nextState == DJState.MINEATTACK)
					{
						SetState(DJState.MINEATTACK);
						nextState = DJState.NONE;
					}
					else if (nextState == DJState.VINYLATTACK)
					{
						SetState(DJState.VINYLATTACK);
						nextState = DJState.NONE;
					}
				}
				break;
			case DJState.JUMPDOWN:
			{
				float t = (stateTime - 0.1f) * 2f;
				baseOffset = Mathf.Lerp(dropDownHeight, 0f, t);
				if (baseOffset == 0f && stateTime > ((nextState == DJState.VINYLIMPACT) ? 0.6f : 1f))
				{
					if (nextState == DJState.MINEATTACK)
					{
						SetState(DJState.MINEATTACK);
						nextState = DJState.NONE;
					}
					else if (nextState == DJState.VINYLATTACK)
					{
						SetState(DJState.VINYLATTACK);
						nextState = DJState.NONE;
					}
					else if (nextState == DJState.VINYLIMPACT)
					{
						SetState(DJState.VINYLIMPACT);
						nextState = DJState.NONE;
					}
				}
				break;
			}
			case DJState.DIE:
				if (stateTime > 10f)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			}
		}

		private void ExitState(DJState state)
		{
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
				if (num2 < num && escapeDirForward)
				{
					escapeDirForward = false;
				}
				else if (num2 > num && !escapeDirForward)
				{
					escapeDirForward = true;
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
			if (num2 < num && !escapeDirForward)
			{
				escapeDirForward = true;
			}
			else if (num2 > num && escapeDirForward)
			{
				escapeDirForward = false;
			}
		}

		private void UpdateNodes()
		{
			CalcPlayerDirection();
			CalcForwardDist();
			Vector3 vector = route[nextRouteNode].position - tf.position;
			vector.y = 0f;
			int nextNode = GetNextNode(nextRouteNode);
			Vector3 vector2 = route[nextNode].position - tf.position;
			vector2.y = 0f;
			if (vector.magnitude < 2f)
			{
				currentRouteNode = nextRouteNode;
				nextRouteNode = nextNode;
			}
		}

		private int GetNextNode(int curNode)
		{
			int num = curNode;
			num = ((!escapeDirForward) ? (num - 1) : (num + 1));
			if (num >= route.Count)
			{
				num = 0;
			}
			if (num < 0)
			{
				num = route.Count - 1;
			}
			return num;
		}

		private void RubberBand()
		{
			if (positioningInfo.SqrFlatDistToPlayer < 64f)
			{
				timeTooNear += Core.dt * 2f;
				if (timeTooFar > 0f)
				{
					timeTooFar -= Core.dt * 4f;
				}
				if (timeMiddle > 0f)
				{
					timeMiddle -= Core.dt * 4f;
				}
				navmeshMovement.SetMovementSpeed(Mathf.Lerp(navmeshMovement.MovementSpeed, tooCloseSpeed, timeTooNear));
			}
			else if (positioningInfo.SqrFlatDistToPlayer > 900f)
			{
				if (timeTooNear > 0f)
				{
					timeTooNear -= Core.dt * 2f;
				}
				if (timeMiddle > 0f)
				{
					timeMiddle -= Core.dt * 2f;
				}
				timeTooFar += Core.dt;
				navmeshMovement.SetMovementSpeed(Mathf.Lerp(navmeshMovement.MovementSpeed, tooFarSpeed, timeTooFar));
			}
			else
			{
				if (timeTooNear > 0f)
				{
					timeTooNear -= Core.dt * 2f;
				}
				if (timeTooFar > 0f)
				{
					timeTooFar -= Core.dt * 2f;
				}
				timeMiddle += Core.dt;
				navmeshMovement.SetMovementSpeed(Mathf.Lerp(navmeshMovement.MovementSpeed, normalSpeed, timeMiddle));
			}
		}

		private void ResetRubberBand()
		{
			timeTooFar = 0f;
			timeTooNear = 0f;
			timeMiddle = 0f;
		}

		private void UpdateAttackTimer()
		{
			attackTimer += Core.dt;
		}

		private void ResetAttackTimer()
		{
			attackTimer = 0f;
		}

		private void SetAnimationMovementSpeed()
		{
			float magnitude = navmeshMovement.Agent.velocity.magnitude;
			animations.SetFloatParameter(movementSpeedHash, magnitude);
		}

		private void AdjustRequiredBaseOffset()
		{
			if (Physics.Raycast(tf.position - navmeshMovement.Agent.baseOffset * Vector3.up, Vector3.up, out var hitInfo, 10f, ceilignCheckLayerMask, QueryTriggerInteraction.Ignore))
			{
				targetBaseOffset = hitInfo.point.y - (tf.position.y - navmeshMovement.Agent.baseOffset);
				targetMovementPosition = MovementPosition.CEILING;
			}
		}

		private void AdjustRequiredMovementPosition()
		{
			if (route[nextRouteNode].name.Contains("Up"))
			{
				if (Physics.Raycast(tf.position - navmeshMovement.Agent.baseOffset * Vector3.up, Vector3.up, out var hitInfo, 10f, ceilignCheckLayerMask, QueryTriggerInteraction.Ignore))
				{
					targetBaseOffset = hitInfo.point.y - (tf.position.y - navmeshMovement.Agent.baseOffset);
					targetMovementPosition = MovementPosition.CEILING;
				}
				else
				{
					targetBaseOffset = 0f;
					targetMovementPosition = MovementPosition.GROUND;
				}
			}
			else
			{
				targetBaseOffset = 0f;
				targetMovementPosition = MovementPosition.GROUND;
			}
		}

		private bool MoveAwayOnGround()
		{
			bool result = false;
			if (currentMovementPosition == MovementPosition.GROUND && targetMovementPosition == MovementPosition.GROUND && djState != DJState.MINEATTACK)
			{
				SetState(DJState.MINEATTACK);
				result = true;
			}
			if (currentMovementPosition == MovementPosition.CEILING && targetMovementPosition == MovementPosition.GROUND)
			{
				SetState(DJState.JUMPDOWN);
				nextState = DJState.MINEATTACK;
				result = true;
			}
			return result;
		}

		private bool MoveAwayOnCeiling()
		{
			bool result = false;
			if (currentMovementPosition == MovementPosition.CEILING && targetMovementPosition == MovementPosition.CEILING && djState != DJState.MINEATTACK)
			{
				SetState(DJState.MINEATTACK);
				result = true;
			}
			if (currentMovementPosition == MovementPosition.GROUND && targetMovementPosition == MovementPosition.CEILING)
			{
				SetState(DJState.JUMPUP);
				nextState = DJState.MINEATTACK;
				result = true;
			}
			return result;
		}

		public void HitByVinyl()
		{
			hitByVinyl = true;
		}

		public override bool IsDead()
		{
			if (HP <= 0 && djState == DJState.DIE)
			{
				return stateTime >= 4f;
			}
			return false;
		}

		public override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);
			if (other.gameObject.layer == 18)
			{
				wasHit = true;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			StopDrivingLoopSound(moveLoopAudioSource);
		}
	}
}
