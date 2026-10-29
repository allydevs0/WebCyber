using UnityEngine;
using UnityEngine.Audio;

namespace Reptile
{
	public class Tankwalker : BasicEnemy
	{
		public enum TankwalkerState
		{
			IDLE = 0,
			MOVETOCIRCLE = 1,
			ROTATEAROUND = 2,
			TANKGUN_AIM = 3,
			TANKGUN_ATTACK = 4,
			TANKGUN_RELOAD = 5,
			MACHINEGUN_PREPARE = 6,
			MACHINEGUN_ATTACK = 7,
			MACHINEGUN_VENT = 8,
			POUND = 9,
			SLAP = 10,
			HEADSTAB = 11,
			LOSE_ARM = 12,
			DIE = 13
		}

		public enum HeadTargetMode
		{
			DEFAULT = 0,
			PLAYER = 1,
			CUSTOM = 2
		}

		[SerializeField]
		private EnemyNavmeshMovement navmeshMovement;

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyMeleeWeapon poundWeapon;

		[SerializeField]
		private EnemyMeleeWeapon slapWeapon;

		[SerializeField]
		private EnemyMeleeWeapon headStabWeapon;

		[SerializeField]
		private EnemyFirearm leftMachineGun;

		[SerializeField]
		private EnemyFirearm rightMachineGun;

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private Transform aimReferenceTransform;

		[SerializeField]
		private EnemyGrindLines enemyGrindLines;

		[SerializeField]
		private EnemyTargets enemyTargets;

		[SerializeField]
		private EnemyFirearm mortar;

		[SerializeField]
		private EnemyExtension extensionCockpit;

		[SerializeField]
		private ParticleSystem cockpitSmoke;

		[SerializeField]
		private ParticleSystem cockpitOverheat;

		[SerializeField]
		private ParticleSystem cockpitExplosion;

		[SerializeField]
		private ParticleSystem groundImpact;

		[SerializeField]
		private GameObject poundEffect;

		[SerializeField]
		private GameObject slapEffect;

		[SerializeField]
		private GameObject stabEffect;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioSource ventLeftAudioSource;

		[SerializeField]
		private AudioSource ventRightAudioSource;

		[SerializeField]
		private AudioSource cockpitAudioSource;

		[SerializeField]
		private Transform headBone;

		[SerializeField]
		private Transform shLBone;

		[SerializeField]
		private Transform shRBone;

		[SerializeField]
		private Transform spine1Bone;

		[SerializeField]
		private Transform spine2Bone;

		[SerializeField]
		private GameObject[] turretGrindLines;

		[SerializeField]
		private EnemySpawnAdds addSpawner;

		[SerializeField]
		private Arm leftArmData;

		[SerializeField]
		private Arm rightArmData;

		[SerializeField]
		private EnemyAnimationEventRelay animationEventRelay;

		[SerializeField]
		private bool deactivateCockpitAfterExplosion = true;

		[SerializeField]
		private float durationBeforeDeath = 16f;

		[SerializeField]
		private bool sinkAfterDeath = true;

		private TankwalkerState tankwalkerState;

		private TankwalkerState lastTankwalkerState;

		private float stateTime;

		private int idleHash = Animator.StringToHash("idle");

		private int walkHash = Animator.StringToHash("walk");

		private int aimMachinegunsHash = Animator.StringToHash("aimMachineguns");

		private int poundHash = Animator.StringToHash("pound");

		private int slapHashRight = Animator.StringToHash("slapRight");

		private int slapHashLeft = Animator.StringToHash("slapLeft");

		private int headStabHash = Animator.StringToHash("headStab");

		private int AimDistanceHash = Animator.StringToHash("AimDistance");

		private int AimAngleHash = Animator.StringToHash("AimAngle");

		private int prepareMachineGunsHash = Animator.StringToHash("prepareMachineguns");

		private int machineGunsEndHash = Animator.StringToHash("machinegunsEnd");

		private int ventStartHash = Animator.StringToHash("ventStart");

		private int aimMachineGunsHash = Animator.StringToHash("aimMachineguns");

		private int ventEndHash = Animator.StringToHash("ventEnd");

		private int tankwalkerDieHash = Animator.StringToHash("tankwalkerDie");

		private int tankwalkerLoseLeftArmHash = Animator.StringToHash("tankwalkerLoseLeftArm");

		private int tankwalkerLoseRightArmHash = Animator.StringToHash("tankwalkerLoseRightArm");

		private int moveXHash = Animator.StringToHash("MoveX");

		private int moveYHash = Animator.StringToHash("MoveY");

		private int tankwalkerMortarShootHash = Animator.StringToHash("tankwalkerMortarShoot");

		private bool leftArmAlive = true;

		private bool rightArmAlive = true;

		private AudioManager audioManager;

		private HeadTargetMode headTargetMode;

		private Vector3 headTargetDir;

		private float postReloadDelay;

		private bool hitWhileVenting;

		private bool needToLoseArm;

		private int placedTargetInt;

		private bool playerWasOnMe;

		private Quaternion currentShRRotation;

		private Quaternion currentShLRotation = Quaternion.identity;

		private Quaternion currentShRLocalRotation;

		private Quaternion currentShLLocalRotation = Quaternion.identity;

		private float curHeadAngle;

		private Quaternion currentSpineLocalRotation = Quaternion.identity;

		private Quaternion currentSpine2LocalRotation = Quaternion.identity;

		private float poundActivateTimer;

		protected override void Setup()
		{
			base.Setup();
			navmeshMovement.Init(this, positioningInfo, player);
			animations.Init(this);
			poundWeapon.Init(this, positioningInfo);
			slapWeapon.Init(this, positioningInfo);
			headStabWeapon.Init(this, positioningInfo);
			leftMachineGun.Init(this, positioningInfo);
			rightMachineGun.Init(this, positioningInfo);
			aiming.Init(this, player);
			enemyGrindLines.Init();
			enemyTargets.Init();
			leftArmData.armExtension.Init(this, player);
			leftArmData.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			rightArmData.armExtension.Init(this, player);
			rightArmData.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			extensionCockpit.Init(this, player);
			extensionCockpit.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			mortar.Init(this, positioningInfo);
			audioManager = Core.Instance.AudioManager;
			if (addSpawner != null)
			{
				addSpawner.Init(this);
			}
			animationEventRelay.Init();
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			leftMachineGun.Equip(equip: true);
			rightMachineGun.Equip(equip: true);
			slapWeapon.Equip(equip: true);
			poundWeapon.Equip(equip: true);
			poundEffect.SetActive(value: false);
			slapEffect.SetActive(value: false);
			stabEffect.SetActive(value: false);
			leftArmData.ResetArm();
			rightArmData.ResetArm();
			extensionCockpit.CloseExtension();
			extensionCockpit.ResetGraffitiSpots();
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
			HP = fullHP;
			SetState(TankwalkerState.IDLE);
			leftArmData.SwitchToLongGrindLines();
			rightArmData.SwitchToLongGrindLines();
		}

		private void SetState(TankwalkerState targetState)
		{
			lastTankwalkerState = tankwalkerState;
			ExitState(tankwalkerState);
			tankwalkerState = targetState;
			stateTime = 0f;
			switch (tankwalkerState)
			{
			case TankwalkerState.IDLE:
				navmeshMovement.StopMoving();
				animations.PlayAnim(idleHash);
				headTargetMode = HeadTargetMode.DEFAULT;
				navmeshMovement.SetRotationSpeed(1f);
				navmeshMovement.LookAtPlayer(instant: false);
				break;
			case TankwalkerState.MOVETOCIRCLE:
			{
				headTargetMode = HeadTargetMode.PLAYER;
				animations.PlayAnim(walkHash);
				navmeshMovement.SetMovementSpeed(4f);
				float num3 = 5f;
				num3 = ((leftMachineGun.CanShoot || rightMachineGun.CanShoot) ? leftMachineGun.MinAttackRange : (mortar.CanShoot ? mortar.MinAttackRange : (slapWeapon.CanAttack ? slapWeapon.DesiredAttackRange : ((!headStabWeapon.CanAttack) ? poundWeapon.DesiredAttackRange : headStabWeapon.DesiredAttackRange))));
				navmeshMovement.CirclePlayer(3f, 5f, num3, 1f, 20f, 40f);
				navmeshMovement.LookAtPlayer(instant: false);
				navmeshMovement.SetRotationSpeed(1f);
				break;
			}
			case TankwalkerState.TANKGUN_AIM:
				headTargetMode = HeadTargetMode.CUSTOM;
				placedTargetInt = 0;
				mortar.ClipSize = 0;
				animations.PlayAnim(idleHash);
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				break;
			case TankwalkerState.TANKGUN_ATTACK:
				headTargetMode = HeadTargetMode.CUSTOM;
				animations.PlayAnim(idleHash);
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				break;
			case TankwalkerState.MACHINEGUN_PREPARE:
				headTargetMode = HeadTargetMode.DEFAULT;
				animations.PlayAnim(prepareMachineGunsHash);
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunStart, audioSource);
				navmeshMovement.StopMoving();
				navmeshMovement.SetRotationSpeed(1f);
				break;
			case TankwalkerState.MACHINEGUN_ATTACK:
				headTargetMode = HeadTargetMode.DEFAULT;
				animations.PlayAnim(aimMachinegunsHash);
				currentShRRotation = shRBone.rotation;
				currentShLRotation = shLBone.rotation;
				navmeshMovement.StopMoving();
				navmeshMovement.SetRotationSpeed(0.4f);
				if (rightArmData.armAlive)
				{
					rightArmData.SwitchToShortGrindLines();
				}
				if (leftArmData.armAlive)
				{
					leftArmData.SwitchToShortGrindLines();
				}
				break;
			case TankwalkerState.MACHINEGUN_VENT:
				headTargetMode = HeadTargetMode.DEFAULT;
				hitWhileVenting = false;
				animations.PlayAnim(machineGunsEndHash);
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, audioSource);
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: true);
				break;
			case TankwalkerState.POUND:
				headTargetMode = HeadTargetMode.DEFAULT;
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				poundWeapon.Attack();
				animations.PlayAnim(poundHash, restart: true);
				break;
			case TankwalkerState.SLAP:
				headTargetMode = HeadTargetMode.DEFAULT;
				if (leftArmData.armAlive && rightArmData.armAlive)
				{
					bool flag = Random.value < 0.5f;
					animations.PlayAnim(flag ? slapHashLeft : slapHashRight, restart: true);
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerWindUp, audioSource);
				}
				else if (leftArmData.armAlive || rightArmData.armAlive)
				{
					animations.PlayAnim(leftArmData.armAlive ? slapHashLeft : slapHashRight, restart: true);
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerWindUp, audioSource);
				}
				else
				{
					animations.PlayAnim(headStabHash, restart: true);
				}
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtPlayer(instant: false);
				slapWeapon.Attack();
				navmeshMovement.SetRotationSpeed(2f);
				break;
			case TankwalkerState.HEADSTAB:
				headTargetMode = HeadTargetMode.DEFAULT;
				animations.PlayAnim(headStabHash, restart: true);
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtPlayer(instant: false);
				headStabWeapon.Attack();
				navmeshMovement.SetRotationSpeed(2f);
				break;
			case TankwalkerState.LOSE_ARM:
				navmeshMovement.StopMoving();
				headTargetMode = HeadTargetMode.DEFAULT;
				if (!leftArmData.armAlive && leftArmData.armRoot.gameObject.activeSelf)
				{
					leftArmData.armSmoke.Play();
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, ventLeftAudioSource);
					leftArmData.DeactivateAllGrindLines();
				}
				if (!rightArmData.armAlive && rightArmData.armRoot.gameObject.activeSelf)
				{
					rightArmData.armSmoke.Play();
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, ventRightAudioSource);
					rightArmData.DeactivateAllGrindLines();
				}
				navmeshMovement.LookAtCurrentDir(instant: false);
				break;
			case TankwalkerState.DIE:
			{
				headTargetMode = HeadTargetMode.DEFAULT;
				navmeshMovement.StopMoving();
				navmeshMovement.LookAtCurrentDir(instant: false);
				cockpitOverheat.Play();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, cockpitAudioSource);
				if (extensionCockpit.GSpot.lastPlayerToPaint == player)
				{
					WorldHandler.instance.AddCombatBlocker(10f);
					WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
					player.tf.position = extensionCockpit.GSpot.GetGraffitiSpotPlayerPos();
					Vector3 vector = -player.tf.forward;
					player.DoJumpEffects(vector);
					Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
					player.SetRotHard(Quaternion.LookRotation(normalized));
					float num = player.jumpSpeed * 1f;
					float num2 = player.maxMoveSpeed * 1f;
					player.SetVelocity(num * Vector3.up + normalized * num2);
					player.ActivateAbility(player.flipOutJumpAbility);
				}
				animations.PlayAnim(idleHash);
				groundImpact.Play();
				for (int i = 0; i < turretGrindLines.Length; i++)
				{
					turretGrindLines[i].SetActive(value: false);
				}
				break;
			}
			case TankwalkerState.ROTATEAROUND:
			case TankwalkerState.TANKGUN_RELOAD:
				break;
			}
		}

		internal void UpdateVent(AudioManager audioManager, Arm arm, AudioSource audioSource)
		{
			if (CheckIfShouldVent(arm.armExtension))
			{
				if (arm.PlaySteam())
				{
					AudioClip audioClipFromID = audioManager.GetAudioClipFromID(SfxCollectionID.CombatSfx, AudioClipID.VentStart);
					AudioMixerGroup audioChannel = audioManager.GetAudioChannel(AudioChannelID.Gameplay);
					audioManager.PlayOneShotSfx(audioChannel, audioClipFromID, audioSource, 0f);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, audioSource, audioClipFromID.length);
				}
			}
			else if (arm.StopSteam())
			{
				audioManager.StopLoopingSfx(audioSource);
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.VentEnd, audioSource);
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			UpdateState();
			CheckIfShouldRespondToArmExtensionHit(leftArmData);
			CheckIfShouldRespondToArmExtensionHit(rightArmData);
			UpdateVent(audioManager, leftArmData, ventLeftAudioSource);
			UpdateVent(audioManager, rightArmData, ventRightAudioSource);
			if (extensionCockpit.State == EnemyExtension.ExtensionState.OPEN && !extensionCockpit.GSpot.IsFinished() && extensionCockpit.GSpot.state != GraffitiState.PAINTING)
			{
				if (!cockpitSmoke.isPlaying)
				{
					cockpitSmoke.Play();
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, cockpitAudioSource);
				}
			}
			else if (cockpitSmoke.isPlaying)
			{
				cockpitSmoke.Stop();
				audioManager.StopLoopingSfx(cockpitAudioSource);
			}
			if (extensionCockpit.GSpot.state == GraffitiState.FINISHED && HP > 0)
			{
				ReceiveDamage(1);
			}
		}

		private void CheckIfShouldRespondToArmExtensionHit(Arm armData)
		{
			if (armData.armExtension.ReceivedHit)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, audioSource);
				armData.armExtension.OpenExtension();
				armData.armExtension.ReceivedHit = false;
				player.SetPosAndRotHard(armData.armExtension.GSpot.GetGraffitiSpotPlayerPos(), Quaternion.LookRotation((armData.armExtension.GSpot.transform.position - armData.armExtension.GSpot.GetGraffitiSpotPlayerPos()).normalized));
				player.hitBounceAbility.FreeformBounce();
			}
		}

		private bool CheckIfShouldVent(EnemyExtension extension)
		{
			if (extension.State == EnemyExtension.ExtensionState.OPEN && !extension.GSpot.IsFinished() && extension.GSpot.state != GraffitiState.PAINTING && tankwalkerState == TankwalkerState.MACHINEGUN_VENT)
			{
				return true;
			}
			return false;
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (tankwalkerState)
			{
			case TankwalkerState.IDLE:
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.MOVETOCIRCLE);
				}
				break;
			case TankwalkerState.MOVETOCIRCLE:
				MovementAnimation();
				if (enemyGrindLines.PlayerActiveOnEnemy && navmeshMovement.CurrentMovementState != EnemyNavmeshMovement.MovementState.STATIONARY)
				{
					navmeshMovement.LookAtCurrentDir(instant: true);
				}
				if (!enemyGrindLines.PlayerActiveOnEnemy && navmeshMovement.CurrentMovementState == EnemyNavmeshMovement.MovementState.STATIONARY)
				{
					navmeshMovement.LookAtPlayer(instant: false);
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !CheckIfShouldMachineGunAttack() && !CheckIfShouldMortarAttack() && !CheckIfShouldSlap() && !CheckIfShouldHeadStab())
				{
					CheckIfShouldPound();
				}
				break;
			case TankwalkerState.TANKGUN_AIM:
			{
				Vector3 practicalWorldVelocity = player.GetPracticalWorldVelocity();
				practicalWorldVelocity.y = 0f;
				float num = practicalWorldVelocity.magnitude;
				Vector3 vector = practicalWorldVelocity.normalized;
				bool flag = num <= player.minMoveSpeed;
				Vector3 vector2 = Vector3.Cross(Vector3.up, vector);
				Vector3 zero = Vector3.zero;
				if (stateTime > 0.5f && placedTargetInt < enemyTargets.MaxTargetVisuals)
				{
					if (flag)
					{
						vector = Random.insideUnitSphere;
						vector.y = 0f;
						num = 4f;
					}
					float num2 = 2f + (float)placedTargetInt * 1.5f;
					zero = vector * num * Random.Range(0.5f, 1f) + vector2 * Random.Range(0f - num2, num2) + player.tf.position;
					int layerMask = 2106385;
					if (Physics.Raycast(zero + Vector3.up * 25f, -Vector3.up, out var hitInfo, 50f, layerMask, QueryTriggerInteraction.Ignore))
					{
						enemyTargets.TargetVisuals[placedTargetInt].SetPos(hitInfo.point);
						Vector3 forward = base.transform.position - hitInfo.point;
						forward.y = 0f;
						enemyTargets.TargetVisuals[placedTargetInt].tf.rotation = Quaternion.LookRotation(forward);
						enemyTargets.TargetVisuals[placedTargetInt].SetOn();
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.sniper_lockon, enemyTargets.TargetVisuals[placedTargetInt].gameObject.GetComponent<AudioSource>());
						mortar.ClipSize++;
					}
					else
					{
						enemyTargets.TargetVisuals[placedTargetInt].SetOff();
					}
					headTargetDir = zero - headBone.position;
					headTargetDir.y = 0f;
					headTargetDir = headTargetDir.normalized;
					placedTargetInt++;
					stateTime = 0f;
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && placedTargetInt == enemyTargets.MaxTargetVisuals - 1 && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.TANKGUN_ATTACK);
				}
				break;
			}
			case TankwalkerState.TANKGUN_ATTACK:
			{
				bool flag2 = false;
				if (stateTime > 0.5f)
				{
					for (int i = 0; i < enemyTargets.MaxTargetVisuals; i++)
					{
						if (!enemyTargets.TargetVisuals[i].off)
						{
							if (!flag2)
							{
								headTargetDir = enemyTargets.TargetVisuals[i].pos - headBone.position;
								headTargetDir.y = 0f;
								headTargetDir = headTargetDir.normalized;
								flag2 = true;
							}
							if (mortar.CanShoot)
							{
								mortar.Shoot(enemyTargets.TargetVisuals[i].pos, enemyTargets.TargetVisuals[i].tf.forward, 0f);
								animations.PlayAnim(tankwalkerMortarShootHash, restart: true);
								enemyTargets.TargetVisuals[i].SetOff();
							}
						}
					}
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && mortar.Reloading && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.IDLE);
				}
				break;
			}
			case TankwalkerState.MACHINEGUN_PREPARE:
				if (enemyGrindLines.PlayerActiveOnEnemy)
				{
					navmeshMovement.LookAtCurrentDir(instant: true);
				}
				else
				{
					navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: true);
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm())
				{
					if (stateTime > 5.8f && !enemyGrindLines.PlayerActiveOnEnemy)
					{
						animations.PlayAnim(aimMachinegunsHash);
					}
					if ((double)stateTime > 5.8 && !enemyGrindLines.PlayerActiveOnEnemy)
					{
						SetState(TankwalkerState.MACHINEGUN_ATTACK);
					}
				}
				break;
			case TankwalkerState.MACHINEGUN_ATTACK:
				if (enemyGrindLines.PlayerActiveOnEnemy)
				{
					playerWasOnMe = true;
					navmeshMovement.LookAtCurrentDir(instant: true);
				}
				else
				{
					if (playerWasOnMe)
					{
						stateTime = 0f;
						playerWasOnMe = false;
					}
					if (stateTime <= 0.5f)
					{
						navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: false);
					}
					if (stateTime > 0.5f)
					{
						navmeshMovement.LookAtPosition(aiming.GetTargetPos(), instant: false);
						if ((leftMachineGun.CanShoot || rightMachineGun.CanShoot) && positioningInfo.SqrDistToPlayer < 900f)
						{
							GameplayCamera.StartScreenShake(ScreenShakeType.LIGHT, 0.15f);
						}
						if (leftMachineGun.CanShoot)
						{
							leftMachineGun.Shoot(leftMachineGun.Muzzle.position, (aiming.GetTargetPos() - leftMachineGun.Muzzle.position).normalized, 0f);
						}
						if (rightMachineGun.CanShoot)
						{
							rightMachineGun.Shoot(rightMachineGun.Muzzle.position, (aiming.GetTargetPos() - rightMachineGun.Muzzle.position).normalized, 0f);
						}
					}
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !CheckIfShouldPound())
				{
					if ((leftMachineGun.Equipped && leftMachineGun.Reloading) || (rightMachineGun.Equipped && rightMachineGun.Reloading))
					{
						SetState(TankwalkerState.MACHINEGUN_VENT);
					}
					else if (!enemyGrindLines.PlayerActiveOnEnemy && Vector3.Dot(positioningInfo.FlatDirToPlayer, tf.forward) < 0f)
					{
						SetState(TankwalkerState.IDLE);
					}
				}
				break;
			case TankwalkerState.MACHINEGUN_VENT:
				if ((double)stateTime > 2.833 && stateTime < 8.833f)
				{
					animations.PlayAnim(ventStartHash);
				}
				if (stateTime > 5.333f && (leftArmData.armExtension.State != EnemyExtension.ExtensionState.OPEN || rightArmData.armExtension.State != EnemyExtension.ExtensionState.OPEN))
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, audioSource);
					leftArmData.armExtension.OpenExtension();
					rightArmData.armExtension.OpenExtension();
				}
				if (stateTime > 8.833f || hitWhileVenting)
				{
					animations.PlayAnim(ventEndHash);
					postReloadDelay += Core.dt;
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && postReloadDelay > 1.5f && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.IDLE);
				}
				break;
			case TankwalkerState.POUND:
				if (poundWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK && !poundEffect.activeSelf)
				{
					if (positioningInfo.SqrDistToPlayer < 2500f)
					{
						GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.3f);
					}
					poundEffect.SetActive(value: true);
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !poundWeapon.Attacking && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.IDLE);
				}
				break;
			case TankwalkerState.SLAP:
				if (stateTime > 1.5f || enemyGrindLines.PlayerActiveOnEnemy)
				{
					navmeshMovement.LookAtCurrentDir(instant: true);
				}
				if (slapWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK && !slapEffect.activeSelf)
				{
					if (positioningInfo.SqrDistToPlayer < 2500f)
					{
						GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.3f);
					}
					slapEffect.SetActive(value: true);
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !slapWeapon.Attacking && stateTime > 1f && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.IDLE);
				}
				break;
			case TankwalkerState.HEADSTAB:
				if (stateTime > 1.5f || enemyGrindLines.PlayerActiveOnEnemy)
				{
					navmeshMovement.LookAtCurrentDir(instant: true);
				}
				if (headStabWeapon.State == EnemyMeleeWeapon.AttackState.ATTACK && !stabEffect.activeSelf)
				{
					if (positioningInfo.SqrDistToPlayer < 2500f)
					{
						GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.3f);
					}
					stabEffect.SetActive(value: true);
				}
				if (!CheckIfDied() && !CheckIfShouldLoseArm() && !headStabWeapon.Attacking && stateTime > 1f && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(TankwalkerState.IDLE);
				}
				break;
			case TankwalkerState.LOSE_ARM:
				if (stateTime > 4f && needToLoseArm)
				{
					needToLoseArm = false;
					if (LoseArm(leftArmData))
					{
						leftMachineGun.Equip(equip: false);
						if (!rightArmData.armAlive)
						{
							slapWeapon.Equip(equip: false);
							headStabWeapon.Equip(equip: true);
						}
						animations.PlayAnim(tankwalkerLoseLeftArmHash);
					}
					if (LoseArm(rightArmData))
					{
						rightMachineGun.Equip(equip: false);
						if (!leftArmData.armAlive)
						{
							slapWeapon.Equip(equip: false);
							headStabWeapon.Equip(equip: true);
						}
						animations.PlayAnim(tankwalkerLoseRightArmHash);
					}
					GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.5f);
				}
				if (stateTime > 7f)
				{
					if (extensionCockpit.State != EnemyExtension.ExtensionState.OPEN && !leftArmData.armAlive && !rightArmData.armAlive)
					{
						extensionCockpit.OpenExtension();
						stateTime -= 3f;
					}
					else
					{
						SetState(TankwalkerState.IDLE);
					}
				}
				break;
			case TankwalkerState.DIE:
				if (stateTime > 2f && !cockpitOverheat.isStopped)
				{
					audioManager.StopLoopingSfx(cockpitAudioSource);
					cockpitOverheat.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
					cockpitExplosion.Play();
					if (deactivateCockpitAfterExplosion)
					{
						extensionCockpit.gameObject.SetActive(value: false);
					}
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.MachineryExplosion, audioSource);
					animations.PlayAnim(tankwalkerDieHash, restart: false);
				}
				if (stateTime > durationBeforeDeath / 2f && sinkAfterDeath)
				{
					navmeshMovement.Deactivate();
					tf.position -= Vector3.up * Core.dt;
				}
				if (stateTime > durationBeforeDeath)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
				break;
			case TankwalkerState.ROTATEAROUND:
			case TankwalkerState.TANKGUN_RELOAD:
				break;
			}
		}

		private bool LoseArm(Arm armData)
		{
			if (!armData.armAlive && armData.armRoot.gameObject.activeSelf)
			{
				GameObject gameObject = new GameObject();
				gameObject.transform.position = armData.armRoot.position;
				gameObject.transform.rotation = armData.armRoot.rotation;
				MeshRenderer[] componentsInChildren = armData.armRoot.gameObject.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					GameObject gameObject2 = Object.Instantiate(componentsInChildren[i].gameObject);
					Transform[] allChildren = gameObject2.transform.GetAllChildren();
					if (allChildren.Length != 0)
					{
						for (int num = allChildren.Length - 1; num > -1; num--)
						{
							Object.Destroy(allChildren[num].gameObject);
						}
					}
					gameObject2.transform.position = componentsInChildren[i].transform.position;
					gameObject2.transform.rotation = componentsInChildren[i].transform.rotation;
					gameObject2.transform.localScale = Vector3.one;
					gameObject2.transform.parent = gameObject.transform;
					gameObject2.layer = 21;
				}
				Object.Instantiate(armData.handStandIn, armData.handStandIn.transform.position, armData.handStandIn.transform.rotation, gameObject.transform).SetActive(value: true);
				armData.hand.enabled = false;
				gameObject.AddComponent<DestroyWhenOutOfView>();
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.isKinematic = false;
				rigidbody.useGravity = true;
				rigidbody.GetComponent<Rigidbody>().AddForce(armData.armExtension.GSpot.transform.forward * 3f, ForceMode.VelocityChange);
				armData.DeactivateAllGrindLines();
				armData.armRoot.localScale = Vector3.zero;
				armData.armRoot.gameObject.SetActive(value: false);
				armData.armExplosion.Play();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.MachineryExplosion, audioSource);
				return true;
			}
			return false;
		}

		protected override void LateUpdateActive()
		{
			base.LateUpdateActive();
			if (tankwalkerState == TankwalkerState.MACHINEGUN_ATTACK)
			{
				Vector3 normalized = (aiming.GetTargetPos() - spine1Bone.position).normalized;
				float num = Vector3.Angle(Vector3.forward, spine1Bone.InverseTransformDirection(Vector3.ProjectOnPlane(normalized, -spine1Bone.right)));
				if (Vector3.Dot(positioningInfo.FlatDirToPlayer, spine1Bone.up) > 0f)
				{
					num *= -1f;
				}
				Vector3 normalized2 = (aiming.GetTargetPos() + 3.3f * Vector3.up - spine2Bone.position).normalized;
				float num2 = Mathf.Clamp(Vector3.SignedAngle(Vector3.forward, spine2Bone.InverseTransformDirection(Vector3.ProjectOnPlane(normalized2, spine2Bone.up)), spine2Bone.forward), -20f, 20f);
				if (!enemyGrindLines.PlayerActiveOnEnemy && positioningInfo.SqrFlatDistToPlayer > 16f)
				{
					if (stateTime < 0.5f)
					{
						currentSpineLocalRotation = spine1Bone.localRotation;
						spine1Bone.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((aiming.GetTargetPos() - spine1Bone.position).normalized, Vector3.up));
						spine1Bone.Rotate(0f, 0f, -90f, Space.Self);
						spine1Bone.localRotation = Quaternion.Lerp(currentSpineLocalRotation, spine1Bone.localRotation, stateTime * 2f);
						currentSpineLocalRotation = spine1Bone.localRotation;
						currentSpine2LocalRotation = spine2Bone.localRotation;
						spine2Bone.Rotate(new Vector3(0f, 0f - num2, 0f), Space.Self);
						spine2Bone.localRotation = Quaternion.Lerp(currentSpine2LocalRotation, spine2Bone.localRotation, stateTime * 2f);
						currentSpine2LocalRotation = spine2Bone.localRotation;
						shRBone.rotation = Quaternion.Lerp(currentShRRotation, Quaternion.LookRotation((aiming.GetTargetPos() - tf.right * 2.83f + tf.up * 4.6f - shRBone.position).normalized, Vector3.down), stateTime * 2f);
						shLBone.rotation = Quaternion.Lerp(currentShLRotation, Quaternion.LookRotation((aiming.GetTargetPos() + tf.right * 2.83f + tf.up * 4.6f - shLBone.position).normalized, Vector3.up), stateTime * 2f);
						currentShRRotation = shRBone.rotation;
						currentShLRotation = shLBone.rotation;
						Vector3 eulerAngles = shLBone.localRotation.eulerAngles;
						eulerAngles.z -= Mathf.Lerp(0f, 9f, stateTime * 2f);
						shLBone.localRotation = Quaternion.Euler(eulerAngles);
						Vector3 eulerAngles2 = shRBone.localRotation.eulerAngles;
						eulerAngles2.z += Mathf.Lerp(0f, 9f, stateTime * 2f);
						shRBone.localRotation = Quaternion.Euler(eulerAngles2);
						currentShRLocalRotation = shRBone.localRotation;
						currentShLLocalRotation = shLBone.localRotation;
					}
					else
					{
						spine1Bone.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((aiming.GetTargetPos() - spine1Bone.position).normalized, Vector3.up));
						spine1Bone.Rotate(0f, 0f, -90f, Space.Self);
						currentSpineLocalRotation = spine1Bone.localRotation;
						spine2Bone.Rotate(new Vector3(0f, 0f - num2, 0f), Space.Self);
						currentSpine2LocalRotation = spine2Bone.localRotation;
						shRBone.rotation = Quaternion.LookRotation((aiming.GetTargetPos() - tf.right * 2.83f + tf.up * 4.6f - shRBone.position).normalized, Vector3.down);
						shLBone.rotation = Quaternion.LookRotation((aiming.GetTargetPos() + tf.right * 2.83f + tf.up * 4.6f - shLBone.position).normalized, Vector3.up);
						Vector3 eulerAngles3 = shLBone.localRotation.eulerAngles;
						eulerAngles3.z -= 9f;
						shLBone.localRotation = Quaternion.Euler(eulerAngles3);
						Vector3 eulerAngles4 = shRBone.localRotation.eulerAngles;
						eulerAngles4.z += 9f;
						shRBone.localRotation = Quaternion.Euler(eulerAngles4);
						currentShRLocalRotation = shRBone.localRotation;
						currentShLLocalRotation = shLBone.localRotation;
					}
				}
				else
				{
					shRBone.localRotation = currentShRLocalRotation;
					shLBone.localRotation = currentShLLocalRotation;
					currentShRRotation = shRBone.rotation;
					currentShLRotation = shLBone.rotation;
					spine1Bone.localRotation = currentSpineLocalRotation;
					spine2Bone.localRotation = currentSpine2LocalRotation;
				}
			}
			if (lastTankwalkerState == TankwalkerState.MACHINEGUN_ATTACK && tankwalkerState != TankwalkerState.MACHINEGUN_ATTACK && stateTime < 1f)
			{
				spine1Bone.localRotation = Quaternion.Lerp(currentSpineLocalRotation, spine1Bone.localRotation, stateTime);
				spine2Bone.localRotation = Quaternion.Lerp(currentSpine2LocalRotation, spine2Bone.localRotation, stateTime);
				shRBone.localRotation = Quaternion.Lerp(currentShRLocalRotation, shRBone.localRotation, stateTime);
				shLBone.localRotation = Quaternion.Lerp(currentShLLocalRotation, shLBone.localRotation, stateTime);
			}
			float b = 0f;
			switch (headTargetMode)
			{
			case HeadTargetMode.DEFAULT:
				b = 0f;
				break;
			case HeadTargetMode.CUSTOM:
				if (headTargetDir.magnitude != 0f)
				{
					b = Vector3.Angle(Vector3.forward, headBone.InverseTransformDirection(Vector3.ProjectOnPlane(headTargetDir, -headBone.right)));
				}
				break;
			case HeadTargetMode.PLAYER:
				if (positioningInfo.FlatDirToPlayer != Vector3.zero && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					b = Vector3.Angle(Vector3.forward, headBone.InverseTransformDirection(Vector3.ProjectOnPlane(positioningInfo.FlatDirToPlayer, -headBone.right)));
				}
				break;
			}
			curHeadAngle = Mathf.Lerp(curHeadAngle, b, 1f - Mathf.Exp(-10f * Core.dt));
			headBone.Rotate(new Vector3(curHeadAngle, 0f, 0f), Space.Self);
		}

		private void ExitState(TankwalkerState targetState)
		{
			switch (tankwalkerState)
			{
			case TankwalkerState.MACHINEGUN_ATTACK:
				rightMachineGun.ReloadManual();
				leftMachineGun.ReloadManual();
				if (rightArmData.armAlive)
				{
					rightArmData.SwitchToLongGrindLines();
				}
				if (leftArmData.armAlive)
				{
					leftArmData.SwitchToLongGrindLines();
				}
				playerWasOnMe = false;
				break;
			case TankwalkerState.MACHINEGUN_VENT:
				postReloadDelay = 0f;
				if (leftArmData.armExtension.GSpot.state != GraffitiState.FINISHED)
				{
					leftArmData.armExtension.CloseExtension();
				}
				if (rightArmData.armExtension.GSpot.state != GraffitiState.FINISHED)
				{
					rightArmData.armExtension.CloseExtension();
				}
				break;
			case TankwalkerState.POUND:
				poundEffect.SetActive(value: false);
				break;
			case TankwalkerState.SLAP:
				slapEffect.SetActive(value: false);
				break;
			case TankwalkerState.HEADSTAB:
				stabEffect.SetActive(value: false);
				break;
			case TankwalkerState.IDLE:
			case TankwalkerState.MOVETOCIRCLE:
			case TankwalkerState.ROTATEAROUND:
			case TankwalkerState.TANKGUN_AIM:
			case TankwalkerState.TANKGUN_ATTACK:
			case TankwalkerState.TANKGUN_RELOAD:
			case TankwalkerState.MACHINEGUN_PREPARE:
				break;
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			enemyTargets.SetOffTargetVisuals();
			navmeshMovement.Deactivate();
			tankwalkerState = TankwalkerState.IDLE;
		}

		private void MovementAnimation()
		{
			if (navmeshMovement.GetVelocity().sqrMagnitude > 0f)
			{
				animations.PlayAnim(walkHash);
				Vector3 localMovementDirection = navmeshMovement.GetLocalMovementDirection();
				animations.SetFloatParameter(moveXHash, Mathf.Lerp(animations.GetFloatParameter(moveXHash), localMovementDirection.x, 1f - Mathf.Exp(-10f * Core.dt)));
				animations.SetFloatParameter(moveYHash, Mathf.Lerp(animations.GetFloatParameter(moveYHash), localMovementDirection.z, 1f - Mathf.Exp(-10f * Core.dt)));
			}
			else
			{
				animations.PlayAnim(idleHash);
			}
		}

		private void OnGSpotStateChanged(GraffitiSpotFinisher gSpot, GraffitiState result)
		{
			switch (result)
			{
			case GraffitiState.PAINTING:
				WorldHandler.instance.AddCombatBlocker(5f);
				if (rightArmData.armExtension.GSpot == gSpot && rightArmData.StopSteam())
				{
					audioManager.StopLoopingSfx(ventRightAudioSource);
				}
				if (leftArmData.armExtension.GSpot == gSpot && leftArmData.StopSteam())
				{
					audioManager.StopLoopingSfx(ventLeftAudioSource);
				}
				if (extensionCockpit.GSpot == gSpot && cockpitSmoke.isPlaying)
				{
					cockpitSmoke.Stop();
					audioManager.StopLoopingSfx(cockpitAudioSource);
				}
				break;
			case GraffitiState.CANCEL:
				WorldHandler.instance.AddCombatBlocker(5f);
				if (rightArmData.armExtension.GSpot == gSpot && rightArmData.armExtension.State == EnemyExtension.ExtensionState.OPEN && rightArmData.PlaySteam())
				{
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, ventRightAudioSource);
				}
				if (leftArmData.armExtension.GSpot == gSpot && leftArmData.armExtension.State == EnemyExtension.ExtensionState.OPEN && leftArmData.PlaySteam())
				{
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, ventLeftAudioSource);
				}
				if (extensionCockpit.State == EnemyExtension.ExtensionState.OPEN && extensionCockpit.GSpot == gSpot)
				{
					cockpitSmoke.Play();
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, cockpitAudioSource);
				}
				break;
			case GraffitiState.FINISHED:
				if (gSpot.lastPlayerToPaint == player)
				{
					player.SetPosAndRotHard(gSpot.GetGraffitiSpotPlayerPos(), Quaternion.LookRotation((gSpot.transform.position - gSpot.GetGraffitiSpotPlayerPos()).normalized));
					player.hitBounceAbility.FreeformBounce();
				}
				hitWhileVenting = true;
				if (gSpot == leftArmData.armExtension.GSpot)
				{
					leftArmData.armAlive = false;
					needToLoseArm = true;
				}
				if (gSpot == rightArmData.armExtension.GSpot)
				{
					rightArmData.armAlive = false;
					needToLoseArm = true;
				}
				break;
			}
		}

		private bool CheckIfShouldPound()
		{
			bool flag = poundWeapon.CanAttack && poundWeapon.IsInRangeFlat();
			if (flag)
			{
				poundActivateTimer += Core.dt;
				if (poundActivateTimer > 0.5f && player.IsGrounded())
				{
					RequestAttack(6f);
					if (receivedAttackPermission)
					{
						SetState(TankwalkerState.POUND);
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
			else
			{
				poundActivateTimer = 0f;
			}
			return flag;
		}

		private bool CheckIfShouldSlap()
		{
			bool flag = !enemyGrindLines.PlayerActiveOnEnemy && slapWeapon.CanAttack && slapWeapon.IsInRangeFlat() && positioningInfo.DiffToPlayer.y < 2f;
			if (flag)
			{
				RequestAttack(6f);
				if (receivedAttackPermission)
				{
					SetState(TankwalkerState.SLAP);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool CheckIfShouldHeadStab()
		{
			bool flag = !enemyGrindLines.PlayerActiveOnEnemy && headStabWeapon.CanAttack && headStabWeapon.IsInRangeFlat() && positioningInfo.DiffToPlayer.y < 2f;
			if (flag)
			{
				RequestAttack(6f);
				if (receivedAttackPermission)
				{
					SetState(TankwalkerState.HEADSTAB);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool CheckIfShouldMachineGunAttack()
		{
			bool flag = !enemyGrindLines.PlayerActiveOnEnemy && (leftMachineGun.CanShoot || rightMachineGun.CanShoot) && leftMachineGun.IsInRangeFlat();
			if (flag)
			{
				RequestAttack(11f);
				if (receivedAttackPermission)
				{
					SetState(TankwalkerState.MACHINEGUN_PREPARE);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool CheckIfShouldMortarAttack()
		{
			bool flag = mortar.CanShoot && mortar.IsInRangeFlat();
			if (flag)
			{
				RequestAttack(4f);
				if (receivedAttackPermission)
				{
					SetState(TankwalkerState.TANKGUN_AIM);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private bool CheckIfDied()
		{
			bool num = HP <= 0;
			if (num)
			{
				SetState(TankwalkerState.DIE);
			}
			return num;
		}

		private bool CheckIfShouldLoseArm()
		{
			bool num = needToLoseArm;
			if (num)
			{
				SetState(TankwalkerState.LOSE_ARM);
			}
			return num;
		}

		public override bool IsDead()
		{
			if (tankwalkerState == TankwalkerState.DIE)
			{
				return stateTime > durationBeforeDeath / 2f;
			}
			return false;
		}
	}
}
