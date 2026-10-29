using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class PoliceCaptainIrene : BasicEnemy
	{
		public enum BasicCopState
		{
			IDLE = 0,
			MOVETOCIRCLE = 1,
			ROTATEAROUND = 2,
			ATTACK = 3,
			HITRESPONSE = 4,
			MOVETOCENTER = 5,
			DIE = 6
		}

		public UnityEvent OnDeath;

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private float hoverHeight = 3f;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyHitResponse hitBoxResponse;

		[SerializeField]
		private EnemyMeleeWeapon meleeWeapon;

		[SerializeField]
		private EnemyFirearm firearm;

		[SerializeField]
		private Transform aimReferenceTransform;

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private bool melee = true;

		[SerializeField]
		private TrailRenderer[] fingerTrails;

		[SerializeField]
		private EnemyVoices enemyVoices;

		[SerializeField]
		private string movementTargetTag;

		[SerializeField]
		private GameObject sprayedVisual;

		[SerializeField]
		private EnemySpawnAdds addSpawner;

		[SerializeField]
		private bool cleanUpOnDeathImmediately;

		[SerializeField]
		private GameObject boostVisuals;

		private BasicCopState basicCopState;

		private BasicCopState lastBasicCopState;

		private float stateTime;

		private List<Transform> centers = new List<Transform>();

		private bool fingerTrailActive;

		private Transform currentCenter;

		private bool hasBeenSprayed;

		private int moveHash = Animator.StringToHash("move");

		private int moveGunHash = Animator.StringToHash("moveGun");

		private int aimHash = Animator.StringToHash("aim");

		private int shootHash = Animator.StringToHash("shoot");

		private int attackMeleeHash = Animator.StringToHash("attackMelee");

		private int attackMeleeHitHash = Animator.StringToHash("attackMeleeHit");

		private int moveDirectionXHash = Animator.StringToHash("moveDirectionX");

		private int moveDirectionYHash = Animator.StringToHash("moveDirectionY");

		private int aimAngleHash = Animator.StringToHash("aimAngle");

		private int successiveHits;

		protected override void Setup()
		{
			base.Setup();
			animations.Init(this);
			enemyVoices.Init();
			hitBoxResponse.Init(this, player, positioningInfo, navmeshMovement, animations);
			navmeshMovement.Init(this, positioningInfo, player);
			meleeWeapon.Init(this, positioningInfo);
			if (firearm != null)
			{
				firearm.Init(this, positioningInfo);
			}
			if (aiming != null)
			{
				aiming.Init(this, player);
			}
			GameObject[] array = GameObject.FindGameObjectsWithTag(movementTargetTag);
			for (int i = 0; i < array.Length; i++)
			{
				centers.Add(array[i].transform);
			}
			if (addSpawner != null)
			{
				addSpawner.Init(this);
			}
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			hitBoxResponse.Deactivate();
			animations.PlayAnim(moveHash);
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
			HP = fullHP;
			meleeWeapon.Equip(melee);
			if (firearm != null)
			{
				firearm.Equip(!melee);
			}
			SetState(BasicCopState.IDLE);
			boostVisuals.SetActive(value: true);
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			basicCopState = BasicCopState.IDLE;
			navmeshMovement.Deactivate();
		}

		private void SetState(BasicCopState targetState)
		{
			lastBasicCopState = basicCopState;
			ExitState(basicCopState);
			basicCopState = targetState;
			stateTime = 0f;
			switch (basicCopState)
			{
			case BasicCopState.IDLE:
				navmeshMovement.MoveToPlayer(5f);
				navmeshMovement.LookAtPlayer(instant: false);
				break;
			case BasicCopState.MOVETOCIRCLE:
				navmeshMovement.MoveToPlayer(4f);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetMovementSpeed(4f);
				break;
			case BasicCopState.ROTATEAROUND:
				navmeshMovement.SetMovementSpeed(3f);
				navmeshMovement.CirclePlayer(3f, 6f, 10f, 0.5f, 20f, 40f);
				break;
			case BasicCopState.ATTACK:
				if (!melee)
				{
					navmeshMovement.StopMoving();
					navmeshMovement.LookAtPlayer(instant: true);
					animations.PlayAnim(aimHash, restart: true);
					animations.SetFloatParameter(moveDirectionXHash, 0f);
					animations.SetFloatParameter(moveDirectionYHash, 0f);
				}
				if (melee)
				{
					animations.PlayAnim(attackMeleeHash, restart: true);
					meleeWeapon.Attack();
					navmeshMovement.MoveToPlayer(0f);
					navmeshMovement.LookAtPlayer(instant: false);
					navmeshMovement.SetMovementSpeed(0f);
					navmeshMovement.StopObstacleAvoidance();
				}
				break;
			case BasicCopState.HITRESPONSE:
				if (HP > 0)
				{
					enemyVoices.PlayHurt();
				}
				else
				{
					enemyVoices.PlayDeath();
				}
				navmeshMovement.Ground();
				if (melee)
				{
					meleeWeapon.CancelAttack();
				}
				break;
			case BasicCopState.MOVETOCENTER:
				currentCenter = centers[Random.Range(0, centers.Count)];
				navmeshMovement.MoveToStaticTarget(currentCenter.position);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetMovementSpeed(12f);
				navmeshMovement.ResumeHover(hoverHeight);
				navmeshMovement.Ascend(4f);
				break;
			case BasicCopState.DIE:
				navmeshMovement.Ground();
				navmeshMovement.StopObstacleAvoidance();
				navmeshMovement.StopMoving();
				boostVisuals.SetActive(value: false);
				ReportCrime();
				OnDeath.Invoke();
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
			switch (basicCopState)
			{
			case BasicCopState.IDLE:
				MovementAnimation();
				successiveHits = 0;
				if (!DoKnockbackIfHit())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			case BasicCopState.MOVETOCIRCLE:
				MovementAnimation();
				if (stateTime > 1f)
				{
					successiveHits = 0;
				}
				if (!DoKnockbackIfHit() && !AttackIfInRangeAndAble())
				{
					CirclePlayerIfInRange();
				}
				break;
			case BasicCopState.ROTATEAROUND:
				MovementAnimation();
				if (stateTime > 1f)
				{
					successiveHits = 0;
				}
				if (!DoKnockbackIfHit() && !AttackIfInRangeAndAble())
				{
					MoveToCircleIfTooFar();
				}
				break;
			case BasicCopState.ATTACK:
				successiveHits = 0;
				if (melee)
				{
					if (positioningInfo.SqrDistToPlayer < 9f || meleeWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER || meleeWeapon.HitLanded)
					{
						animations.PlayAnim(attackMeleeHitHash, restart: false);
					}
					if (meleeWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK && !meleeWeapon.HitLanded)
					{
						if (!fingerTrailActive)
						{
							fingerTrailActive = true;
							for (int i = 0; i < fingerTrails.Length; i++)
							{
								fingerTrails[i].enabled = fingerTrailActive;
							}
						}
						navmeshMovement.Descend(10f);
						navmeshMovement.SetMovementSpeed(20f);
						if (CheckIfInRange(4f))
						{
							navmeshMovement.MoveToStaticTarget(tf.position + tf.forward * 5f);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
					}
					else if (meleeWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER || meleeWeapon.HitLanded)
					{
						navmeshMovement.MoveToStaticTarget(tf.position + tf.forward);
						navmeshMovement.SetMovementSpeed(7f);
						navmeshMovement.Ascend(4f);
						if (meleeWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER)
						{
							worldHandlerInstance.AddCombatBlocker(3f);
							navmeshMovement.LookAtCurrentDir(instant: false);
						}
					}
					if (stateTime > 2f && fingerTrailActive)
					{
						fingerTrailActive = false;
						for (int j = 0; j < fingerTrails.Length; j++)
						{
							fingerTrails[j].enabled = fingerTrailActive;
						}
					}
					if (!meleeWeapon.CanAttack && (meleeWeapon.State == EnemyMeleeWeapon.AttackState.RECOVER || meleeWeapon.State == EnemyMeleeWeapon.AttackState.NONE) && stateTime > 2f && !DoKnockbackIfHit() && !AttackIfInRangeAndAble() && !meleeWeapon.Attacking)
					{
						SetState(BasicCopState.MOVETOCENTER);
					}
				}
				else
				{
					navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: true);
					SetAimAngle();
					if (stateTime > 1f && firearm.CanShoot)
					{
						firearm.Shoot(firearm.Muzzle.position, aiming.GetTargetPos() - firearm.Muzzle.position, aiming.TargetAbility);
						animations.PlayAnim(shootHash, restart: true);
					}
					if (!DoKnockbackIfHit() && firearm.Reloading && positioningInfo.DiffToPlayer.y < -3f)
					{
						SetState(BasicCopState.MOVETOCENTER);
					}
				}
				break;
			case BasicCopState.HITRESPONSE:
				if (HP <= 0 && hitBoxResponse.State == EnemyHitResponse.HitResponseState.LYINGDOWN && stateTime > 1f)
				{
					SetState(BasicCopState.DIE);
				}
				else if (!DoKnockbackIfHit() && hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
				{
					SetState(BasicCopState.MOVETOCENTER);
				}
				break;
			case BasicCopState.MOVETOCENTER:
			{
				MovementAnimation();
				if (stateTime > 1f)
				{
					successiveHits = 0;
				}
				Vector3 vector = currentCenter.position - tf.position;
				vector.y = 0f;
				if (vector.sqrMagnitude > 4f)
				{
					stateTime = 0f;
				}
				if (!DoKnockbackIfHit() && vector.sqrMagnitude <= 4f && stateTime > 2f && !AttackIfInRangeAndAble())
				{
					MoveToCircleOrCircleAround();
				}
				break;
			}
			case BasicCopState.DIE:
				if ((!WorldHandler.instance.IsReallyInScreen(tf.position) && positioningInfo.SqrDistToPlayer > 100f) || cleanUpOnDeathImmediately)
				{
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
				navmeshMovement.Ascend(4f);
				navmeshMovement.StartObstacleAvoidance();
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
			case BasicCopState.MOVETOCENTER:
				break;
			}
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.GetVelocity().sqrMagnitude > 0f)
			{
				if (melee)
				{
					animations.PlayAnim(moveHash);
				}
				else
				{
					animations.PlayAnim(moveGunHash);
				}
				Vector3 localMovementVelocity = navmeshMovement.GetLocalMovementVelocity();
				animations.SetFloatParameter(moveDirectionXHash, Mathf.Lerp(animations.GetFloatParameter(moveDirectionXHash), localMovementVelocity.x / 3f, Core.dt * 2f));
				animations.SetFloatParameter(moveDirectionYHash, Mathf.Lerp(animations.GetFloatParameter(moveDirectionYHash), localMovementVelocity.z / 3f, Core.dt * 2f));
			}
			else
			{
				if (melee)
				{
					animations.PlayAnim(moveHash);
				}
				else
				{
					animations.PlayAnim(moveGunHash);
				}
				animations.SetFloatParameter(moveDirectionXHash, Mathf.Lerp(animations.GetFloatParameter(moveDirectionXHash), 0f, Core.dt * 5f));
				animations.SetFloatParameter(moveDirectionYHash, Mathf.Lerp(animations.GetFloatParameter(moveDirectionYHash), 0f, Core.dt * 5f));
			}
		}

		public override bool IsDead()
		{
			if (stateTime > 0.5f)
			{
				return basicCopState == BasicCopState.DIE;
			}
			return false;
		}

		private void SetAimAngle()
		{
			Vector3 normalized = (aiming.GetTargetPos() + Vector3.up * 1.2f - aimReferenceTransform.position - aimReferenceTransform.right * 0.5f + aimReferenceTransform.right * 0.5f).normalized;
			float num = Vector3.Angle(new Vector3(normalized.x, 0f, normalized.z).normalized, normalized);
			float num2 = ((normalized.y > 0f) ? 1 : (-1));
			animations.SetFloatParameter(aimAngleHash, num2 * (num / 45f));
		}

		public new void OnTriggerStay(Collider other)
		{
			if (other.gameObject.layer == 30 && !hasBeenSprayed && player.sprayHitbox == other.gameObject)
			{
				hasBeenSprayed = true;
				sprayedVisual.SetActive(value: true);
			}
		}

		public override void ReceiveDamage(int damage)
		{
			damage = ((!(navmeshMovement.CurrentBaseOffset < 2f)) ? 1 : 0);
			base.ReceiveDamage(damage);
		}

		private bool MoveToCircleIfTooFar()
		{
			bool num = !CheckIfInRangeFlat(15f);
			if (num)
			{
				SetState(BasicCopState.MOVETOCIRCLE);
			}
			return num;
		}

		private bool CirclePlayerIfInRange()
		{
			bool num = CheckIfInRangeFlat(10f);
			if (num)
			{
				SetState(BasicCopState.ROTATEAROUND);
			}
			return num;
		}

		private bool MoveToCircleOrCircleAround()
		{
			bool flag = false;
			if (CheckIfInRangeFlat(10f))
			{
				SetState(BasicCopState.ROTATEAROUND);
				return true;
			}
			SetState(BasicCopState.MOVETOCIRCLE);
			return true;
		}

		private bool DoKnockbackIfHit()
		{
			if (successiveHits > 2)
			{
				return false;
			}
			bool flag = hitBoxResponse.RespondToHit();
			if (flag)
			{
				successiveHits++;
				if (firearm as CuffMissileLauncher != null)
				{
					(firearm as CuffMissileLauncher).ClearMissiles();
				}
				SetState(BasicCopState.HITRESPONSE);
				flag = true;
			}
			return flag;
		}

		private bool AttackIfInRangeAndAble()
		{
			bool flag = ((!melee) ? (firearm.IsInRangeFlat() && firearm.CanShoot) : (meleeWeapon.IsInRangeFlat() && meleeWeapon.CanAttack && positioningInfo.DiffToPlayer.y < 0f));
			if (flag)
			{
				RequestAttack(melee ? 2 : 6);
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

		private bool CheckIfInRange(float range)
		{
			return positioningInfo.SqrDistToPlayer < range * range;
		}

		private bool CheckIfInRangeFlat(float range)
		{
			return positioningInfo.SqrFlatDistToPlayer < range * range;
		}
	}
}
