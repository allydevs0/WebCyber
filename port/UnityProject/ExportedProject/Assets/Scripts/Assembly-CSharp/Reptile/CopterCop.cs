using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class CopterCop : BasicEnemy
	{
		public enum CopterCopState
		{
			IDLE = 0,
			MOVETORANGE = 1,
			BACKOFTORANGE = 2,
			HANGATRANGE = 3,
			ATTACK = 4,
			RELOAD = 5,
			UNDER_ATTACK = 6,
			DIE = 7
		}

		public UnityEvent OnDeath;

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private EnemyFirearm firearm;

		[SerializeField]
		private EnemyExtension extension;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private GameObject deathExplosionPrefab;

		[SerializeField]
		private LayerMask environmentLayerMask;

		[SerializeField]
		private Transform visualTransform;

		[SerializeField]
		private ParticleSystem groundDust;

		[SerializeField]
		private EnemySpawnAdds addSpawner;

		[SerializeField]
		private AudioSource audioSourceSingleShot;

		[SerializeField]
		private AudioSource audioSourceEngine;

		private PoolableObjectToName deathExplosion;

		private CopterCopState copterCopState;

		private float stateTime;

		private Quaternion machineGunDefaultRotation;

		private Vector3 spawnDestination;

		private int MovementXHash = Animator.StringToHash("MovementX");

		private int MovementYHash = Animator.StringToHash("MovementY");

		private int CopterSpinOutHash = Animator.StringToHash("CopterSpinOut");

		private int CopterMovementHash = Animator.StringToHash("CopterMovement");

		private bool audioPaused;

		private float machineGunRotationSpeed = 100f;

		protected override void Setup()
		{
			base.Setup();
			deathExplosion = new PoolableObjectToName(deathExplosionPrefab, deathExplosionPrefab.name);
			animations.Init(this);
			navmeshMovement.Init(this, positioningInfo, player);
			aiming.Init(this, player);
			firearm.Init(this, positioningInfo);
			extension.Init(this, player);
			extension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			machineGunDefaultRotation = firearm.VisualRoot.transform.localRotation;
			if (addSpawner != null)
			{
				addSpawner.Init(this);
			}
		}

		public void SetSpawnDestination(Vector3 position)
		{
			spawnDestination = position;
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			navmeshMovement.Deactivate();
			extension.ResetGraffitiSpots();
			extension.CloseExtension();
			Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.CopterBlades, audioSourceEngine);
		}

		protected override void UpdateSpawning()
		{
			Vector3 vector = spawnDestination - tf.position;
			float sqrMagnitude = vector.sqrMagnitude;
			if (sqrMagnitude > 0.25f)
			{
				tf.position += vector.normalized * Core.dt * (10f - Mathf.Clamp(5f - sqrMagnitude, 0f, 10f));
				tf.forward = positioningInfo.FlatDirToPlayer;
				animations.PlayAnim(CopterMovementHash);
				Vector3 vector2 = tf.InverseTransformDirection(vector.normalized);
				animations.SetFloatParameter(MovementXHash, Mathf.Lerp(animations.GetFloatParameter(MovementXHash), vector2.x, Core.dt));
				animations.SetFloatParameter(MovementYHash, Mathf.Lerp(animations.GetFloatParameter(MovementYHash), vector2.z, Core.dt));
			}
			if (sqrMagnitude <= 0.25f)
			{
				SetBasicState(BasicEnemyState.ACTIVE);
			}
			UpdateGroundDust();
		}

		protected override void EnteringActiveState()
		{
			base.EnteringActiveState();
			navmeshMovement.Activate();
			HP = fullHP;
			SetState(CopterCopState.IDLE);
		}

		private void SetState(CopterCopState targetState)
		{
			ExitState(copterCopState);
			stateTime = 0f;
			copterCopState = targetState;
			switch (copterCopState)
			{
			case CopterCopState.MOVETORANGE:
				navmeshMovement.MoveToPlayer(firearm.MinAttackRange);
				navmeshMovement.SetMovementSpeed(10f);
				navmeshMovement.LookAtMoveDir(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				break;
			case CopterCopState.BACKOFTORANGE:
				navmeshMovement.MoveToPlayer(firearm.DesiredAttackRange + 5f);
				navmeshMovement.SetMovementSpeed(2f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				break;
			case CopterCopState.HANGATRANGE:
				navmeshMovement.SetMovementSpeed(4f);
				navmeshMovement.CirclePlayer(2f, 5f, firearm.DesiredAttackRange, 5f, 10f, 40f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				break;
			case CopterCopState.ATTACK:
				navmeshMovement.MoveToStaticTarget(tf.position);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				machineGunRotationSpeed = 1f;
				break;
			case CopterCopState.RELOAD:
				navmeshMovement.MoveToStaticTarget(tf.position);
				navmeshMovement.LookAtCurrentDir(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				break;
			case CopterCopState.UNDER_ATTACK:
				navmeshMovement.StopObstacleAvoidance();
				animations.SetFloatParameter(MovementXHash, 0f);
				animations.SetFloatParameter(MovementYHash, 0f);
				navmeshMovement.LookAtCurrentDir(instant: true);
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.StopMoving();
				extension.OpenExtension();
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.CockpitGetHit, audioSourceSingleShot);
				DoHitPause(0.1f);
				player.hitBounceAbility.Trigger(extension.transform.position, 1.3f, tf.forward);
				if (positioningInfo.DirToPlayer.y < 1f)
				{
					player.hitBounceAbility.bounceType = HitBounceAbility.BounceType.COPTER_BOUNCE_FROM_BELOW;
				}
				else
				{
					player.hitBounceAbility.bounceType = HitBounceAbility.BounceType.COPTER_BOUNCE_FROM_FRONT;
				}
				break;
			case CopterCopState.DIE:
				if (extension.GSpot.lastPlayerToPaint == player)
				{
					WorldHandler.instance.AddCombatBlocker(5f);
					WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
					player.tf.position = extension.GSpot.GetGraffitiSpotPlayerPos();
					Vector3 vector = -player.tf.forward;
					player.DoJumpEffects(vector);
					Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
					player.SetRotHard(Quaternion.LookRotation(normalized));
					float num = player.jumpSpeed * 1f;
					float num2 = player.maxMoveSpeed * 1f;
					player.SetVelocity(num * Vector3.up + normalized * num2);
					player.ActivateAbility(player.flipOutJumpAbility);
				}
				navmeshMovement.Deactivate();
				animations.PlayAnim(CopterSpinOutHash);
				Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.CopterBladesCrash, audioSourceEngine);
				break;
			case CopterCopState.IDLE:
				break;
			}
		}

		protected override void PauseImpl()
		{
			base.PauseImpl();
			if (!audioPaused && base.State == PoolableState.Active && copterCopState != CopterCopState.DIE && player.IsBusyWithSequence())
			{
				audioPaused = true;
				Core.Instance.AudioManager.StopLoopingSfx(audioSourceEngine);
			}
		}

		protected override void UnPauseImpl()
		{
			base.UnPauseImpl();
			if (audioPaused && base.State == PoolableState.Active && copterCopState != CopterCopState.DIE)
			{
				audioPaused = false;
				Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.CopterBlades, audioSourceEngine);
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			UpdateGroundDust();
			UpdateState();
		}

		private void UpdateGroundDust()
		{
			if (Physics.Raycast(tf.position, -Vector3.up, out var hitInfo, 15f, environmentLayerMask, QueryTriggerInteraction.Ignore))
			{
				ParticleSystem.MinMaxGradient startColor = groundDust.main.startColor;
				startColor.colorMin = new Vector4(startColor.colorMin.r, startColor.colorMin.g, startColor.colorMin.b, 0.8f - (tf.position.y - hitInfo.point.y) / 15f);
				startColor.colorMax = new Vector4(startColor.colorMax.r, startColor.colorMax.g, startColor.colorMax.b, 1f - (tf.position.y - hitInfo.point.y) / 15f);
				ParticleSystem.MainModule main = groundDust.main;
				main.startColor = startColor;
				groundDust.transform.position = hitInfo.point;
				if (!groundDust.isPlaying)
				{
					groundDust.Play();
				}
			}
			else if (groundDust.isPlaying)
			{
				groundDust.Stop();
			}
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (copterCopState)
			{
			case CopterCopState.IDLE:
				HoverMovement();
				SetState(CopterCopState.MOVETORANGE);
				break;
			case CopterCopState.MOVETORANGE:
				HoverMovement();
				MovementAnimation();
				if (positioningInfo.SqrFlatDistToPlayer < firearm.DesiredAttackRange * firearm.DesiredAttackRange)
				{
					SetState(CopterCopState.HANGATRANGE);
					return;
				}
				if (HaltIfUnderAttack())
				{
					return;
				}
				break;
			case CopterCopState.BACKOFTORANGE:
				HoverMovement();
				MovementAnimation();
				if (positioningInfo.SqrFlatDistToPlayer > firearm.DesiredAttackRange * firearm.DesiredAttackRange)
				{
					SetState(CopterCopState.HANGATRANGE);
					return;
				}
				if (HaltIfUnderAttack())
				{
					return;
				}
				break;
			case CopterCopState.HANGATRANGE:
				HoverMovement();
				MovementAnimation();
				if (HaltIfUnderAttack() || AttackIfInRangeAndAble() || MoveCloserIfNecessary() || MoveBackIfNecessary())
				{
					return;
				}
				break;
			case CopterCopState.ATTACK:
				if (stateTime <= 1f)
				{
					navmeshMovement.SetRotationSpeed(Mathf.Lerp(1f, 10f, stateTime));
					machineGunRotationSpeed = Mathf.Lerp(machineGunRotationSpeed, 100f, stateTime);
				}
				HoverMovement();
				MovementAnimation();
				if (positioningInfo.SqrFlatDistToPlayer > firearm.DesiredAttackRange * firearm.DesiredAttackRange)
				{
					navmeshMovement.MoveToPlayer(firearm.MinAttackRange);
				}
				else
				{
					navmeshMovement.MoveToStaticTarget(tf.position);
				}
				if (stateTime > 1f)
				{
					firearm.Shoot(Vector3.ProjectOnPlane(aiming.GetTargetPos() - firearm.VisualRoot.transform.position, tf.right), aiming.TargetAbility);
				}
				if (HaltIfUnderAttack() || MoveBackIfNecessary() || ReloadIfNecessary())
				{
					return;
				}
				if (stateTime > 6f)
				{
					SetState(CopterCopState.HANGATRANGE);
					return;
				}
				break;
			case CopterCopState.RELOAD:
				HoverMovement();
				MovementAnimation();
				if (HaltIfUnderAttack())
				{
					return;
				}
				if (!firearm.Reloading)
				{
					SetState(CopterCopState.MOVETORANGE);
				}
				break;
			case CopterCopState.UNDER_ATTACK:
				if (stateTime > 3f)
				{
					extension.CloseExtension();
					SetState(CopterCopState.IDLE);
					return;
				}
				if (DieIfDead())
				{
					return;
				}
				break;
			case CopterCopState.DIE:
			{
				base.transform.position -= Vector3.up * 5f * Core.dt;
				if (stateTime > 5f || Physics.SphereCast(tf.position, 1.5f, Vector3.down, out var _, 5f * Core.dt + 0.5f, environmentLayerMask, QueryTriggerInteraction.Ignore))
				{
					GetPoolableFromPoolOrInstantiate(deathExplosion).InitPoolable(tf.position, tf.rotation);
					OnDeath.Invoke();
					SetBasicState(BasicEnemyState.UNACTIVE);
					Core.Instance.AudioManager.StopLoopingSfx(audioSourceEngine);
				}
				break;
			}
			}
			extension.ReceivedHit = false;
		}

		protected override void LateUpdateActive()
		{
			base.LateUpdateActive();
			Quaternion quaternion;
			if (copterCopState == CopterCopState.ATTACK)
			{
				quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(aiming.GetTargetPos() - firearm.VisualRoot.transform.position, visualTransform.right), visualTransform.up);
				quaternion = Quaternion.Inverse(firearm.VisualRoot.transform.parent.rotation) * quaternion;
			}
			else
			{
				quaternion = machineGunDefaultRotation;
			}
			firearm.VisualRoot.transform.localRotation = Quaternion.Lerp(firearm.VisualRoot.transform.localRotation, quaternion, 1f - Mathf.Exp((0f - machineGunRotationSpeed) * Core.dt));
		}

		private void ExitState(CopterCopState state)
		{
			switch (state)
			{
			case CopterCopState.ATTACK:
				machineGunRotationSpeed = 1f;
				break;
			case CopterCopState.UNDER_ATTACK:
				navmeshMovement.StartObstacleAvoidance();
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
			animations.PlayAnim(CopterMovementHash);
			Vector3 localMovementDirection = navmeshMovement.GetLocalMovementDirection();
			animations.SetFloatParameter(MovementXHash, Mathf.Lerp(animations.GetFloatParameter(MovementXHash), localMovementDirection.x, Core.dt));
			animations.SetFloatParameter(MovementYHash, Mathf.Lerp(animations.GetFloatParameter(MovementYHash), localMovementDirection.z, Core.dt));
		}

		private void HoverMovement()
		{
			if (positioningInfo.DiffToPlayer.y > -1f)
			{
				navmeshMovement.ResumeHover(3f);
				navmeshMovement.Ascend(1f);
			}
			else if (positioningInfo.DiffToPlayer.y < -4f)
			{
				navmeshMovement.Descend(1f);
			}
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

		private void OnGSpotStateChanged(GraffitiSpotFinisher gSpot, GraffitiState result)
		{
			if (result == GraffitiState.FINISHED)
			{
				ReceiveDamage(1);
			}
		}

		private bool HaltIfUnderAttack()
		{
			if (extension.ReceivedHit)
			{
				player.DoHitEffect(extension.transform);
				SetState(CopterCopState.UNDER_ATTACK);
				extension.ReceivedHit = false;
				return true;
			}
			return false;
		}

		private bool DieIfDead()
		{
			bool num = HP <= 0;
			if (num)
			{
				SetState(CopterCopState.DIE);
			}
			return num;
		}

		private bool AttackIfInRangeAndAble()
		{
			bool flag = firearm.IsInRangeFlat() && firearm.CanShoot;
			if (flag)
			{
				RequestAttack(6f);
				if (receivedAttackPermission)
				{
					if (LineOfSight(tf, positioningInfo.DirToPlayer, positioningInfo.DistToPlayer + 1f))
					{
						SetState(CopterCopState.ATTACK);
					}
					else
					{
						ReleaseAttack();
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
				SetState(CopterCopState.RELOAD);
			}
			return reloading;
		}

		private bool MoveCloserIfNecessary()
		{
			bool num = positioningInfo.SqrFlatDistToPlayer > firearm.MaxAttackRange * firearm.MaxAttackRange;
			if (num)
			{
				SetState(CopterCopState.MOVETORANGE);
			}
			return num;
		}

		private bool MoveBackIfNecessary()
		{
			bool num = positioningInfo.SqrFlatDistToPlayer < firearm.MinAttackRange * firearm.MinAttackRange;
			if (num)
			{
				SetState(CopterCopState.BACKOFTORANGE);
			}
			return num;
		}
	}
}
