using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class PoliceCaptainSniper : BasicEnemy
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
			RELOCATE = 7,
			DIE = 8
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
		private PlayableDirector escapeSequenceFirstTime;

		[SerializeField]
		private PlayableDirector escapeSequence;

		[SerializeField]
		private PlayableDirector escapeSequenceLastTime;

		[SerializeField]
		private GameObject sniperPrefab;

		[SerializeField]
		private GameObject sprayedVisual;

		private GameObject lineObject;

		private SniperCopState sniperCopState;

		private SniperCopState lastState;

		private float stateTime;

		private bool lockedOn;

		private bool aboutToFire;

		private bool hasBeenSprayed;

		private Transform[] locationSequence;

		private int currentPosition;

		private int idleAgroHash = Animator.StringToHash("idleAgro");

		private int moveShuffleHash = Animator.StringToHash("moveShuffle");

		private int moveRunHash = Animator.StringToHash("moveRun");

		private int getUpHash = Animator.StringToHash("getUp");

		private int gunLoweredHash = Animator.StringToHash("gunLowered");

		private int gunAimHash = Animator.StringToHash("gunAim");

		private int gunReloadHash = Animator.StringToHash("gunReload");

		private int moveDirectionXHash = Animator.StringToHash("moveDirectionX");

		private int moveDirectionYHash = Animator.StringToHash("moveDirectionY");

		private int aimAngleHash = Animator.StringToHash("aimAngle");

		private Transform hoodie;

		private Transform hoodie2;

		private Transform hoodie3;

		private Transform mask;

		private bool canPlayEngageVO = true;

		private float targetColorTimer;

		protected override void Setup()
		{
			base.Setup();
			GameObject gameObject = GameObject.Find("SniperLocations");
			locationSequence = gameObject.transform.GetAllChildren();
			currentPosition = 0;
			if (locationSequence.Length != 0)
			{
				tf.position = locationSequence[currentPosition].position;
			}
			animations.Init(this);
			targets.Init();
			lineObject = line.gameObject;
			SetLine(set: false);
			targets.SetOffTargetVisuals();
			enemyVoices.Init();
			hitBoxResponse.Init(this, player, positioningInfo, navmeshMovement, animations);
			navmeshMovement.Init(this, positioningInfo, player);
			firearm.Init(this, positioningInfo);
			aiming.Init(this, player, positioningInfo, firearm);
			hoodie = animations.Animator.transform.FindRecursive("hoodie");
			hoodie2 = animations.Animator.transform.FindRecursive("hoodie2");
			hoodie3 = animations.Animator.transform.FindRecursive("hoodie3");
			mask = animations.Animator.transform.FindRecursive("mask");
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			hitBoxResponse.Deactivate();
			LowerGun();
			sprayedVisual.SetActive(value: false);
			hasBeenSprayed = false;
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
			SetState(SniperCopState.IDLE);
			HP = locationSequence.Length;
			SetLine(set: false);
			targets.SetOffTargetVisuals();
			escapeSequenceFirstTime = GameObject.Find("ch2s5b").GetComponent<PlayableDirector>();
			escapeSequence = GameObject.Find("SniperCaptainEscape").GetComponent<PlayableDirector>();
			escapeSequenceLastTime = GameObject.Find("SniperCaptainEscapeLastTime").GetComponent<PlayableDirector>();
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
				else if (Random.value > 0.5f)
				{
					canPlayEngageVO = true;
				}
				break;
			case SniperCopState.ATTACK:
				animations.PlayAnim(idleAgroHash);
				AimGun();
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
				LowerGun();
				targets.SetOffTargetVisuals();
				enemyVoices.PlayHurt();
				break;
			case SniperCopState.RELOCATE:
				animations.PlayAnim(getUpHash);
				navmeshMovement.WarpToPosition(locationSequence[currentPosition].position);
				tf.LookAt(locationSequence[currentPosition + 1].position, Vector3.up);
				if (HP == locationSequence.Length - 1)
				{
					SequenceHandler.instance.StartEnteringSequence(escapeSequenceFirstTime, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true, setAllowPhoneOnAfterSequence: false);
				}
				else if (HP == 1)
				{
					SequenceHandler.instance.StartEnteringSequence(escapeSequenceLastTime, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true, setAllowPhoneOnAfterSequence: false);
				}
				else
				{
					SequenceHandler.instance.StartEnteringSequence(escapeSequence, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true, setAllowPhoneOnAfterSequence: false);
				}
				worldHandlerInstance.AddCombatBlocker(5f);
				LowerGun();
				targets.SetOffTargetVisuals();
				break;
			case SniperCopState.DIE:
				navmeshMovement.StopObstacleAvoidance();
				ReportCrime();
				worldHandlerInstance.UnsubscribeToCombatArbiter(this);
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
				if (!DoKnockbackIfHit() && !BackOffOrDefend() && !AttackIfAble())
				{
					SetState(SniperCopState.MOVECLOSER);
				}
				break;
			case SniperCopState.MOVECLOSER:
				MovementAnimation();
				if (!DoKnockbackIfHit() && !AttackIfAble() && stateTime > 3f)
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.BACKOFF:
				MovementAnimation();
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
				if (!DoKnockbackIfHit())
				{
					if (navmeshMovement.CurrentMovementState == EnemyNavmeshMovement.MovementState.STATIONARY)
					{
						BackOffOrDefend();
					}
					else if (positioningInfo.DistToPlayer >= 1f && !AttackIfAble())
					{
						SetState(SniperCopState.IDLE);
					}
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
				if (stateTime < 1f)
				{
					if (aiming.LockOn)
					{
						if (!lockedOn)
						{
							stateTime = 1f;
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
				if (!DoKnockbackIfHit())
				{
					if (stateTime > 3f || (stateTime > 2f && UnderDirectThreat()))
					{
						SetState(SniperCopState.ATTACK);
					}
					else if (Vector3.Dot(normalized, firearm.Muzzle.forward) < 0f)
					{
						BackOffOrDefend();
					}
				}
				break;
			}
			case SniperCopState.ATTACK:
				if (!DoKnockbackIfHit() && stateTime > 0.5f && !ReloadIfNecessary() && !BackOffOrDefend() && !AttackIfAble())
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
				if (!DoKnockbackIfHit() && !firearm.Reloading && !BackOffOrDefend() && !AttackIfAble())
				{
					SetState(SniperCopState.IDLE);
				}
				break;
			case SniperCopState.HITRESPONSE:
				if (HP <= 0 && stateTime > 0.7f)
				{
					SetState(SniperCopState.DIE);
				}
				else if (stateTime > 0.5f && HP > 0)
				{
					SetState(SniperCopState.RELOCATE);
				}
				break;
			case SniperCopState.RELOCATE:
				if (!SequenceHandler.instance.IsInSequence())
				{
					currentPosition++;
					navmeshMovement.Deactivate();
					tf.position = locationSequence[currentPosition].position;
					navmeshMovement.Activate();
					hitBoxResponse.Activate();
					SetState(SniperCopState.IDLE);
					switch (currentPosition)
					{
					case 1:
						battle.SpawnAddsFromGroup(0, requiredForCompletion: false);
						break;
					case 2:
						battle.ClearAddsFromGroup(0);
						battle.SpawnAddsFromGroup(1, requiredForCompletion: false);
						break;
					case 3:
						battle.ClearAddsFromGroup(1);
						battle.SpawnAddsFromGroup(2, requiredForCompletion: false);
						break;
					}
				}
				break;
			case SniperCopState.DIE:
				if (!WorldHandler.instance.IsReallyInScreen(tf.position) && positioningInfo.SqrDistToPlayer > 100f)
				{
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
			case SniperCopState.DIE:
				navmeshMovement.StartObstacleAvoidance();
				break;
			case SniperCopState.IDLE:
			case SniperCopState.MOVECLOSER:
			case SniperCopState.BACKOFF:
			case SniperCopState.ATTACK:
			case SniperCopState.RELOAD:
			case SniperCopState.HITRESPONSE:
			case SniperCopState.RELOCATE:
				break;
			}
		}

		protected override void LateUpdateActive()
		{
			base.LateUpdateActive();
			if (currentPosition > 0)
			{
				hoodie.localPosition = new Vector3(0.074f, 0f, -0.108f);
				hoodie.localEulerAngles = new Vector3(0f, 254f, 0f);
				hoodie2.localPosition = new Vector3(0.134f, 0f, -0.26f);
				hoodie2.localEulerAngles = new Vector3(0f, 264f, 0f);
				hoodie3.localPosition = new Vector3(0.297f, 0f, -0.3f);
				hoodie3.localEulerAngles = new Vector3(0f, 299f, 0f);
				mask.localPosition = new Vector3(0.07f, 0f, 0f);
				mask.localEulerAngles = new Vector3(270f, 115f, 0f);
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			targets.SetOffTargetVisuals();
			navmeshMovement.Deactivate();
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.GetVelocity().sqrMagnitude > 0f)
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

		public override bool IsDead()
		{
			return sniperCopState == SniperCopState.DIE;
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

		public new void OnTriggerStay(Collider other)
		{
			if (other.gameObject.layer == 30 && !hasBeenSprayed && player.sprayHitbox == other.gameObject)
			{
				hasBeenSprayed = true;
				sprayedVisual.SetActive(value: true);
			}
		}

		private bool DoKnockbackIfHit()
		{
			bool flag = hitBoxResponse.RespondToHit();
			if (flag)
			{
				SetState(SniperCopState.HITRESPONSE);
				flag = true;
			}
			return flag;
		}

		private bool BackOffOrDefend()
		{
			if (positioningInfo.SqrDistToPlayer < 2.25f)
			{
				if (firearm.CanShoot)
				{
					float num = positioningInfo.SqrDistToPlayer / 2.25f;
					if (Random.value > num)
					{
						if (navmeshMovement.BackOffIfPossible(5f, 2f))
						{
							SetState(SniperCopState.BACKOFF);
							return true;
						}
						SetState(SniperCopState.AIMING);
						return true;
					}
				}
				else if (navmeshMovement.BackOffIfPossible(5f, 2f))
				{
					SetState(SniperCopState.BACKOFF);
					return true;
				}
			}
			return false;
		}

		private bool AttackIfAble()
		{
			bool flag = firearm.CanShoot;
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

		public override void ReceiveDamage(int damage)
		{
			damage = 1;
			base.ReceiveDamage(damage);
		}
	}
}
