using UnityEngine;
using UnityEngine.AI;

namespace Reptile
{
	public class SniperCop : BasicEnemy
	{
		public enum SniperCopState
		{
			IDLE = 0,
			MOVECLOSER = 1,
			BACKOFF = 2,
			AIMING = 3,
			ATTACK = 4,
			RELOAD = 5,
			HITRESPONSE = 6,
			DIE = 7,
			RUNAWAY = 8,
			RETURN = 9
		}

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyHitResponse hitBoxResponse;

		[SerializeField]
		private EnemyLockOnAiming aiming;

		[SerializeField]
		private EnemyFirearm firearm;

		[SerializeField]
		private EnemyTargets targets;

		[SerializeField]
		private EnemyVoices enemyVoices;

		[SerializeField]
		private Transform aimReferenceTransform;

		[SerializeField]
		private LineRenderer line;

		[SerializeField]
		private Transform laserPointerStart;

		[SerializeField]
		private LayerMask targetVisualLayerMask;

		[SerializeField]
		private Renderer sprayedVisual;

		[SerializeField]
		private EnemyGraffitiSpotDamager graffitiSpotDamager;

		[SerializeField]
		private UpdateGraffitiOnOtherJuggledEnemies graffitiUpdater;

		private GameObject lineObject;

		private SniperCopState sniperCopState;

		private SniperCopState lastState;

		private float stateTime;

		private bool lockedOn;

		private bool aboutToFire;

		private bool didDeathPause;

		private bool didDeathScream;

		private Material normalGraffitiMaterial;

		private int idleAgroHash = Animator.StringToHash("idleAgro");

		private int idleRunAwayHash = Animator.StringToHash("idleRunAway");

		private int moveShuffleHash = Animator.StringToHash("moveShuffle");

		private int moveRunHash = Animator.StringToHash("moveRun");

		private int runAwayHash = Animator.StringToHash("runAway");

		private int runAwayBHash = Animator.StringToHash("runAwayB");

		private int gunLoweredHash = Animator.StringToHash("gunLowered");

		private int gunAimHash = Animator.StringToHash("gunAim");

		private int gunShootHash = Animator.StringToHash("gunShoot");

		private int gunReloadHash = Animator.StringToHash("gunReload");

		private int lieDownHash = Animator.StringToHash("lieDown");

		private int vaultFenceHash = Animator.StringToHash("vaultFence");

		private int jumpDownHash = Animator.StringToHash("jumpDown");

		private int jumpDownLandHash = Animator.StringToHash("jumpDownLand");

		private int moveDirectionXHash = Animator.StringToHash("moveDirectionX");

		private int moveDirectionYHash = Animator.StringToHash("moveDirectionY");

		private int aimAngleHash = Animator.StringToHash("aimAngle");

		private int selectedRunAwayHash;

		private Vector3 returnPos;

		private bool canPlayEngageVO = true;

		private PoliceTubeSpawner targetPoliceTube;

		private GameObject looseWeapon;

		private float targetColorTimer;

		protected override void Setup()
		{
			base.Setup();
			animations.Init(this);
			hitBoxResponse.Init(this, player, positioningInfo, navmeshMovement, animations);
			navmeshMovement.Init(this, positioningInfo, player);
			firearm.Init(this, positioningInfo);
			aiming.Init(this, player, positioningInfo, firearm);
			targets.Init();
			lineObject = line.gameObject;
			enemyVoices.Init();
			SetLine(set: false);
			targets.SetOffTargetVisuals();
			graffitiSpotDamager.Init(this);
			normalGraffitiMaterial = sprayedVisual.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			graffitiUpdater.Init(this);
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			hitBoxResponse.Deactivate();
			LowerGun();
			sprayedVisual.enabled = false;
			sprayedVisual.GetComponent<SkinnedMeshRenderer>().sharedMaterial = normalGraffitiMaterial;
			graffitiSpotDamager.ResetGraffitiSpots();
			graffitiSpotDamager.SetGraffitiSpotsActive(active: false);
		}

		protected override void UpdateSpawning()
		{
			base.UpdateSpawning();
			if (basicStateTime > 0f)
			{
				SetBasicState(BasicEnemyState.ACTIVE);
			}
		}

		protected override void EnteringActiveState()
		{
			base.EnteringActiveState();
			navmeshMovement.Activate();
			hitBoxResponse.Activate();
			HP = fullHP;
			selectedRunAwayHash = ((Random.value < 0.6f) ? runAwayHash : runAwayBHash);
			looseWeapon = null;
			firearm.Equip(equip: true);
			SetLine(set: false);
			targets.SetOffTargetVisuals();
			SetState(SniperCopState.IDLE);
			didDeathPause = false;
			didDeathScream = false;
			canPlayEngageVO = true;
		}

		private void SetState(SniperCopState targetState)
		{
			lastState = sniperCopState;
			ExitState(sniperCopState);
			sniperCopState = targetState;
			stateTime = 0f;
			switch (sniperCopState)
			{
			case SniperCopState.IDLE:
				ReadyGun();
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.MOVECLOSER:
				navmeshMovement.MoveToPlayer(5f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetMovementSpeed(5f);
				ReadyGun();
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.BACKOFF:
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetMovementSpeed(3f);
				ReadyGun();
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.AIMING:
				animations.PlayAnim(idleAgroHash);
				navmeshMovement.MoveToStaticTarget(tf.position);
				aiming.ResetAiming();
				AimGun();
				targets.SetOnTargetVisuals();
				if (canPlayEngageVO)
				{
					enemyVoices.PlayEngage();
					canPlayEngageVO = false;
				}
				else if (Random.value > 0.8f)
				{
					canPlayEngageVO = true;
				}
				break;
			case SniperCopState.ATTACK:
				animations.PlayAnim(idleAgroHash);
				ShootGun();
				navmeshMovement.MoveToStaticTarget(tf.position);
				firearm.Shoot(firearm.Muzzle.position, (aiming.GetTargetPos() + CalculateAimOffset() - firearm.Muzzle.position).normalized, 0f);
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.RELOAD:
				animations.PlayAnim(idleAgroHash);
				ReloadGun();
				navmeshMovement.BackOffIfPossible(20f, 5f);
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.HITRESPONSE:
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
				targets.SetOffTargetVisuals();
				if (hitBoxResponse.IncomingHitType == EnemyHitResponse.HitType.SPRAY)
				{
					sprayedVisual.enabled = true;
				}
				break;
			case SniperCopState.DIE:
				hitBoxResponse.SetStayDownOnDeathThisTime(value: true);
				navmeshMovement.StopObstacleAvoidance();
				navmeshMovement.StopMoving();
				ReportCrime();
				worldHandlerInstance.UnsubscribeToCombatArbiter(this);
				break;
			case SniperCopState.RUNAWAY:
				navmeshMovement.MoveToStaticTarget(returnPos, useOffMeshLinks: true);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetMovementSpeed(Random.Range(6f, 6.5f));
				break;
			case SniperCopState.RETURN:
				navmeshMovement.MoveToStaticTarget(targetPoliceTube.GetSpawnTransform().position);
				tf.parent = targetPoliceTube.GetSpawnTransform();
				break;
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			UpdateState();
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (sniperCopState)
			{
			case SniperCopState.IDLE:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGun();
				}
				if (!DoKnockBackIfHit() && !BackOfOrDefendIfNecessary() && !AttackIfInRangeAndAble())
				{
					SetState(SniperCopState.MOVECLOSER);
				}
				break;
			case SniperCopState.MOVECLOSER:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGun();
				}
				if (!DoKnockBackIfHit() && !AttackIfInRangeAndAble() && stateTime > 3f)
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.BACKOFF:
				MovementAnimation();
				if (navmeshMovement.IsOnNavMeshLink())
				{
					LowerGun();
				}
				else
				{
					ReadyGun();
				}
				if (positioningInfo.SqrDistToPlayer <= 25f)
				{
					navmeshMovement.SetMovementSpeed(6f);
					navmeshMovement.LookAtMoveDir(instant: false);
				}
				else
				{
					navmeshMovement.SetMovementSpeed(3f);
					navmeshMovement.LookAtPlayer(instant: false);
				}
				if (!DoKnockBackIfHit() && (navmeshMovement.CurrentMovementState != EnemyNavmeshMovement.MovementState.STATIONARY || !BackOfOrDefendIfNecessary()) && positioningInfo.DistToPlayer >= 10f && !AttackIfInRangeAndAble())
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.AIMING:
			{
				aiming.AimingUpdate();
				navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: true);
				Vector3 targetPos = aiming.GetTargetPos();
				Vector3 position = firearm.Muzzle.position;
				Vector3 normalized = (targetPos - firearm.Muzzle.position).normalized;
				if (Physics.Raycast(position, normalized, out var hitInfo, 200f, targetVisualLayerMask))
				{
					targets.SetOnTargetVisuals();
					targets.targetVisualTf.position = hitInfo.point;
					Vector3 forward = base.transform.position - hitInfo.point;
					forward.y = 0f;
					targets.targetVisualTf.rotation = Quaternion.LookRotation(forward);
				}
				else
				{
					targets.SetOffTargetVisuals();
				}
				SetLine(set: true);
				SetAimAngle();
				if (stateTime < 3.5f)
				{
					if (aiming.LockOn)
					{
						if (!lockedOn)
						{
							stateTime = 3.5f;
							targets.targetVisualTf.gameObject.GetComponent<Animator>().Play("targetVisualClose");
							lockedOn = true;
						}
					}
					else
					{
						if (lockedOn)
						{
							targets.targetVisualTf.gameObject.GetComponent<Animator>().Play("targetVisualOpen");
						}
						lockedOn = false;
					}
				}
				else if (!aboutToFire || aiming.LockOn != lockedOn)
				{
					if (aiming.LockOn)
					{
						targets.targetVisualTf.gameObject.GetComponent<Animator>().Play("targetVisualAboutToFire");
						lockedOn = true;
					}
					else
					{
						targets.targetVisualTf.gameObject.GetComponent<Animator>().Play("targetVisualAboutToFireOpen");
						lockedOn = false;
					}
					aboutToFire = true;
				}
				if (!DoKnockBackIfHit() && (stateTime > 5f || (stateTime > 2f && UnderDirectThreat())))
				{
					SetState(SniperCopState.ATTACK);
				}
				break;
			}
			case SniperCopState.ATTACK:
				if (!DoKnockBackIfHit() && stateTime > 0.5f && !ReloadIfNecessary() && !AttackIfInRangeAndAble())
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.RELOAD:
				if (!firearm.Reloading)
				{
					animations.PlayAnim(idleAgroHash);
				}
				MovementAnimation();
				navmeshMovement.SetMovementSpeed(3f);
				navmeshMovement.LookAtPlayer(instant: false);
				if (!DoKnockBackIfHit() && !firearm.Reloading && !BackOfOrDefendIfNecessary() && !AttackIfInRangeAndAble())
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.HITRESPONSE:
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
					looseWeapon = Object.Instantiate(firearm.VisualRoot);
					firearm.Equip(equip: false);
					looseWeapon.transform.position = firearm.VisualRoot.transform.position;
					looseWeapon.transform.rotation = firearm.VisualRoot.transform.rotation;
					looseWeapon.AddComponent<BoxCollider>();
					looseWeapon.AddComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
					looseWeapon.AddComponent<OneOffVisualEffect>().duration = 2f;
					looseWeapon.layer = 21;
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
						SetState(SniperCopState.DIE);
					}
					else if (targetPoliceTube != null)
					{
						returnPos = targetPoliceTube.ClaimForReturn(tf.position);
						if (hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
						{
							SetState(SniperCopState.RUNAWAY);
						}
					}
				}
				else if (!DoKnockBackIfHit() && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.DIE:
				if (!WorldHandler.instance.IsReallyInScreen(tf.position) && positioningInfo.SqrDistToPlayer > 100f)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case SniperCopState.RUNAWAY:
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
						SetState(SniperCopState.RETURN);
					}
					else if (stateTime > 5f || ((navmeshMovement.GetLocalMovementVelocity().sqrMagnitude == 0f || positioningInfo.DistToPlayer > 10f) && stateTime > 0.5f))
					{
						targetPoliceTube.UnclaimForReturn();
						SetBasicState(BasicEnemyState.UNACTIVE);
					}
				}
				break;
			case SniperCopState.RETURN:
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

		private void ExitState(SniperCopState targetState)
		{
			switch (sniperCopState)
			{
			case SniperCopState.AIMING:
				lockedOn = (aboutToFire = false);
				SetLine(set: false);
				break;
			case SniperCopState.HITRESPONSE:
				if (navmeshMovement.Agent.isActiveAndEnabled && !navmeshMovement.ValidateCanReachPlayerFromNavmesh())
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case SniperCopState.DIE:
				navmeshMovement.StartObstacleAvoidance();
				break;
			case SniperCopState.IDLE:
			case SniperCopState.MOVECLOSER:
			case SniperCopState.BACKOFF:
			case SniperCopState.ATTACK:
			case SniperCopState.RELOAD:
				break;
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			targets.SetOffTargetVisuals();
			navmeshMovement.Deactivate();
			if (targetPoliceTube != null)
			{
				targetPoliceTube.UnclaimForReturn();
			}
			hitBoxResponse.SetStayDownOnDeathThisTime(value: false);
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
				Vector3 localMovementVelocity = navmeshMovement.GetLocalMovementVelocity();
				if (navmeshMovement.GetVelocity().sqrMagnitude > 12f && localMovementVelocity.z > 0f)
				{
					animations.PlayAnim(moveRunHash);
				}
				else
				{
					animations.PlayAnim(moveShuffleHash);
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

		private void LowerGun()
		{
			animations.SetLayerActivation(1, value: false);
		}

		private void ReadyGun()
		{
			animations.SetLayerActivation(1, value: true);
			animations.PlayAnim(gunLoweredHash);
		}

		private void AimGun()
		{
			animations.SetLayerActivation(1, value: true);
			animations.PlayAnim(gunAimHash);
		}

		private void ShootGun()
		{
			animations.SetLayerActivation(1, value: true);
			animations.PlayAnim(gunShootHash);
		}

		private void ReloadGun()
		{
			animations.SetLayerActivation(1, value: true);
			animations.PlayAnim(gunReloadHash);
		}

		private void SetLine(bool set)
		{
			if (set)
			{
				if (aiming.LockOn)
				{
					targetColorTimer = (targetColorTimer + 10f * Core.dt) % 1f;
					bool flag = targetColorTimer > 0.5f;
					if (lineObject.activeSelf != flag)
					{
						lineObject.SetActive(flag);
					}
				}
				else if (!lineObject.activeSelf)
				{
					lineObject.SetActive(value: true);
				}
				if (lineObject.activeSelf)
				{
					line.positionCount = 3;
					Vector3 vector = targets.targetVisualTf.position + CalculateAimOffset() - laserPointerStart.position;
					float magnitude = vector.magnitude;
					line.SetPosition(0, laserPointerStart.position);
					line.SetPosition(1, laserPointerStart.position + vector.normalized * magnitude * 0.75f);
					line.SetPosition(2, laserPointerStart.position + vector.normalized * magnitude);
				}
			}
			else if (lineObject.activeSelf)
			{
				lineObject.SetActive(value: false);
			}
		}

		private Vector3 CalculateAimOffset()
		{
			Vector3 result = Vector3.zero;
			if (aiming.LockOn || positioningInfo.SqrDistToPlayer < 64f)
			{
				result = Vector3.up * 1.2f;
			}
			return result;
		}

		private void SetAimAngle()
		{
			Vector3 normalized = (aiming.GetTargetPos() + CalculateAimOffset() - aimReferenceTransform.position - aimReferenceTransform.right * 0.5f + aimReferenceTransform.right * 0.5f).normalized;
			float num = Vector3.Angle(new Vector3(normalized.x, 0f, normalized.z).normalized, normalized);
			float num2 = ((normalized.y > 0f) ? 1 : (-1));
			animations.SetFloatParameter(aimAngleHash, num2 * (num / 45f));
		}

		public bool LineOfSight(Transform gunTf, Vector3 dir, float length)
		{
			int layerMask = 2106897;
			if (Physics.Raycast(gunTf.position, dir, out var hitInfo, length, layerMask))
			{
				return hitInfo.collider.gameObject.layer == 9;
			}
			return false;
		}

		private bool DoKnockBackIfHit()
		{
			bool flag = hitBoxResponse.RespondToHit();
			if (flag)
			{
				SetState(SniperCopState.HITRESPONSE);
				flag = true;
			}
			return flag;
		}

		private bool BackOfOrDefendIfNecessary()
		{
			if (positioningInfo.SqrDistToPlayer < 225f)
			{
				if (firearm.CanShoot)
				{
					float num = positioningInfo.SqrDistToPlayer / 225f;
					if (Random.value > num)
					{
						if (navmeshMovement.BackOffIfPossible(20f, 5f))
						{
							SetState(SniperCopState.BACKOFF);
							return true;
						}
						SetState(SniperCopState.AIMING);
						return true;
					}
				}
				else if (navmeshMovement.BackOffIfPossible(20f, 10f))
				{
					SetState(SniperCopState.BACKOFF);
					return true;
				}
			}
			return false;
		}

		private bool AttackIfInRangeAndAble()
		{
			bool flag = firearm.IsInRangeFlat() && firearm.CanShoot;
			if (flag)
			{
				if (LineOfSight(aimReferenceTransform, positioningInfo.DirToPlayer, positioningInfo.DistToPlayer))
				{
					RequestAttack(6f);
					if (receivedAttackPermission)
					{
						SetState(SniperCopState.AIMING);
						flag = true;
					}
					else
					{
						flag = false;
					}
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
				SetState(SniperCopState.RELOAD);
			}
			return reloading;
		}

		private bool UnderDirectThreat()
		{
			if (positioningInfo.SqrDistToPlayer < 225f)
			{
				return WorldHandler.instance.IsReallyInScreen(tf.position);
			}
			return false;
		}

		private bool CheckIfInRange(float range)
		{
			return positioningInfo.SqrDistToPlayer < range * range;
		}
	}
}
