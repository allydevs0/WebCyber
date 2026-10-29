using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class BasicCop : BasicEnemy
	{
		public enum BasicCopState
		{
			IDLE = 0,
			MOVETOCIRCLE = 1,
			ROTATEAROUND = 2,
			ATTACK = 3,
			HITRESPONSE = 4,
			SWAPWEAPON = 5,
			RELOAD = 6,
			DIE = 7,
			RUNAWAY = 8,
			RETURN = 9
		}

		public enum AttackMode
		{
			MELEE = 0,
			RANGED = 1,
			NONE = 2
		}

		public enum GunState
		{
			LOWERED = 0,
			READIED = 1,
			AIMED = 2
		}

		[Serializable]
		public struct MaterialSet
		{
			public Material normalMat;

			public Material corruptMat;
		}

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyHitResponse hitBoxResponse;

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private EnemyFirearm firearm;

		[SerializeField]
		private EnemyMeleeWeapon meleeWeapon;

		[SerializeField]
		private Transform aimReferenceTransform;

		[SerializeField]
		private EnemyVoices enemyVoices;

		[SerializeField]
		private float changeToDrawGunInEncounter = 0.4f;

		[SerializeField]
		private AnimationCurve changeToDrawGunAtWantedLevel;

		[SerializeField]
		private SkinnedMeshRenderer mainRenderer;

		[SerializeField]
		private AttackMode requiredAttackMode;

		[SerializeField]
		private Renderer sprayedVisual;

		[SerializeField]
		private EnemyGraffitiSpotDamager graffitiSpotDamager;

		[SerializeField]
		private UpdateGraffitiOnOtherJuggledEnemies graffitiUpdater;

		[SerializeField]
		private MaterialSet[] materialSets;

		public List<BasicCopState> stateHistory = new List<BasicCopState>();

		private BasicCopState basicCopState;

		private BasicCopState lastBasicCopState;

		private float stateTime;

		private AttackMode attackMode = AttackMode.NONE;

		private GunState gunState;

		private bool didDeathPause;

		private bool didDeathScream;

		private Material normalGraffitiMaterial;

		private int idleTubeHash = Animator.StringToHash("idleTube");

		private int idleAgroHash = Animator.StringToHash("idleAgro");

		private int idleRunAwayHash = Animator.StringToHash("idleRunAway");

		private int moveShuffleHash = Animator.StringToHash("moveShuffle");

		private int moveRunHash = Animator.StringToHash("moveRun");

		private int runAwayHash = Animator.StringToHash("runAway");

		private int runAwayBHash = Animator.StringToHash("runAwayB");

		private int grabBatonHash = Animator.StringToHash("grabBaton");

		private int grabGunHash = Animator.StringToHash("grabGun");

		private int attackMeleeAHash = Animator.StringToHash("attackMeleeA");

		private int attackMeleeBHash = Animator.StringToHash("attackMeleeB");

		private int gunAimHash = Animator.StringToHash("gunAim");

		private int gunShootHash = Animator.StringToHash("gunShoot");

		private int gunLowerHash = Animator.StringToHash("gunLower");

		private int gunLoweredHash = Animator.StringToHash("gunLowered");

		private int gunReloadHash = Animator.StringToHash("gunReload");

		private int lieDownHash = Animator.StringToHash("lieDown");

		private int vaultFenceHash = Animator.StringToHash("vaultFence");

		private int jumpDownHash = Animator.StringToHash("jumpDown");

		private int jumpDownLandHash = Animator.StringToHash("jumpDownLand");

		private int moveDirectionXHash = Animator.StringToHash("moveDirectionX");

		private int moveDirectionYHash = Animator.StringToHash("moveDirectionY");

		private int aimAngleHash = Animator.StringToHash("aimAngle");

		private int selectedRunAwayHash;

		private int selectedMatSet = -1;

		private int meleeAttack;

		private float targetRangeOffset;

		private int attackDone;

		private Vector3 returnPos;

		private PoliceTubeSpawner targetPoliceTube;

		private GameObject looseWeapon;

		private float lastWantedCheck;

		protected override void Setup()
		{
			base.Setup();
			graffitiSpotDamager.Init(this);
			enemyVoices.Init();
			animations.Init(this);
			hitBoxResponse.Init(this, player, positioningInfo, navmeshMovement, animations);
			navmeshMovement.Init(this, positioningInfo, player);
			aiming.Init(this, player);
			firearm.Init(this, positioningInfo);
			meleeWeapon.Init(this, positioningInfo);
			normalGraffitiMaterial = sprayedVisual.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			graffitiUpdater.Init(this);
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			animations.PlayAnim(idleTubeHash);
			selectedMatSet = UnityEngine.Random.Range(0, materialSets.Length);
			mainRenderer.sharedMaterial = materialSets[selectedMatSet].normalMat;
			meleeWeapon.Equip(equip: false);
			firearm.Equip(equip: false);
			hitBoxResponse.Deactivate();
			sprayedVisual.enabled = false;
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
			gunState = GunState.LOWERED;
			HP = fullHP;
			selectedRunAwayHash = ((UnityEngine.Random.value < 0.6f) ? runAwayHash : runAwayBHash);
			lastWantedCheck = 0f;
			looseWeapon = null;
			attackMode = AttackMode.NONE;
			WeaponSwapOnWanted(justCheck: true);
			basicCopState = BasicCopState.IDLE;
			SetState(BasicCopState.SWAPWEAPON);
			didDeathPause = false;
			didDeathScream = false;
			enemyVoices.PlaySpawn();
		}

		private void SetState(BasicCopState targetState)
		{
			stateHistory.Add(targetState);
			lastBasicCopState = basicCopState;
			ExitState(basicCopState);
			basicCopState = targetState;
			stateTime = 0f;
			switch (basicCopState)
			{
			case BasicCopState.IDLE:
				navmeshMovement.MoveToPlayer(5f, useOffMeshLinks: true);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				targetRangeOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
				ReadyGunIfRanged();
				break;
			case BasicCopState.MOVETOCIRCLE:
				navmeshMovement.MoveToPlayer(4f, useOffMeshLinks: true);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.SetMovementSpeed(UnityEngine.Random.Range(5.5f, 6f));
				targetRangeOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
				ReadyGunIfRanged();
				break;
			case BasicCopState.ROTATEAROUND:
				navmeshMovement.CirclePlayer(1f, 4f, 4f, 0.75f, 20f, 40f);
				navmeshMovement.SetMovementSpeed(UnityEngine.Random.Range(2f, 3f));
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(5f);
				targetRangeOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
				ReadyGunIfRanged();
				break;
			case BasicCopState.ATTACK:
				attackDone = 0;
				if (attackMode == AttackMode.RANGED)
				{
					animations.PlayAnim(idleAgroHash);
					navmeshMovement.MoveToStaticTarget(tf.position);
					AimGunIfRanged();
					navmeshMovement.SetRotationSpeed(10f);
				}
				else
				{
					navmeshMovement.SetRotationSpeed(10f);
					meleeAttack = UnityEngine.Random.Range(0, 2);
					navmeshMovement.MoveToPlayer(1f);
					navmeshMovement.LookAtPlayer(instant: false);
					navmeshMovement.SetMovementSpeed(UnityEngine.Random.Range(5.5f, 6f));
				}
				break;
			case BasicCopState.HITRESPONSE:
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
				LowerGun();
				if (hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.SPRAY)
				{
					sprayedVisual.enabled = true;
				}
				break;
			case BasicCopState.SWAPWEAPON:
				navmeshMovement.SetRotationSpeed(1f);
				LowerGun();
				if (requiredAttackMode == AttackMode.RANGED)
				{
					animations.PlayAnim(grabGunHash);
				}
				else if (requiredAttackMode == AttackMode.MELEE)
				{
					animations.PlayAnim(grabBatonHash);
				}
				navmeshMovement.MoveToStaticTarget(tf.position);
				navmeshMovement.LookAtCurrentDir(instant: false);
				break;
			case BasicCopState.RELOAD:
				navmeshMovement.SetRotationSpeed(1f);
				AimGunIfRanged();
				animations.PlayAnim(gunReloadHash);
				navmeshMovement.MoveToStaticTarget(tf.position);
				navmeshMovement.LookAtCurrentDir(instant: false);
				break;
			case BasicCopState.DIE:
				hitBoxResponse.SetStayDownOnDeathThisTime(value: true);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.StopObstacleAvoidance();
				navmeshMovement.StopMoving();
				ReportCrime();
				worldHandlerInstance.UnsubscribeToCombatArbiter(this);
				break;
			case BasicCopState.RUNAWAY:
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.MoveToStaticTarget(returnPos, useOffMeshLinks: true);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetMovementSpeed(UnityEngine.Random.Range(4.5f, 5f));
				break;
			case BasicCopState.RETURN:
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.MoveToStaticTarget(targetPoliceTube.GetSpawnTransform().position);
				tf.parent = targetPoliceTube.GetSpawnTransform();
				break;
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			WeaponSwapOnWanted(justCheck: false);
			UpdateState();
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (basicCopState)
			{
			case BasicCopState.IDLE:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGunIfRanged();
				}
				if (!DoKnockBackIfHit() && !SwapWeaponIfNecessary())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.MOVETOCIRCLE:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGunIfRanged();
				}
				if (!DoKnockBackIfHit() && !navmeshMovement.IsOnNavMeshLink() && !SwapWeaponIfNecessary() && !AttackIfInRangeAndAble())
				{
					CirclePlayerIfInRange();
				}
				break;
			case BasicCopState.ROTATEAROUND:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGunIfRanged();
				}
				if (!DoKnockBackIfHit() && !navmeshMovement.IsOnNavMeshLink() && !SwapWeaponIfNecessary() && !AttackIfInRangeAndAble())
				{
					MoveToCircleIfTooFar();
				}
				break;
			case BasicCopState.ATTACK:
				if (attackMode == AttackMode.RANGED)
				{
					navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: false);
					SetAimAngle();
					if (stateTime <= 1f)
					{
						navmeshMovement.SetRotationSpeed(Mathf.Lerp(10f, 100f, stateTime));
					}
					if (stateTime > 1f && firearm.CanShoot)
					{
						attackDone++;
						animations.PlayAnim(gunShootHash, restart: true);
						firearm.Shoot(aiming.GetTargetPos() - tf.position, aiming.TargetAbility);
					}
				}
				else
				{
					if (!CheckIfInRange(2f) && !meleeWeapon.Attacking)
					{
						navmeshMovement.MoveToPlayer(1f);
						navmeshMovement.LookAtPlayer(instant: false);
					}
					else
					{
						if (meleeWeapon.CanAttack)
						{
							attackDone++;
							enemyVoices.PlayAttack();
							meleeWeapon.Attack();
							animations.PlayAnim((meleeAttack == 0) ? attackMeleeAHash : attackMeleeBHash, restart: true);
							navmeshMovement.MoveToStaticTarget(tf.position + tf.forward);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
						if (meleeWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK || positioningInfo.SqrDistToPlayer < 0.25f)
						{
							navmeshMovement.MoveToStaticTarget(tf.position);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
					}
					if (!meleeWeapon.Attacking)
					{
						MovementAnimation();
					}
				}
				if (((attackMode == AttackMode.MELEE && meleeWeapon.HitLanded && meleeWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK) || !DoKnockBackIfHit()) && stateTime > 2f && !SwapWeaponIfNecessary() && !ReloadIfNecessary() && !AttackIfInRangeAndAble() && (!meleeWeapon.Attacking || attackMode == AttackMode.RANGED))
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.HITRESPONSE:
				if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN && positioningInfo.DistToPlayer < 4f && positioningInfo.DiffToPlayer.y < 2f && !player.IsGrounded())
				{
					graffitiSpotDamager.SetGraffitiSpotsActive(active: true);
				}
				else
				{
					graffitiSpotDamager.SetGraffitiSpotsActive(active: false);
				}
				if (HP == 0 && stateTime > 0.06f && !didDeathPause && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.PLAYER_BOOST && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.PLAYER_SPECIALAIR && hitBoxResponse.IncomingHitType != EnemyHitResponse.HitType.EXPLOSION)
				{
					player.hitBounceAbility.Trigger(tf.position, 0.5f);
					DoHitPause(player.hitBounceAbility.hitpauseDuration + 0.1f, enemyOnly: true);
					didDeathPause = true;
				}
				if (HP <= 0 && looseWeapon == null && (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKBACK || hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN))
				{
					if (meleeWeapon.Equipped)
					{
						looseWeapon = UnityEngine.Object.Instantiate(meleeWeapon.VisualRoot);
						meleeWeapon.Equip(equip: false);
					}
					if (firearm.Equipped)
					{
						looseWeapon = UnityEngine.Object.Instantiate(firearm.VisualRoot);
						firearm.Equip(equip: false);
					}
					if (looseWeapon != null)
					{
						looseWeapon.transform.position = meleeWeapon.VisualRoot.transform.position;
						looseWeapon.transform.rotation = meleeWeapon.VisualRoot.transform.rotation;
						looseWeapon.AddComponent<BoxCollider>();
						looseWeapon.AddComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
						looseWeapon.AddComponent<OneOffVisualEffect>().duration = 2f;
						looseWeapon.layer = 21;
					}
				}
				if (HP <= 0)
				{
					if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKBACK && hitBoxResponse.State == EnemyHitResponse.HitResponseState.KNOCKDOWN && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
					{
						break;
					}
					if (targetPoliceTube == null)
					{
						targetPoliceTube = WantedManager.instance.GetNearestPoliceTube(tf.position, navmeshMovement.GetAreaMask());
					}
					if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.LYINGDOWN && targetPoliceTube == null)
					{
						SetState(BasicCopState.DIE);
					}
					else if (targetPoliceTube != null)
					{
						returnPos = targetPoliceTube.ClaimForReturn(tf.position);
						if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
						{
							SetState(BasicCopState.RUNAWAY);
						}
					}
				}
				else if (((!(player == null) && player.ability == player.handplantAbility && player.handplantAbility.screwPoleMode) || !DoKnockBackIfHit()) && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE && !SwapWeaponIfNecessary())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.SWAPWEAPON:
				if (stateTime > 0.42f && attackMode != requiredAttackMode)
				{
					firearm.Equip(requiredAttackMode == AttackMode.RANGED);
					meleeWeapon.Equip(requiredAttackMode == AttackMode.MELEE);
					attackMode = requiredAttackMode;
					if (attackMode == AttackMode.MELEE)
					{
						mainRenderer.sharedMaterial = materialSets[selectedMatSet].normalMat;
						threat = 1;
					}
					else
					{
						mainRenderer.sharedMaterial = materialSets[selectedMatSet].corruptMat;
						threat = 4;
					}
				}
				if (stateTime > 1f)
				{
					AimGunIfRanged();
				}
				if (!DoKnockBackIfHit() && stateTime > 1.5f)
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.RELOAD:
				if (!DoKnockBackIfHit() && !firearm.Reloading && !AttackIfInRangeAndAble())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.DIE:
				if (!WorldHandler.instance.IsReallyInScreen(tf.position) && positioningInfo.SqrDistToPlayer > 100f)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case BasicCopState.RUNAWAY:
				MovementAnimationRunAway();
				if (WorldHandler.instance.IsReallyInScreen(tf.position))
				{
					stateTime = 0f;
				}
				if (Vector3.Distance(returnPos, tf.position) < 10f && targetPoliceTube.State != PoliceTubeSpawner.TubeState.UPTORETURN)
				{
					targetPoliceTube.ComeUpForReturn();
				}
				if (!DoKnockBackIfHit())
				{
					if (Vector3.Distance(returnPos, tf.position) < 0.3f && targetPoliceTube.State == PoliceTubeSpawner.TubeState.UPTORETURN && targetPoliceTube.TubeStateTime > 1.5f)
					{
						SetState(BasicCopState.RETURN);
					}
					else if (stateTime > 5f || ((navmeshMovement.GetLocalMovementVelocity().sqrMagnitude == 0f || positioningInfo.DistToPlayer > 10f) && stateTime > 0.5f))
					{
						targetPoliceTube.UnclaimForReturn();
						SetBasicState(BasicEnemyState.UNACTIVE);
					}
				}
				break;
			case BasicCopState.RETURN:
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
			}
		}

		private void ExitState(BasicCopState targetState)
		{
			switch (basicCopState)
			{
			case BasicCopState.ATTACK:
				meleeWeapon.CancelAttack();
				break;
			case BasicCopState.HITRESPONSE:
				if (navmeshMovement.Agent.isActiveAndEnabled && !navmeshMovement.ValidateCanReachPlayerFromNavmesh())
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case BasicCopState.DIE:
				navmeshMovement.StartObstacleAvoidance();
				break;
			case BasicCopState.IDLE:
			case BasicCopState.MOVETOCIRCLE:
			case BasicCopState.ROTATEAROUND:
			case BasicCopState.SWAPWEAPON:
			case BasicCopState.RELOAD:
				break;
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			navmeshMovement.Deactivate();
			if (targetPoliceTube != null)
			{
				targetPoliceTube.UnclaimForReturn();
			}
			hitBoxResponse.SetStayDownOnDeathThisTime(value: false);
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.IsMovingAlongNavMeshLink())
			{
				if (navmeshMovement.IsOnEscalator())
				{
					animations.PlayAnim(moveRunHash, restart: false, UnityEngine.Random.value);
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
				if (navmeshMovement.GetVelocity().sqrMagnitude > 12f && localMovementVelocity.z > 0f)
				{
					animations.PlayAnim(moveRunHash, restart: false, UnityEngine.Random.value);
				}
				else
				{
					animations.PlayAnim(moveShuffleHash, restart: false, UnityEngine.Random.value);
				}
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
			if (navmeshMovement.IsMovingAlongNavMeshLink())
			{
				if (navmeshMovement.IsOnEscalator())
				{
					animations.PlayAnim(selectedRunAwayHash, restart: false, UnityEngine.Random.value);
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
					animations.PlayAnim(selectedRunAwayHash, restart: false, UnityEngine.Random.value);
				}
				else
				{
					animations.PlayAnim(idleRunAwayHash, restart: false, UnityEngine.Random.value);
				}
				animations.SetFloatParameter(moveDirectionXHash, localMovementVelocity.x);
				animations.SetFloatParameter(moveDirectionYHash, localMovementVelocity.z);
			}
			else
			{
				animations.PlayAnim(idleRunAwayHash, restart: false, UnityEngine.Random.value);
			}
		}

		public override bool IsDead()
		{
			return HP <= 0;
		}

		private void LowerGun()
		{
			animations.SetLayerActivation(1, value: false);
			gunState = GunState.LOWERED;
		}

		private void ReadyGunIfRanged()
		{
			if (attackMode == AttackMode.RANGED)
			{
				animations.SetLayerActivation(1, value: true);
				if (gunState == GunState.LOWERED)
				{
					animations.PlayAnim(gunLoweredHash, restart: false);
				}
				if (gunState == GunState.AIMED)
				{
					animations.PlayAnim(gunLowerHash, restart: false);
				}
				gunState = GunState.READIED;
			}
			else
			{
				animations.SetLayerActivation(1, value: false);
			}
		}

		private void AimGunIfRanged()
		{
			if (attackMode == AttackMode.RANGED)
			{
				animations.SetLayerActivation(1, value: true);
				if (gunState != GunState.AIMED)
				{
					animations.PlayAnim(gunAimHash);
					gunState = GunState.AIMED;
				}
			}
			else
			{
				animations.SetLayerActivation(1, value: false);
			}
		}

		private void SetAimAngle()
		{
			if (attackMode == AttackMode.RANGED)
			{
				Vector3 normalized = (aiming.GetTargetPos() + Vector3.up * 1.2f - aimReferenceTransform.position - aimReferenceTransform.right * 0.5f + aimReferenceTransform.right * 0.5f).normalized;
				float num = Vector3.Angle(new Vector3(normalized.x, 0f, normalized.z).normalized, normalized);
				float num2 = ((normalized.y > 0f) ? 1 : (-1));
				animations.SetFloatParameter(aimAngleHash, num2 * (num / 45f));
			}
		}

		private void WeaponSwapOnWanted(bool justCheck)
		{
			if ((WantedManager.instance != null && WantedManager.instance.WantedStarProgress != lastWantedCheck) || justCheck)
			{
				if ((WorldHandler.instance.currentEncounter != null && UnityEngine.Random.value < changeToDrawGunInEncounter) || (WorldHandler.instance.currentEncounter == null && WantedManager.instance != null && UnityEngine.Random.value < changeToDrawGunAtWantedLevel.Evaluate(WantedManager.instance.WantedStarProgress) && Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars >= 4f))
				{
					requiredAttackMode = AttackMode.RANGED;
				}
				else if (attackMode == AttackMode.NONE)
				{
					requiredAttackMode = AttackMode.MELEE;
				}
			}
			if (WantedManager.instance != null)
			{
				lastWantedCheck = WantedManager.instance.WantedStarProgress;
			}
		}

		private bool MoveToCircleIfTooFar()
		{
			bool num = !CheckIfInRange(10f + targetRangeOffset);
			if (num)
			{
				SetState(BasicCopState.MOVETOCIRCLE);
			}
			return num;
		}

		private bool CirclePlayerIfInRange()
		{
			bool num = CheckIfInRange(7.5f + targetRangeOffset);
			if (num)
			{
				SetState(BasicCopState.ROTATEAROUND);
			}
			return num;
		}

		private bool MoveToCircleOrCircleAround()
		{
			bool flag = false;
			if (CheckIfInRange(7.5f + targetRangeOffset))
			{
				SetState(BasicCopState.ROTATEAROUND);
				return true;
			}
			SetState(BasicCopState.MOVETOCIRCLE);
			return true;
		}

		private bool DoKnockBackIfHit()
		{
			bool flag = hitBoxResponse.RespondToHit();
			if (flag)
			{
				SetState(BasicCopState.HITRESPONSE);
				flag = true;
			}
			return flag;
		}

		private bool AttackIfInRangeAndAble()
		{
			bool flag = (firearm.CanShoot && firearm.IsInRangeFlat() && stateTime > 2f) || (meleeWeapon.CanAttack && meleeWeapon.IsInRange());
			if (flag)
			{
				RequestAttack(2f);
				if (receivedAttackPermission)
				{
					SetState(BasicCopState.ATTACK);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool ReloadIfNecessary()
		{
			bool reloading = firearm.Reloading;
			if (reloading)
			{
				SetState(BasicCopState.RELOAD);
			}
			return reloading;
		}

		private bool SwapWeaponIfNecessary()
		{
			bool num = requiredAttackMode != attackMode;
			if (num)
			{
				SetState(BasicCopState.SWAPWEAPON);
			}
			return num;
		}

		private bool CheckIfInRange(float range)
		{
			return positioningInfo.SqrDistToPlayer < range * range;
		}
	}
}
