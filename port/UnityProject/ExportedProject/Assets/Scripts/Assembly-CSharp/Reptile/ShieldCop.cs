using UnityEngine;
using UnityEngine.AI;

namespace Reptile
{
	public class ShieldCop : BasicEnemy
	{
		public enum ShieldCopState
		{
			IDLE = 0,
			MOVETOCIRCLE = 1,
			ROTATEAROUND = 2,
			ATTACK = 3,
			BLOCK = 4,
			KNOCKBACK = 5,
			KNOCKDOWN = 6,
			GETUP = 7,
			HITRESPONSE = 8,
			STUNNED = 9,
			DIE = 10,
			RUNAWAY = 11,
			RETURN = 12
		}

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyHitResponse hitBoxResponse;

		[SerializeField]
		private EnemyMeleeWeapon shieldWeapon;

		[SerializeField]
		private EnemyMeleeWeapon batonWeapon;

		[SerializeField]
		private EnemyShield shield;

		[SerializeField]
		private EnemyVoices enemyVoices;

		[SerializeField]
		private AudioClipID hitBlockAudioClipID;

		[SerializeField]
		private AudioSource hitImpactAudioSource;

		[SerializeField]
		private Renderer sprayedVisual;

		[SerializeField]
		private GameObject shieldSprayedVisual;

		[SerializeField]
		private EnemyGraffitiSpotDamager graffitiSpotDamager;

		[SerializeField]
		private UpdateGraffitiOnOtherJuggledEnemies graffitiUpdater;

		private ShieldCopState shieldCopState;

		private ShieldCopState lastState;

		private float stateTime;

		private float stunTimer;

		private Material normalMaterial;

		private Material normalGraffitiMaterial;

		private bool didDeathPause;

		private bool didDeathScream;

		private int idleTubeHash = Animator.StringToHash("idleTube");

		private int idleAgroHash = Animator.StringToHash("idleAgro");

		private int idleRunAwayHash = Animator.StringToHash("idleRunAway");

		private int moveShuffleHash = Animator.StringToHash("moveShuffle");

		private int moveRunHash = Animator.StringToHash("moveRun");

		private int runAwayHash = Animator.StringToHash("runAway");

		private int runAwayBHash = Animator.StringToHash("runAwayB");

		private int attackMeleeAHash = Animator.StringToHash("attackMeleeA");

		private int attackMeleeBHash = Animator.StringToHash("attackMeleeB");

		private int lookAroundHash = Animator.StringToHash("lookAround");

		private int beatShieldHash = Animator.StringToHash("beatShield");

		private int shieldOpenStunnedHash = Animator.StringToHash("shieldOpenStunned");

		private int shieldBlockHash = Animator.StringToHash("shieldBlock");

		private int lieDownHash = Animator.StringToHash("lieDown");

		private int vaultFenceHash = Animator.StringToHash("vaultFence");

		private int jumpDownHash = Animator.StringToHash("jumpDown");

		private int jumpDownLandHash = Animator.StringToHash("jumpDownLand");

		private int moveDirectionXHash = Animator.StringToHash("moveDirectionX");

		private int moveDirectionYHash = Animator.StringToHash("moveDirectionY");

		private int attacksReceivedWhileStunned;

		private int selectedRunAwayHash;

		private Vector3 returnPos;

		private bool canPlayEngageSound = true;

		private PoliceTubeSpawner targetPoliceTube;

		private GameObject looseWeapon;

		private GameObject looseShield;

		protected override void Setup()
		{
			base.Setup();
			animations.Init(this);
			hitBoxResponse.Init(this, player, positioningInfo, navmeshMovement, animations);
			navmeshMovement.Init(this, positioningInfo, player);
			shieldWeapon.Init(this, positioningInfo);
			batonWeapon.Init(this, positioningInfo);
			shield.Init();
			enemyVoices.Init();
			graffitiSpotDamager.Init(this);
			normalGraffitiMaterial = sprayedVisual.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			graffitiUpdater.Init(this);
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			hitBoxResponse.Deactivate();
			animations.PlayAnim(idleTubeHash);
			sprayedVisual.enabled = false;
			shieldSprayedVisual.SetActive(value: false);
			sprayedVisual.GetComponent<SkinnedMeshRenderer>().sharedMaterial = normalGraffitiMaterial;
			graffitiSpotDamager.ResetGraffitiSpots();
			graffitiSpotDamager.SetGraffitiSpotsActive(active: false);
		}

		protected override void UpdateSpawning()
		{
			base.UpdateSpawning();
			if (basicStateTime > 1.5f)
			{
				navmeshMovement.Activate();
				navmeshMovement.MoveToStaticTarget(tf.position + tf.forward * 1f);
				navmeshMovement.SetMovementSpeed(3f);
				MovementAnimation();
			}
			if (basicStateTime > 1.75f)
			{
				SetBasicState(BasicEnemyState.ACTIVE);
			}
		}

		protected override void EnteringActiveState()
		{
			base.EnteringActiveState();
			navmeshMovement.Activate();
			hitBoxResponse.Activate();
			shieldWeapon.Equip(equip: true);
			batonWeapon.Equip(equip: true);
			shield.ResetShield();
			HP = fullHP;
			selectedRunAwayHash = ((Random.value < 0.6f) ? runAwayHash : runAwayBHash);
			looseWeapon = null;
			looseShield = null;
			SetState(ShieldCopState.IDLE);
			didDeathPause = false;
			didDeathScream = false;
			canPlayEngageSound = true;
			if (CheckIfInRange(40f) && !CheckIfInRange(20f))
			{
				enemyVoices.PlaySpawn();
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			UpdateState();
		}

		private void SetState(ShieldCopState targetState)
		{
			lastState = shieldCopState;
			ExitState(shieldCopState);
			shieldCopState = targetState;
			stateTime = 0f;
			switch (shieldCopState)
			{
			case ShieldCopState.IDLE:
				navmeshMovement.MoveToPlayer(5f, useOffMeshLinks: true);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetMovementSpeed(7f);
				break;
			case ShieldCopState.MOVETOCIRCLE:
				if (!canPlayEngageSound && Random.value > 0.3f && !CheckIfInRange(15f))
				{
					canPlayEngageSound = true;
				}
				navmeshMovement.MoveToPlayer(4f, useOffMeshLinks: true);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetMovementSpeed(7f);
				break;
			case ShieldCopState.ROTATEAROUND:
				navmeshMovement.CirclePlayer(1f, 4f, 7f, 0.5f, 20f, 40f);
				navmeshMovement.SetRotationSpeed(5f);
				navmeshMovement.SetMovementSpeed(3f);
				break;
			case ShieldCopState.ATTACK:
				animations.PlayAnim((Random.value < 0.5f) ? attackMeleeAHash : attackMeleeBHash, restart: true);
				shieldWeapon.Attack();
				batonWeapon.Attack();
				enemyVoices.PlayAttack();
				navmeshMovement.MoveToPlayer(0f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.MoveToPlayer(1f);
				navmeshMovement.SetMovementSpeed(0f);
				navmeshMovement.StopObstacleAvoidance();
				break;
			case ShieldCopState.BLOCK:
				animations.PlayAnim(shieldBlockHash);
				navmeshMovement.MoveToStaticTarget(tf.position);
				navmeshMovement.LookAtPlayer(instant: true);
				navmeshMovement.SetRotationSpeed(5f);
				if (hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.SPRAY)
				{
					shieldSprayedVisual.SetActive(value: true);
				}
				else
				{
					player.GetHit(0, -positioningInfo.DiffToPlayer.normalized, KnockbackAbility.KnockbackType.SHIELDBASH);
				}
				hitBoxResponse.CancelHit();
				break;
			case ShieldCopState.HITRESPONSE:
				navmeshMovement.SetRotationSpeed(1f);
				if (HP <= 0 && !didDeathScream)
				{
					enemyVoices.PlayDeath();
					didDeathScream = true;
				}
				else
				{
					enemyVoices.PlayHurt();
				}
				shieldWeapon.CancelAttack();
				if (hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.SPRAY)
				{
					sprayedVisual.enabled = true;
					shieldSprayedVisual.SetActive(value: true);
				}
				break;
			case ShieldCopState.STUNNED:
				animations.PlayAnim(shieldOpenStunnedHash);
				break;
			case ShieldCopState.DIE:
				hitBoxResponse.SetStayDownOnDeathThisTime(value: true);
				navmeshMovement.StopObstacleAvoidance();
				navmeshMovement.StopMoving();
				ReportCrime();
				worldHandlerInstance.UnsubscribeToCombatArbiter(this);
				break;
			case ShieldCopState.RUNAWAY:
				navmeshMovement.MoveToStaticTarget(returnPos, useOffMeshLinks: true);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.SetMovementSpeed(Random.Range(4.5f, 5f));
				break;
			case ShieldCopState.RETURN:
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.MoveToStaticTarget(targetPoliceTube.GetSpawnTransform().position);
				tf.parent = targetPoliceTube.GetSpawnTransform();
				break;
			case ShieldCopState.KNOCKBACK:
			case ShieldCopState.KNOCKDOWN:
			case ShieldCopState.GETUP:
				break;
			}
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (shieldCopState)
			{
			case ShieldCopState.IDLE:
				MovementAnimation();
				if (!DoKnockBackIfNecessary())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case ShieldCopState.MOVETOCIRCLE:
				MovementAnimation();
				if (canPlayEngageSound && CheckIfInRange(15f))
				{
					enemyVoices.PlayEngage();
					canPlayEngageSound = false;
				}
				if (!DoKnockBackIfNecessary() && !AttackIfInRangeAndAble())
				{
					CirclePlayerIfInRange();
				}
				break;
			case ShieldCopState.ROTATEAROUND:
				MovementAnimation();
				if (!DoKnockBackIfNecessary() && !AttackIfInRangeAndAble())
				{
					MoveToCircleIfTooFar();
				}
				break;
			case ShieldCopState.ATTACK:
				if (shieldWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK || batonWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK)
				{
					navmeshMovement.SetMovementSpeed((positioningInfo.DistToPlayer < 1f) ? positioningInfo.DistToPlayer : 5f);
					if (CheckIfInRange(4f))
					{
						navmeshMovement.MoveToStaticTarget(tf.position + tf.forward * 5f);
						navmeshMovement.LookAtCurrentDir(instant: false);
					}
				}
				else if (batonWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER || stateTime > 3f)
				{
					navmeshMovement.StartObstacleAvoidance();
					navmeshMovement.StopMoving();
					navmeshMovement.SetMovementSpeed(0f);
					if (stateTime > 3f)
					{
						if (!shieldWeapon.HitLanded && !batonWeapon.HitLanded && Vector3.Dot(tf.forward, positioningInfo.DirToPlayer) < 0.2f)
						{
							worldHandlerInstance.AddCombatBlocker(3f);
							animations.PlayAnim(lookAroundHash);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
						else
						{
							animations.PlayAnim(beatShieldHash);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
					}
				}
				if (shieldWeapon.State == EnemyMeleeWeapon.AttackState.PRE_ATTACK || (shieldWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER && batonWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER))
				{
					if (DoKnockBackIfNecessary())
					{
						break;
					}
				}
				else
				{
					hitBoxResponse.CancelHit();
				}
				if (shieldWeapon.State == EnemyMeleeWeapon.AttackState.NONE && batonWeapon.State == EnemyMeleeWeapon.AttackState.NONE)
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case ShieldCopState.BLOCK:
				hitBoxResponse.CancelHit();
				if (stateTime > 0.75f)
				{
					enemyVoices.PlayBlock();
					MoveToCircleOrCircleAround();
				}
				break;
			case ShieldCopState.HITRESPONSE:
				if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN && positioningInfo.DistToPlayer < 4f && positioningInfo.DiffToPlayer.y < 2f && !player.IsGrounded())
				{
					graffitiSpotDamager.SetGraffitiSpotsActive(active: true);
				}
				else
				{
					graffitiSpotDamager.SetGraffitiSpotsActive(active: false);
				}
				if (HP == 0 && !didDeathPause && stateTime > 0.06f && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.PLAYER_BOOST && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.PLAYER_SPECIALAIR && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.EXPLOSION)
				{
					player.hitBounceAbility.Trigger(tf.position, 0.5f);
					DoHitPause(player.hitBounceAbility.hitpauseDuration + 0.1f, enemyOnly: true);
					didDeathPause = true;
				}
				if (HP <= 0 && looseWeapon == null && (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKBACK || hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN))
				{
					looseWeapon = Object.Instantiate(batonWeapon.VisualRoot);
					batonWeapon.Equip(equip: false);
					looseShield = Object.Instantiate(shieldWeapon.VisualRoot);
					shieldWeapon.Equip(equip: false);
					looseWeapon.transform.position = batonWeapon.VisualRoot.transform.position;
					looseWeapon.transform.rotation = batonWeapon.VisualRoot.transform.rotation;
					looseWeapon.AddComponent<BoxCollider>();
					looseWeapon.AddComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
					looseWeapon.AddComponent<OneOffVisualEffect>().duration = 2f;
					looseWeapon.layer = 21;
					looseShield.transform.position = shieldWeapon.VisualRoot.transform.position;
					looseShield.transform.rotation = shieldWeapon.VisualRoot.transform.rotation;
					looseShield.AddComponent<BoxCollider>();
					looseShield.AddComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
					looseShield.AddComponent<OneOffVisualEffect>().duration = 2f;
					looseShield.layer = 21;
				}
				if (HP <= 0)
				{
					if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKBACK && hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
					{
						break;
					}
					if (targetPoliceTube == null)
					{
						targetPoliceTube = WantedManager.instance.GetNearestPoliceTube(tf.position, navmeshMovement.GetAreaMask() | (1 << NavMesh.GetAreaFromName("Jump")));
					}
					if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.LYINGDOWN && targetPoliceTube == null)
					{
						SetState(ShieldCopState.DIE);
					}
					else if (targetPoliceTube != null)
					{
						returnPos = targetPoliceTube.ClaimForReturn(tf.position);
						if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
						{
							SetState(ShieldCopState.RUNAWAY);
						}
					}
				}
				else if (!DoKnockBackIfNecessary() && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
				{
					if (attacksReceivedWhileStunned < 3)
					{
						stunTimer = 2f;
						SetState(ShieldCopState.STUNNED);
					}
					else
					{
						stunTimer = 0f;
						attacksReceivedWhileStunned = 0;
						SetState(ShieldCopState.IDLE);
					}
				}
				break;
			case ShieldCopState.STUNNED:
				stunTimer -= Core.dt;
				if (!DoKnockBackIfNecessary() && stunTimer <= 0f)
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case ShieldCopState.DIE:
				if (!WorldHandler.instance.IsReallyInScreen(tf.position) && positioningInfo.SqrDistToPlayer > 100f)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case ShieldCopState.RUNAWAY:
				MovementAnimationRunAway();
				if (WorldHandler.instance.IsReallyInScreen(tf.position))
				{
					stateTime = 0f;
				}
				if (Vector3.Distance(returnPos, tf.position) < 10f && targetPoliceTube.State != PoliceTubeSpawner.TubeState.UPTORETURN)
				{
					targetPoliceTube.ComeUpForReturn();
				}
				if (!DoKnockBackIfNecessary())
				{
					if (Vector3.Distance(returnPos, tf.position) < 0.3f && targetPoliceTube.State == PoliceTubeSpawner.TubeState.UPTORETURN && targetPoliceTube.TubeStateTime > 1.5f)
					{
						SetState(ShieldCopState.RETURN);
					}
					else if (stateTime > 5f || ((navmeshMovement.GetLocalMovementVelocity().sqrMagnitude == 0f || positioningInfo.DistToPlayer > 10f) && stateTime > 0.5f))
					{
						targetPoliceTube.UnclaimForReturn();
						SetBasicState(BasicEnemyState.UNACTIVE);
					}
				}
				break;
			case ShieldCopState.RETURN:
				MovementAnimationRunAway();
				if ((targetPoliceTube.GetSpawnTransform().position - tf.position).sqrMagnitude < 0.1f && navmeshMovement.Activated)
				{
					tf.position = targetPoliceTube.GetSpawnTransform().position;
					navmeshMovement.Deactivate();
					targetPoliceTube.ReadyToGoDown();
				}
				if (targetPoliceTube.State != PoliceTubeSpawner.TubeState.UPTORETURN && targetPoliceTube.TubeStateTime > 2f)
				{
					tf.parent = null;
					targetPoliceTube.UnclaimForReturn();
					ReportCrime();
					worldHandlerInstance.UnsubscribeToCombatArbiter(this);
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case ShieldCopState.KNOCKBACK:
			case ShieldCopState.KNOCKDOWN:
			case ShieldCopState.GETUP:
				break;
			}
		}

		private void ExitState(ShieldCopState targetState)
		{
			switch (shieldCopState)
			{
			case ShieldCopState.ATTACK:
				navmeshMovement.StartObstacleAvoidance();
				shieldWeapon.CancelAttack();
				batonWeapon.CancelAttack();
				break;
			case ShieldCopState.HITRESPONSE:
				if (navmeshMovement.Agent.isActiveAndEnabled && !navmeshMovement.ValidateCanReachPlayerFromNavmesh())
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case ShieldCopState.DIE:
				navmeshMovement.StartObstacleAvoidance();
				break;
			case ShieldCopState.IDLE:
			case ShieldCopState.MOVETOCIRCLE:
			case ShieldCopState.ROTATEAROUND:
			case ShieldCopState.BLOCK:
			case ShieldCopState.KNOCKBACK:
			case ShieldCopState.KNOCKDOWN:
			case ShieldCopState.GETUP:
			case ShieldCopState.STUNNED:
				break;
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			navmeshMovement.Deactivate();
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.IsOnNavMeshLink())
			{
				if (navmeshMovement.IsOnEscalator())
				{
					animations.PlayAnim(moveRunHash, restart: false, Random.value);
				}
				else if (navmeshMovement.IsDrop())
				{
					if (navmeshMovement.Landed)
					{
						animations.PlayAnim(jumpDownLandHash, restart: false);
					}
					else
					{
						animations.PlayAnim(jumpDownHash, restart: false);
					}
				}
				else
				{
					animations.PlayAnim(vaultFenceHash, restart: false);
				}
			}
			else if (navmeshMovement.GetVelocity().sqrMagnitude > 0f)
			{
				animations.PlayAnim(moveShuffleHash);
				Vector3 localMovementVelocity = navmeshMovement.GetLocalMovementVelocity();
				animations.SetFloatParameter(moveDirectionXHash, localMovementVelocity.x);
				animations.SetFloatParameter(moveDirectionYHash, localMovementVelocity.z);
			}
			else
			{
				animations.PlayAnim(idleAgroHash);
			}
		}

		private void MovementAnimationRunAway()
		{
			if (navmeshMovement.IsOnNavMeshLink())
			{
				if (navmeshMovement.IsOnEscalator())
				{
					animations.PlayAnim(selectedRunAwayHash, restart: false, Random.value);
				}
				else if (navmeshMovement.IsDrop())
				{
					if (navmeshMovement.Landed)
					{
						animations.PlayAnim(jumpDownLandHash, restart: false);
					}
					else
					{
						animations.PlayAnim(jumpDownHash, restart: false);
					}
				}
				else
				{
					animations.PlayAnim(vaultFenceHash, restart: false);
				}
			}
			else if (navmeshMovement.GetVelocity().sqrMagnitude > 0f)
			{
				Vector3 localMovementVelocity = navmeshMovement.GetLocalMovementVelocity();
				if (navmeshMovement.GetVelocity().sqrMagnitude > 4f && localMovementVelocity.z > 0f)
				{
					animations.PlayAnim(selectedRunAwayHash);
				}
				else
				{
					animations.PlayAnim(idleRunAwayHash);
				}
				animations.SetFloatParameter(moveDirectionXHash, localMovementVelocity.x);
				animations.SetFloatParameter(moveDirectionYHash, localMovementVelocity.z);
			}
			else
			{
				animations.PlayAnim(idleRunAwayHash);
			}
		}

		public override bool IsDead()
		{
			return HP <= 0;
		}

		private bool MoveToCircleIfTooFar()
		{
			bool num = !CheckIfInRange(9f);
			if (num)
			{
				SetState(ShieldCopState.MOVETOCIRCLE);
			}
			return num;
		}

		private bool CirclePlayerIfInRange()
		{
			bool num = CheckIfInRange(7.5f);
			if (num)
			{
				SetState(ShieldCopState.ROTATEAROUND);
			}
			return num;
		}

		private bool MoveToCircleOrCircleAround()
		{
			bool flag = false;
			if (CheckIfInRange(7.55f))
			{
				SetState(ShieldCopState.ROTATEAROUND);
				return true;
			}
			SetState(ShieldCopState.MOVETOCIRCLE);
			return true;
		}

		private bool DoKnockBackIfNecessary()
		{
			bool flag = hitBoxResponse.HitToRespondTo;
			if (flag)
			{
				if (hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.PLAYER_BOOST || hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.PLAYER_SPECIALAIR || hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.EXPLOSION || stunTimer > 0f || (shieldWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER && !shieldWeapon.HitLanded && batonWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER && !batonWeapon.HitLanded && Vector3.Dot(tf.forward, positioningInfo.DirToPlayer) < 0f))
				{
					hitBoxResponse.RespondToHit();
					SetState(ShieldCopState.HITRESPONSE);
					flag = true;
					attacksReceivedWhileStunned++;
					stunTimer = 2f;
				}
				else if (stunTimer <= 0f && (hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE || hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKBACK) && (hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.SPRAY || !shieldSprayedVisual.activeSelf))
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.ShieldBlock, hitImpactAudioSource, 0.05f);
					SetState(ShieldCopState.BLOCK);
				}
			}
			return flag;
		}

		private bool AttackIfInRangeAndAble()
		{
			bool flag = shieldWeapon.CanAttack && shieldWeapon.IsInRange();
			if (flag)
			{
				RequestAttack(3f);
				if (receivedAttackPermission)
				{
					SetState(ShieldCopState.ATTACK);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool CheckIfInRange(float range)
		{
			return positioningInfo.SqrDistToPlayer < range * range;
		}
	}
}
