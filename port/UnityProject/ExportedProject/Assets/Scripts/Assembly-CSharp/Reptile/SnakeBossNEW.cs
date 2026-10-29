using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class SnakeBossNEW : BasicEnemy
	{
		public enum SnakeBossState
		{
			IDLE = 0,
			RAILRIDER = 1,
			MACHINEGUN_PREPARE = 2,
			MACHINEGUN_ATTACK = 3,
			VENT = 4,
			TANKGUN_AIM = 5,
			TANKGUN_ATTACK = 6,
			LOSE_ARM = 7,
			DIE = 8
		}

		public enum ChestState
		{
			CLOSED = 0,
			PEAKING = 1,
			OPEN = 2
		}

		[SerializeField]
		private EnemyAnimations animations;

		[SerializeField]
		private EnemyGrindLines enemyGrindLines;

		[SerializeField]
		private EnemyExtension extensionCockpit;

		[SerializeField]
		private EnemyTargets enemyTargets;

		[SerializeField]
		private EnemyFirearm mortar;

		[SerializeField]
		private Transform headBone;

		[SerializeField]
		private Transform cannonBone;

		[SerializeField]
		private Transform upperSpineBone;

		[SerializeField]
		private Transform shLBone01;

		[SerializeField]
		private Transform shRBone01;

		[SerializeField]
		private Transform shLBone02;

		[SerializeField]
		private Transform shRBone02;

		[SerializeField]
		private Transform shLBone03;

		[SerializeField]
		private Transform shRBone03;

		[SerializeField]
		private EnemyFirearm leftMachineGun;

		[SerializeField]
		private EnemyFirearm rightMachineGun;

		[SerializeField]
		private EnemyFirearm leftMachineGun2;

		[SerializeField]
		private EnemyFirearm rightMachineGun2;

		[SerializeField]
		private EnemyFirearm leftMachineGun3;

		[SerializeField]
		private EnemyFirearm rightMachineGun3;

		[SerializeField]
		private EnemyTargetabilityAiming aiming;

		[SerializeField]
		private GameObject[] grindLines;

		[SerializeField]
		private GrindPath path;

		[SerializeField]
		private GrindPath path2;

		[SerializeField]
		private RailRiderLauncher launcherRight3;

		[SerializeField]
		private RailRiderLauncher launcherLeft3;

		[SerializeField]
		private RailRiderLauncher launcherRight2;

		[SerializeField]
		private RailRiderLauncher launcherLeft2;

		[SerializeField]
		private RailRiderLauncher launcherRight1;

		[SerializeField]
		private RailRiderLauncher launcherLeft1;

		[SerializeField]
		private Arm leftArm1Data;

		[SerializeField]
		private Arm leftArm2Data;

		[SerializeField]
		private Arm leftArm3Data;

		[SerializeField]
		private Arm rightArm1Data;

		[SerializeField]
		private Arm rightArm2Data;

		[SerializeField]
		private Arm rightArm3Data;

		[SerializeField]
		private GameObject chestCollider;

		[SerializeField]
		private ParticleSystem chestSteam;

		[SerializeField]
		private SnakeBossChestImpactReceiver chestImpactReceiver;

		[SerializeField]
		private PlayableDirector chestOpenSequence;

		[SerializeField]
		private ParticleSystem[] highCableEffects;

		[SerializeField]
		private ParticleSystem[] midCableEffects;

		[SerializeField]
		private ParticleSystem[] lowCableEffects;

		[SerializeField]
		private MusicTrack customMusicTrack;

		[SerializeField]
		private AudioSource[] audioSources;

		[SerializeField]
		private Transform rightMiddleLauncher;

		[SerializeField]
		private Transform leftMiddleLauncher;

		[SerializeField]
		private Transform rightMiddleLauncherAlt;

		[SerializeField]
		private Transform leftMiddleLauncherAlt;

		[SerializeField]
		private Transform rightLowerLauncher;

		[SerializeField]
		private Transform leftLowerLauncher;

		[SerializeField]
		private Transform rightLowerLauncherAlt;

		[SerializeField]
		private Transform leftLowerLauncherAlt;

		[SerializeField]
		private SnakeBossAnimationEventRelay animationEventRelay;

		private int curAudioSourceIndex;

		private SnakeBossState snakeBossState;

		private SnakeBossState lastSnakeBossState;

		private float stateTime;

		private ChestState pastChestState;

		private ChestState chestState;

		private int idleHash = Animator.StringToHash("idle");

		private int idleNoArmsHash = Animator.StringToHash("idleNoArms");

		private int startMortarHash = Animator.StringToHash("startMortar");

		private int mortarShootHash = Animator.StringToHash("mortarShoot");

		private int stopMortarHash = Animator.StringToHash("stopMortar");

		private int prepareMachineGunsHash = Animator.StringToHash("MachineGunPrepare");

		private int aimMachineGunsHash = Animator.StringToHash("MachineGunAim");

		private int machineGunVentHash = Animator.StringToHash("MachineGunToVent");

		private int machineGunVentIdleHash = Animator.StringToHash("MachineGunVenting");

		private int ventEndHash = Animator.StringToHash("MachineGunVentEnd");

		private int machineGunStopHash = Animator.StringToHash("MachineGunStop");

		private int loseArmRecoilLowLeftHash = Animator.StringToHash("LoseArmRecoilLowLeft");

		private int loseArmRecoilLowRightHash = Animator.StringToHash("LoseArmRecoilLowRight");

		private int loseArmRecoilMidLeftHash = Animator.StringToHash("LoseArmRecoilMidLeft");

		private int loseArmRecoilMidRightHash = Animator.StringToHash("LoseArmRecoilMidRight");

		private int loseArmRecoilHighLeftHash = Animator.StringToHash("LoseArmRecoilHighLeft");

		private int loseArmRecoilHighRightHash = Animator.StringToHash("LoseArmRecoilHighRight");

		private int railRidersHighStart = Animator.StringToHash("RailRidersHighStart");

		private int railRidersLowStart = Animator.StringToHash("RailRidersLowStart");

		private int railRidersMidStart = Animator.StringToHash("RailRidersMidStart");

		private int railRidersHighStopHash = Animator.StringToHash("RailRidersHighStop");

		private int railRidersMidStopHash = Animator.StringToHash("RailRidersMidStop");

		private int railRidersLowStopHash = Animator.StringToHash("RailRidersLowStop");

		private int railRidersHighVentStartHash = Animator.StringToHash("RailRidersHighToVent");

		private int railRidersHighVentEndHash = Animator.StringToHash("RailRidersHighVentEnd");

		private int railRidersMidVentStartHash = Animator.StringToHash("RailRidersMidToVent");

		private int railRidersMidVentEndHash = Animator.StringToHash("RailRidersMidVentEnd");

		private int railRidersLowVentStartHash = Animator.StringToHash("RailRidersLowToVent");

		private int railRidersLowVentEndHash = Animator.StringToHash("RailRidersLowVentEnd");

		private int dieHash = Animator.StringToHash("die");

		private int mortarAimDirHash = Animator.StringToHash("mortarAimDir");

		private int chestCloseHash = Animator.StringToHash("chestClose");

		private int chestOpenHash = Animator.StringToHash("chestOpen");

		private int chestPeakHash = Animator.StringToHash("chestPeak");

		private Transform[] missileLocations;

		private GameObject teleporterLow;

		private GameObject teleporterHigh;

		private int railRiderAttackToUse = -1;

		private Vector3 lastHeadTargetDir;

		private Vector3 headTargetDir;

		private float timeSet;

		private bool stopVenting;

		private Vector3 originalForward;

		private int armWithSmallestDist = -1;

		private int armWithSecondSmallestDist = -1;

		private int placedTargetInt;

		private bool needToLoseArm;

		private bool setNextTargetDir;

		private float aimBlend1;

		private float aimBlend2;

		private float aimBlend3;

		private float aimBlend4;

		private float aimBlend5;

		private float aimBlend6;

		private int firstMissileLocation;

		private Quaternion targetCannonRotation = Quaternion.identity;

		private MusicTrack customMusicTrackInstance;

		private AudioManager audioManager;

		private AudioSource curVentAudioSourceLeftArm1;

		private AudioSource curVentAudioSourceLeftArm2;

		private AudioSource curVentAudioSourceLeftArm3;

		private AudioSource curVentAudioSourceRightArm1;

		private AudioSource curVentAudioSourceRightArm2;

		private AudioSource curVentAudioSourceRightArm3;

		private Vector3 lastDirToPlayer;

		private float angle;

		private int lastAttackType = -1;

		private float overallLookAtWeight;

		protected override void Setup()
		{
			base.Setup();
			animations.Init(this);
			launcherRight3.Init(this, positioningInfo);
			launcherLeft3.Init(this, positioningInfo);
			launcherRight2.Init(this, positioningInfo);
			launcherLeft2.Init(this, positioningInfo);
			launcherRight1.Init(this, positioningInfo);
			launcherLeft1.Init(this, positioningInfo);
			enemyGrindLines.Init();
			enemyTargets.Init();
			mortar.Init(this, positioningInfo);
			leftArm1Data.armExtension.Init(this, player);
			leftArm1Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			leftArm2Data.armExtension.Init(this, player);
			leftArm2Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			leftArm3Data.armExtension.Init(this, player);
			leftArm3Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			rightArm1Data.armExtension.Init(this, player);
			rightArm1Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			rightArm2Data.armExtension.Init(this, player);
			rightArm2Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			rightArm3Data.armExtension.Init(this, player);
			rightArm3Data.armExtension.onGSpotStateChanged.AddListener(OnGSpotStateChanged);
			leftMachineGun.Init(this, positioningInfo);
			rightMachineGun.Init(this, positioningInfo);
			leftMachineGun2.Init(this, positioningInfo);
			rightMachineGun2.Init(this, positioningInfo);
			leftMachineGun3.Init(this, positioningInfo);
			rightMachineGun3.Init(this, positioningInfo);
			aiming.Init(this, player);
			animationEventRelay.Init(this);
			GameObject[] array = GameObject.FindGameObjectsWithTag("EnemyMovementTargetA");
			missileLocations = new Transform[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				missileLocations[i] = array[i].transform;
			}
			originalForward = tf.forward;
			teleporterHigh = GameObject.Find("TeleporterHigh");
			teleporterLow = GameObject.Find("TeleporterLow");
			GameObject gameObject = GameObject.Find("InnerGrind");
			path = gameObject.GetComponent<GrindPath>();
			gameObject = GameObject.Find("OuterGrind");
			path2 = gameObject.GetComponent<GrindPath>();
			audioManager = Core.Instance.AudioManager;
			curVentAudioSourceLeftArm1 = (curVentAudioSourceLeftArm2 = (curVentAudioSourceLeftArm3 = (curVentAudioSourceRightArm1 = (curVentAudioSourceRightArm2 = (curVentAudioSourceRightArm3 = audioSources[0])))));
		}

		private void OnDisable()
		{
			Object.Destroy(customMusicTrackInstance);
		}

		protected override void EnterSpawningState()
		{
			base.EnterSpawningState();
			leftArm1Data.ResetArm();
			leftArm2Data.ResetArm();
			leftArm3Data.ResetArm();
			rightArm1Data.ResetArm();
			rightArm2Data.ResetArm();
			rightArm3Data.ResetArm();
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
			HP = fullHP;
			SwitchAllArmsToLongGrinds();
			SetState(SnakeBossState.IDLE);
			pastChestState = ChestState.OPEN;
			chestState = ChestState.CLOSED;
			chestImpactReceiver.WasHit = false;
			teleporterHigh.SetActive(value: false);
			teleporterLow.SetActive(value: true);
			overallLookAtWeight = 0f;
			for (int i = 0; i < lowCableEffects.Length; i++)
			{
				lowCableEffects[i].Stop();
			}
			for (int j = 0; j < midCableEffects.Length; j++)
			{
				midCableEffects[j].Stop();
			}
			for (int k = 0; k < highCableEffects.Length; k++)
			{
				highCableEffects[k].Stop();
			}
			targetCannonRotation = cannonBone.localRotation;
		}

		private void SetState(SnakeBossState targetState)
		{
			lastSnakeBossState = snakeBossState;
			ExitState(snakeBossState);
			snakeBossState = targetState;
			stateTime = 0f;
			switch (snakeBossState)
			{
			case SnakeBossState.IDLE:
				animations.PlayAnim(idleHash);
				break;
			case SnakeBossState.RAILRIDER:
				if (railRiderAttackToUse == 0 || railRiderAttackToUse == 4)
				{
					animations.PlayAnim(railRidersLowStart, restart: true);
					for (int k = 0; k < lowCableEffects.Length; k++)
					{
						lowCableEffects[k].Play();
					}
				}
				if (railRiderAttackToUse == 1 || railRiderAttackToUse == 5)
				{
					animations.PlayAnim(railRidersMidStart, restart: true);
					for (int l = 0; l < midCableEffects.Length; l++)
					{
						midCableEffects[l].Play();
					}
				}
				if (railRiderAttackToUse == 2 || railRiderAttackToUse == 6)
				{
					animations.PlayAnim(railRidersHighStart, restart: true);
					for (int m = 0; m < highCableEffects.Length; m++)
					{
						highCableEffects[m].Play();
					}
				}
				DeactivateAllArmGrinds();
				break;
			case SnakeBossState.MACHINEGUN_PREPARE:
				animations.PlayAnim(prepareMachineGunsHash);
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(leftArm1Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(leftArm2Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(leftArm3Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(rightArm1Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(rightArm2Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.FinalBossGunStart, GetAudioSourceAtLocation(rightArm3Data.armRoot.position));
				DeactivateAllArmGrinds();
				break;
			case SnakeBossState.MACHINEGUN_ATTACK:
				animations.PlayAnim(aimMachineGunsHash);
				DeactivateAllArmGrinds();
				break;
			case SnakeBossState.VENT:
				leftMachineGun.ReloadManual();
				leftMachineGun2.ReloadManual();
				leftMachineGun3.ReloadManual();
				rightMachineGun.ReloadManual();
				rightMachineGun2.ReloadManual();
				rightMachineGun3.ReloadManual();
				launcherLeft1.ReloadManual();
				launcherLeft2.ReloadManual();
				launcherLeft3.ReloadManual();
				launcherRight1.ReloadManual();
				launcherRight2.ReloadManual();
				launcherRight3.ReloadManual();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(leftArm1Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(leftArm2Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(leftArm3Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(rightArm1Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(rightArm2Data.armRoot.position));
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.TankWalkerGunAway, GetAudioSourceAtLocation(rightArm3Data.armRoot.position));
				if (lastSnakeBossState == SnakeBossState.RAILRIDER)
				{
					if (railRiderAttackToUse == 0 || railRiderAttackToUse == 4)
					{
						animations.PlayAnim(railRidersLowVentStartHash);
					}
					if (railRiderAttackToUse == 1 || railRiderAttackToUse == 5)
					{
						animations.PlayAnim(railRidersMidVentStartHash);
					}
					if (railRiderAttackToUse == 2 || railRiderAttackToUse == 6)
					{
						animations.PlayAnim(railRidersHighVentStartHash);
					}
				}
				else
				{
					animations.PlayAnim(machineGunVentHash);
				}
				DeactivateAllArmGrinds();
				stopVenting = false;
				break;
			case SnakeBossState.TANKGUN_AIM:
			{
				animations.PlayAnim(startMortarHash);
				animations.SetFloatParameter(mortarAimDirHash, 0f);
				placedTargetInt = 0;
				mortar.ClipSize = 0;
				float num3 = float.MaxValue;
				int num4 = 0;
				for (int j = 0; j < missileLocations.Length; j++)
				{
					float sqrMagnitude = (player.tf.position - missileLocations[j].position).sqrMagnitude;
					if (sqrMagnitude < num3)
					{
						num4 = j;
						num3 = sqrMagnitude;
					}
				}
				firstMissileLocation = num4;
				break;
			}
			case SnakeBossState.TANKGUN_ATTACK:
			{
				setNextTargetDir = false;
				for (int i = 0; i < enemyTargets.MaxTargetVisuals; i++)
				{
					if (!enemyTargets.TargetVisuals[i].off && !setNextTargetDir)
					{
						Vector3 pos = enemyTargets.TargetVisuals[i].pos;
						pos.y = 0f;
						Vector3 position = tf.position;
						position.y = 0f;
						Vector3 eulerAngles = Quaternion.LookRotation((pos - position).normalized).eulerAngles;
						eulerAngles.y -= 90f;
						eulerAngles.y = Mathf.RoundToInt(eulerAngles.y / 90f) * 90;
						if (eulerAngles.y < 0f)
						{
							eulerAngles.y += 360f;
						}
						headTargetDir = eulerAngles;
						setNextTargetDir = true;
						timeSet = Time.time;
					}
				}
				break;
			}
			case SnakeBossState.LOSE_ARM:
				switch (lastSnakeBossState)
				{
				case SnakeBossState.RAILRIDER:
					if (railRiderAttackToUse == 0 || railRiderAttackToUse == 4)
					{
						animations.PlayAnim(railRidersLowStopHash);
					}
					if (railRiderAttackToUse == 1 || railRiderAttackToUse == 5)
					{
						animations.PlayAnim(railRidersMidStopHash);
					}
					if (railRiderAttackToUse == 2 || railRiderAttackToUse == 6)
					{
						animations.PlayAnim(railRidersHighStopHash);
					}
					break;
				case SnakeBossState.IDLE:
					animations.PlayAnim(idleHash);
					stateTime = 6f;
					break;
				case SnakeBossState.VENT:
					stateTime = 6f;
					break;
				case SnakeBossState.TANKGUN_ATTACK:
					animations.PlayAnim(stopMortarHash);
					stateTime = 3f;
					break;
				case SnakeBossState.TANKGUN_AIM:
					animations.PlayAnim(stopMortarHash);
					stateTime = 3f;
					break;
				case SnakeBossState.MACHINEGUN_PREPARE:
					animations.PlayAnim(machineGunStopHash);
					break;
				case SnakeBossState.MACHINEGUN_ATTACK:
					animations.PlayAnim(machineGunStopHash);
					break;
				case SnakeBossState.LOSE_ARM:
					animations.PlayAnim(idleHash);
					stateTime = 6f;
					break;
				}
				break;
			case SnakeBossState.DIE:
			{
				animations.PlayAnim(dieHash);
				WorldHandler.instance.AddCombatBlocker(10f);
				WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
				Vector3 vector = -player.tf.forward;
				player.DoJumpEffects(vector);
				Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
				player.SetRotHard(Quaternion.LookRotation(normalized));
				float num = player.jumpSpeed * 1f;
				float num2 = player.maxMoveSpeed * 1f;
				player.SetVelocity(num * Vector3.up + normalized * num2);
				player.ActivateAbility(player.flipOutJumpAbility);
				break;
			}
			}
		}

		protected override void UpdateActive()
		{
			base.UpdateActive();
			Vector3 flatDirToPlayer = positioningInfo.FlatDirToPlayer;
			angle = Vector3.SignedAngle(lastDirToPlayer, flatDirToPlayer, Vector3.up);
			lastDirToPlayer = flatDirToPlayer;
			if (positioningInfo.DiffToPlayer.y > 37f && player.IsGrinding())
			{
				teleporterHigh.SetActive(value: true);
				teleporterLow.SetActive(value: false);
			}
			else if (positioningInfo.DiffToPlayer.y <= 37f && player.IsGrinding())
			{
				teleporterHigh.SetActive(value: false);
				teleporterLow.SetActive(value: true);
			}
			CheckIfShouldRespondToArmExtensionHit(leftArm1Data);
			CheckIfShouldRespondToArmExtensionHit(leftArm2Data);
			CheckIfShouldRespondToArmExtensionHit(leftArm3Data);
			CheckIfShouldRespondToArmExtensionHit(rightArm1Data);
			CheckIfShouldRespondToArmExtensionHit(rightArm2Data);
			CheckIfShouldRespondToArmExtensionHit(rightArm3Data);
			if (chestImpactReceiver.WasHit && HP > 0)
			{
				HP = 0;
				player.DoHitEffectPosition(headBone.position);
			}
			Vector3 vector = player.tf.position - headBone.position;
			float num = Mathf.Abs(vector.y);
			vector.y = 0f;
			int num2;
			if (snakeBossState != SnakeBossState.IDLE && snakeBossState != SnakeBossState.MACHINEGUN_PREPARE)
			{
				num2 = ((snakeBossState == SnakeBossState.MACHINEGUN_ATTACK) ? 1 : 0);
				if (num2 == 0)
				{
					goto IL_01bc;
				}
			}
			else
			{
				num2 = 1;
			}
			if (vector.magnitude > 6f && !enemyGrindLines.PlayerActiveOnEnemy && num < 10f && chestState == ChestState.CLOSED)
			{
				pastChestState = chestState;
				chestState = ChestState.PEAKING;
			}
			goto IL_01bc;
			IL_01bc:
			if ((num2 == 0 || vector.magnitude < 5f || (!enemyGrindLines.PlayerActiveOnEnemy && num > 15f)) && chestState == ChestState.PEAKING)
			{
				pastChestState = chestState;
				chestState = ChestState.CLOSED;
			}
			if (CheckIfShouldVent(leftArm1Data.armExtension))
			{
				if (leftArm1Data.PlaySteam())
				{
					curVentAudioSourceLeftArm1 = GetAudioSourceAtLocation(leftArm1Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceLeftArm1);
				}
			}
			else if (leftArm1Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceLeftArm1);
			}
			if (CheckIfShouldVent(leftArm2Data.armExtension))
			{
				if (leftArm2Data.PlaySteam())
				{
					curVentAudioSourceLeftArm2 = GetAudioSourceAtLocation(leftArm2Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceLeftArm2);
				}
			}
			else if (leftArm2Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceLeftArm2);
			}
			if (CheckIfShouldVent(leftArm3Data.armExtension))
			{
				if (leftArm3Data.PlaySteam())
				{
					curVentAudioSourceLeftArm3 = GetAudioSourceAtLocation(leftArm3Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceLeftArm3);
				}
			}
			else if (leftArm3Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceLeftArm3);
			}
			if (CheckIfShouldVent(rightArm1Data.armExtension))
			{
				if (rightArm1Data.PlaySteam())
				{
					curVentAudioSourceRightArm1 = GetAudioSourceAtLocation(rightArm1Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceRightArm1);
				}
			}
			else if (rightArm1Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceRightArm1);
			}
			if (CheckIfShouldVent(rightArm2Data.armExtension))
			{
				if (rightArm2Data.PlaySteam())
				{
					curVentAudioSourceRightArm2 = GetAudioSourceAtLocation(rightArm2Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceRightArm2);
				}
			}
			else if (rightArm2Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceRightArm2);
			}
			if (CheckIfShouldVent(rightArm3Data.armExtension))
			{
				if (rightArm3Data.PlaySteam())
				{
					curVentAudioSourceRightArm3 = GetAudioSourceAtLocation(rightArm3Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.VentLoop, curVentAudioSourceRightArm3);
				}
			}
			else if (rightArm3Data.StopSteam())
			{
				audioManager.StopLoopingSfx(curVentAudioSourceRightArm3);
			}
			switch (chestState)
			{
			case ChestState.CLOSED:
				if (pastChestState != chestState)
				{
					animations.PlayAnim(chestCloseHash);
					pastChestState = chestState;
				}
				if (chestCollider.activeSelf)
				{
					chestCollider.SetActive(value: false);
				}
				if (chestSteam.isPlaying)
				{
					chestSteam.Stop();
				}
				break;
			case ChestState.OPEN:
				if (!chestCollider.activeSelf)
				{
					chestCollider.SetActive(value: true);
				}
				if (!chestSteam.isPlaying)
				{
					chestSteam.Play();
				}
				if (pastChestState != chestState)
				{
					animations.PlayAnim(chestOpenHash);
					SequenceHandler.instance.StartEnteringSequence(chestOpenSequence, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true, setPausePlayer: true);
					pastChestState = chestState;
				}
				break;
			case ChestState.PEAKING:
				if (pastChestState != chestState)
				{
					animations.PlayAnim(chestPeakHash);
					pastChestState = chestState;
				}
				if (chestCollider.activeSelf)
				{
					chestCollider.SetActive(value: false);
				}
				if (chestSteam.isPlaying)
				{
					chestSteam.Stop();
				}
				break;
			}
			if (!SequenceHandler.instance.IsInSequence())
			{
				UpdateState();
			}
			if (enemyGrindLines.PlayerActiveOnEnemy)
			{
				leftArm1Data.SetGrindLineRadiuses(1f);
				leftArm2Data.SetGrindLineRadiuses(1f);
				leftArm3Data.SetGrindLineRadiuses(1f);
				rightArm1Data.SetGrindLineRadiuses(1f);
				rightArm2Data.SetGrindLineRadiuses(1f);
				rightArm3Data.SetGrindLineRadiuses(1f);
			}
			else
			{
				leftArm1Data.SetGrindLineRadiuses(2f);
				leftArm2Data.SetGrindLineRadiuses(2f);
				leftArm3Data.SetGrindLineRadiuses(2f);
				rightArm1Data.SetGrindLineRadiuses(2f);
				rightArm2Data.SetGrindLineRadiuses(2f);
				rightArm3Data.SetGrindLineRadiuses(2f);
				rightArm3Data.SetGrindLineRadiuses(2f);
			}
		}

		private void CheckIfShouldRespondToArmExtensionHit(Arm armData)
		{
			if (armData.armExtension.ReceivedHit)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(armData.armRoot.position));
				armData.armExtension.OpenExtension();
				armData.armExtension.ReceivedHit = false;
				player.SetPosAndRotHard(armData.armExtension.GSpot.GetGraffitiSpotPlayerPos(), Quaternion.LookRotation((armData.armExtension.GSpot.transform.position - armData.armExtension.GSpot.GetGraffitiSpotPlayerPos()).normalized));
				player.hitBounceAbility.FreeformBounce();
			}
		}

		private bool CheckIfShouldVent(EnemyExtension extension)
		{
			if (extension.State == EnemyExtension.ExtensionState.OPEN && !extension.GSpot.IsFinished() && extension.GSpot.state != GraffitiState.PAINTING && snakeBossState == SnakeBossState.VENT)
			{
				return true;
			}
			return false;
		}

		private void UpdateState()
		{
			stateTime += Core.dt;
			switch (snakeBossState)
			{
			case SnakeBossState.IDLE:
				if (!enemyGrindLines.PlayerActiveOnEnemy)
				{
					float t = positioningInfo.DiffToPlayer.y / 60f;
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(positioningInfo.FlatDirToPlayer), Core.dt / Mathf.Lerp(6f, 1f, t));
				}
				if (CheckIfDied())
				{
					break;
				}
				if (needToLoseArm)
				{
					SetState(SnakeBossState.LOSE_ARM);
				}
				else
				{
					if (!player.IsGrinding() || !(stateTime > 4f) || enemyGrindLines.PlayerActiveOnEnemy)
					{
						break;
					}
					if (Random.value > 0.65f && lastAttackType != 1 && (leftMachineGun3.CanShoot || leftMachineGun2.CanShoot || leftMachineGun.CanShoot || rightMachineGun3.CanShoot || rightMachineGun2.CanShoot || rightMachineGun.CanShoot))
					{
						lastAttackType = 1;
						SetState(SnakeBossState.MACHINEGUN_PREPARE);
						break;
					}
					if (Random.value > 0.1f && lastAttackType != 2)
					{
						if (player.GetVelocity().y > 0f)
						{
							if (positioningInfo.DiffToPlayer.y < 22f && Mathf.Abs(Vector3.Dot(Vector3.right, positioningInfo.FlatDirToPlayer)) > 0.5f && (leftArm1Data.armAlive || rightArm1Data.armAlive))
							{
								lastAttackType = 2;
								railRiderAttackToUse = 0;
								SetState(SnakeBossState.RAILRIDER);
								break;
							}
							if (positioningInfo.DiffToPlayer.y < 32f && (leftArm2Data.armAlive || rightArm2Data.armAlive))
							{
								lastAttackType = 2;
								railRiderAttackToUse = 1;
								SetState(SnakeBossState.RAILRIDER);
								break;
							}
							if (positioningInfo.DiffToPlayer.y < 60f && (leftArm3Data.armAlive || rightArm3Data.armAlive))
							{
								lastAttackType = 2;
								railRiderAttackToUse = 2;
								SetState(SnakeBossState.RAILRIDER);
								break;
							}
						}
						else
						{
							if (positioningInfo.DiffToPlayer.y > 32f && positioningInfo.DiffToPlayer.y < 50f && Mathf.Abs(Vector3.Dot(Vector3.right, positioningInfo.FlatDirToPlayer)) > 0.5f && (leftArm1Data.armAlive || rightArm1Data.armAlive))
							{
								lastAttackType = 2;
								railRiderAttackToUse = 4;
								SetState(SnakeBossState.RAILRIDER);
								break;
							}
							if (positioningInfo.DiffToPlayer.y > 50f && (leftArm2Data.armAlive || rightArm2Data.armAlive))
							{
								lastAttackType = 2;
								railRiderAttackToUse = 5;
								SetState(SnakeBossState.RAILRIDER);
								break;
							}
						}
					}
					if (mortar.CanShoot && Random.value > 0.5f)
					{
						lastAttackType = 3;
						SetState(SnakeBossState.TANKGUN_AIM);
					}
				}
				break;
			case SnakeBossState.RAILRIDER:
				if (stateTime > 5f)
				{
					SwitchAllArmsToLongGrinds();
					switch (railRiderAttackToUse)
					{
					case 0:
						if (positioningInfo.DiffToPlayer.y > 28f && player.GetVelocity().y > 0f)
						{
							railRiderAttackToUse = 4;
							break;
						}
						launcherRight1.SetMuzzle(rightLowerLauncher);
						launcherRight1.SetGrindPath((launcherRight1.ShotsFired % 2 == 0) ? path : path2);
						launcherRight1.Shoot(launcherRight1.Muzzle.forward, 0f);
						launcherLeft1.SetMuzzle(leftLowerLauncher);
						launcherLeft1.SetGrindPath((launcherLeft1.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft1.Shoot(launcherLeft1.Muzzle.forward, 0f);
						break;
					case 1:
						if (positioningInfo.DiffToPlayer.y > 50f && player.GetVelocity().y > 0f)
						{
							railRiderAttackToUse = 5;
							break;
						}
						launcherRight2.SetMuzzle(rightMiddleLauncherAlt);
						launcherRight2.SetGrindPath((launcherRight2.ShotsFired % 2 == 0) ? path : path2);
						launcherRight2.Shoot(launcherRight2.Muzzle.forward, 0f);
						launcherLeft2.SetMuzzle(leftMiddleLauncherAlt);
						launcherLeft2.SetGrindPath((launcherLeft2.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft2.Shoot(launcherLeft2.Muzzle.forward, 0f);
						break;
					case 2:
						launcherRight3.SetGrindPath((launcherRight3.ShotsFired % 2 == 0) ? path : path2);
						launcherRight3.Shoot(launcherRight3.Muzzle.forward, 0f);
						launcherLeft3.SetGrindPath((launcherLeft3.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft3.Shoot(launcherLeft3.Muzzle.forward, 0f);
						break;
					case 4:
						if (positioningInfo.DiffToPlayer.y < 24f && player.GetVelocity().y < 0f)
						{
							railRiderAttackToUse = 0;
							break;
						}
						launcherRight1.SetMuzzle(rightLowerLauncherAlt);
						launcherRight1.SetGrindPath((launcherRight1.ShotsFired % 2 == 0) ? path : path2);
						launcherRight1.Shoot(launcherRight1.Muzzle.forward, 0f);
						launcherLeft1.SetMuzzle(leftLowerLauncherAlt);
						launcherLeft1.SetGrindPath((launcherLeft1.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft1.Shoot(launcherLeft1.Muzzle.forward, 0f);
						break;
					case 5:
						if (positioningInfo.DiffToPlayer.y < 47f && player.GetVelocity().y < 0f)
						{
							railRiderAttackToUse = 1;
							break;
						}
						launcherRight2.SetMuzzle(rightMiddleLauncher);
						launcherRight2.SetGrindPath((launcherRight2.ShotsFired % 2 == 0) ? path : path2);
						launcherRight2.Shoot(launcherRight2.Muzzle.forward, 0f);
						launcherLeft2.SetMuzzle(leftMiddleLauncher);
						launcherLeft2.SetGrindPath((launcherLeft2.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft2.Shoot(launcherLeft2.Muzzle.forward, 0f);
						break;
					case 6:
						launcherRight3.SetGrindPath((launcherRight3.ShotsFired % 2 == 0) ? path : path2);
						launcherRight3.Shoot(-launcherRight3.Muzzle.forward, 0f);
						launcherLeft3.SetGrindPath((launcherLeft3.ShotsFired % 2 == 1) ? path : path2);
						launcherLeft3.Shoot(-launcherLeft3.Muzzle.forward, 0f);
						break;
					}
				}
				if (stateTime < 0.75f)
				{
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(originalForward), Core.dt * 4f);
				}
				if (!CheckIfDied())
				{
					if (needToLoseArm)
					{
						SetState(SnakeBossState.LOSE_ARM);
					}
					else if (!enemyGrindLines.PlayerActiveOnEnemy && (launcherRight3.Reloading || launcherLeft3.Reloading || launcherRight2.Reloading || launcherLeft2.Reloading || launcherRight1.Reloading || launcherLeft1.Reloading))
					{
						SetState(SnakeBossState.VENT);
					}
				}
				break;
			case SnakeBossState.MACHINEGUN_PREPARE:
				if (!CheckIfDied())
				{
					if (needToLoseArm)
					{
						SetState(SnakeBossState.LOSE_ARM);
					}
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(positioningInfo.FlatDirToPlayer), Core.dt);
					if (stateTime > 3.416f && !enemyGrindLines.PlayerActiveOnEnemy)
					{
						SetState(SnakeBossState.MACHINEGUN_ATTACK);
					}
				}
				break;
			case SnakeBossState.MACHINEGUN_ATTACK:
			{
				aiming.GetTargetPos();
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(positioningInfo.FlatDirToPlayer), Core.dt * (Mathf.Sin(stateTime) / 4f + 0.75f) * 10f);
				armWithSmallestDist = -1;
				float num2 = float.MaxValue;
				armWithSecondSmallestDist = -1;
				float num3 = float.MaxValue;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				float num8 = 0f;
				float num9 = 0f;
				float num10 = 0f;
				num4 = leftMachineGun.Muzzle.position.y - player.tf.position.y;
				num5 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num5 < num2 && leftArm1Data.armAlive)
				{
					armWithSmallestDist = 0;
					num2 = num5;
				}
				num4 = rightMachineGun.Muzzle.position.y - player.tf.position.y;
				num6 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num6 < num2 && rightArm1Data.armAlive)
				{
					armWithSmallestDist = 1;
					num2 = num6;
				}
				num4 = leftMachineGun2.Muzzle.position.y - player.tf.position.y;
				num7 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num7 < num2 && leftArm2Data.armAlive)
				{
					armWithSmallestDist = 2;
					num2 = num7;
				}
				num4 = rightMachineGun2.Muzzle.position.y - player.tf.position.y;
				num8 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num8 < num2 && rightArm2Data.armAlive)
				{
					armWithSmallestDist = 3;
					num2 = num8;
				}
				num4 = leftMachineGun3.Muzzle.position.y - player.tf.position.y;
				num9 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num9 < num2 && leftArm3Data.armAlive)
				{
					armWithSmallestDist = 4;
					num2 = num9;
				}
				num4 = rightMachineGun3.Muzzle.position.y - player.tf.position.y;
				num10 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (num10 < num2 && rightArm3Data.armAlive)
				{
					armWithSmallestDist = 5;
					num2 = num10;
				}
				num4 = leftMachineGun.Muzzle.position.y - player.tf.position.y;
				num5 = (float)((num4 > 0f) ? 1 : 2) * Mathf.Abs(num4);
				if (armWithSmallestDist != 0 && num5 < num3 && leftArm1Data.armAlive)
				{
					armWithSecondSmallestDist = 0;
					num3 = num5;
				}
				if (armWithSmallestDist != 1 && num6 < num2 && rightArm1Data.armAlive)
				{
					armWithSecondSmallestDist = 1;
					num3 = num6;
				}
				if (armWithSmallestDist != 2 && num7 < num3 && leftArm2Data.armAlive)
				{
					armWithSecondSmallestDist = 2;
					num3 = num7;
				}
				if (armWithSmallestDist != 3 && num8 < num3 && rightArm2Data.armAlive)
				{
					armWithSecondSmallestDist = 3;
					num3 = num8;
				}
				if (armWithSmallestDist != 4 && num9 < num3 && leftArm3Data.armAlive)
				{
					armWithSecondSmallestDist = 4;
					num3 = num9;
				}
				if (armWithSmallestDist != 5 && num10 < num3 && rightArm3Data.armAlive)
				{
					armWithSecondSmallestDist = 5;
					num3 = num10;
				}
				animations.Animator.SetLayerWeight(2, (armWithSmallestDist == 0 || armWithSecondSmallestDist == 0) ? 1 : 0);
				animations.Animator.SetLayerWeight(3, (armWithSmallestDist == 1 || armWithSecondSmallestDist == 1) ? 1 : 0);
				animations.Animator.SetLayerWeight(4, (armWithSmallestDist == 2 || armWithSecondSmallestDist == 2) ? 1 : 0);
				animations.Animator.SetLayerWeight(5, (armWithSmallestDist == 3 || armWithSecondSmallestDist == 3) ? 1 : 0);
				animations.Animator.SetLayerWeight(6, (armWithSmallestDist == 4 || armWithSecondSmallestDist == 4) ? 1 : 0);
				animations.Animator.SetLayerWeight(7, (armWithSmallestDist == 5 || armWithSecondSmallestDist == 5) ? 1 : 0);
				if (leftMachineGun.CanShoot && (armWithSmallestDist == 0 || armWithSecondSmallestDist == 0))
				{
					leftMachineGun.Shoot(leftMachineGun.Muzzle.position, (aiming.GetTargetPos() - leftMachineGun.Muzzle.position).normalized, 0f);
				}
				if (rightMachineGun.CanShoot && (armWithSmallestDist == 1 || armWithSecondSmallestDist == 1))
				{
					rightMachineGun.Shoot(rightMachineGun.Muzzle.position, (aiming.GetTargetPos() - rightMachineGun.Muzzle.position).normalized, 0f);
				}
				if (leftMachineGun2.CanShoot && (armWithSmallestDist == 2 || armWithSecondSmallestDist == 2))
				{
					leftMachineGun2.Shoot(leftMachineGun2.Muzzle.position, (aiming.GetTargetPos() - leftMachineGun2.Muzzle.position).normalized, 0f);
				}
				if (rightMachineGun2.CanShoot && (armWithSmallestDist == 3 || armWithSecondSmallestDist == 3))
				{
					rightMachineGun2.Shoot(rightMachineGun2.Muzzle.position, (aiming.GetTargetPos() - rightMachineGun2.Muzzle.position).normalized, 0f);
				}
				if (leftMachineGun3.CanShoot && (armWithSmallestDist == 4 || armWithSecondSmallestDist == 4))
				{
					leftMachineGun3.Shoot(leftMachineGun3.Muzzle.position, (aiming.GetTargetPos() - leftMachineGun3.Muzzle.position).normalized, 0f);
				}
				if (rightMachineGun3.CanShoot && (armWithSmallestDist == 5 || armWithSecondSmallestDist == 5))
				{
					rightMachineGun3.Shoot(rightMachineGun3.Muzzle.position, (aiming.GetTargetPos() - rightMachineGun3.Muzzle.position).normalized, 0f);
				}
				if (!CheckIfDied())
				{
					if (needToLoseArm)
					{
						SetState(SnakeBossState.LOSE_ARM);
					}
					else if (stateTime > 8f)
					{
						SetState(SnakeBossState.VENT);
					}
				}
				break;
			}
			case SnakeBossState.VENT:
			{
				float num12 = ((lastSnakeBossState == SnakeBossState.MACHINEGUN_ATTACK) ? 2.32f : 5f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(originalForward), Core.dt);
				if (stateTime > num12 && (leftArm1Data.armExtension.State != EnemyExtension.ExtensionState.OPEN || rightArm1Data.armExtension.State != EnemyExtension.ExtensionState.OPEN || leftArm2Data.armExtension.State != EnemyExtension.ExtensionState.OPEN || rightArm2Data.armExtension.State != EnemyExtension.ExtensionState.OPEN || leftArm3Data.armExtension.State != EnemyExtension.ExtensionState.OPEN || rightArm3Data.armExtension.State != EnemyExtension.ExtensionState.OPEN))
				{
					if (stateTime < num12 + 8.17f)
					{
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(leftArm1Data.armRoot.position));
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(leftArm2Data.armRoot.position));
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(leftArm3Data.armRoot.position));
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(rightArm1Data.armRoot.position));
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(rightArm2Data.armRoot.position));
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.HatchOpen, GetAudioSourceAtLocation(rightArm3Data.armRoot.position));
						leftArm1Data.armExtension.OpenExtension();
						leftArm2Data.armExtension.OpenExtension();
						leftArm3Data.armExtension.OpenExtension();
						rightArm1Data.armExtension.OpenExtension();
						rightArm2Data.armExtension.OpenExtension();
						rightArm3Data.armExtension.OpenExtension();
					}
					SwitchAllArmsToLongGrinds();
				}
				if (stateTime >= num12 + 8.17f || stopVenting)
				{
					if (lastSnakeBossState != SnakeBossState.RAILRIDER)
					{
						animations.PlayAnim(ventEndHash);
					}
					else
					{
						if (railRiderAttackToUse == 0)
						{
							animations.PlayAnim(railRidersLowVentEndHash);
						}
						if (railRiderAttackToUse == 1)
						{
							animations.PlayAnim(railRidersMidVentEndHash);
						}
						if (railRiderAttackToUse == 2)
						{
							animations.PlayAnim(railRidersHighVentEndHash);
						}
						if (railRiderAttackToUse == 4)
						{
							animations.PlayAnim(railRidersLowVentEndHash);
						}
						if (railRiderAttackToUse == 5)
						{
							animations.PlayAnim(railRidersMidVentEndHash);
						}
						if (railRiderAttackToUse == 6)
						{
							animations.PlayAnim(railRidersHighVentEndHash);
						}
					}
					if (stopVenting)
					{
						stateTime = 12.97f;
						stopVenting = false;
					}
				}
				if (!CheckIfDied() && stateTime > num12 + 8.17f + 5.86f)
				{
					if (needToLoseArm)
					{
						SetState(SnakeBossState.LOSE_ARM);
					}
					else if (!enemyGrindLines.PlayerActiveOnEnemy)
					{
						SetState(SnakeBossState.IDLE);
					}
				}
				break;
			}
			case SnakeBossState.TANKGUN_AIM:
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(originalForward), Core.dt);
				Vector3 practicalWorldVelocity = player.GetPracticalWorldVelocity();
				practicalWorldVelocity.y = 0f;
				_ = practicalWorldVelocity.magnitude;
				Vector3 normalized = practicalWorldVelocity.normalized;
				_ = player.minMoveSpeed;
				Vector3.Cross(Vector3.up, normalized);
				_ = Vector3.zero;
				if (stateTime > 3.167f / (float)enemyTargets.MaxTargetVisuals && placedTargetInt < enemyTargets.MaxTargetVisuals)
				{
					int num = ((angle < 0f) ? (firstMissileLocation + 1 + placedTargetInt) : (firstMissileLocation - 1 - placedTargetInt));
					if (num < 0)
					{
						num = missileLocations.Length + num;
					}
					else if (num >= missileLocations.Length)
					{
						num -= missileLocations.Length;
					}
					enemyTargets.TargetVisuals[placedTargetInt].SetPos(missileLocations[num].position);
					Vector3 forward = base.transform.position - missileLocations[num].position;
					forward.y = 0f;
					enemyTargets.TargetVisuals[placedTargetInt].tf.rotation = Quaternion.LookRotation(forward);
					enemyTargets.TargetVisuals[placedTargetInt].SetOn();
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.sniper_lockon, enemyTargets.TargetVisuals[placedTargetInt].gameObject.GetComponent<AudioSource>());
					mortar.ClipSize++;
					placedTargetInt++;
					stateTime = 0f;
				}
				if (!CheckIfDied())
				{
					if (needToLoseArm)
					{
						SetState(SnakeBossState.LOSE_ARM);
					}
					else if ((placedTargetInt == enemyTargets.MaxTargetVisuals || placedTargetInt >= enemyTargets.TargetVisuals.Count - 1) && !enemyGrindLines.PlayerActiveOnEnemy && stateTime > 3.16f)
					{
						SetState(SnakeBossState.TANKGUN_ATTACK);
					}
				}
				break;
			}
			case SnakeBossState.TANKGUN_ATTACK:
			{
				if (stateTime > 1f && !mortar.Reloading)
				{
					for (int i = 0; i < enemyTargets.MaxTargetVisuals; i++)
					{
						if (enemyTargets.TargetVisuals[i].off)
						{
							continue;
						}
						if (!setNextTargetDir && mortar.RemainingTimeBetweenShots <= 1f)
						{
							Vector3 pos = enemyTargets.TargetVisuals[i].pos;
							pos.y = 0f;
							Vector3 position = tf.position;
							position.y = 0f;
							Vector3 eulerAngles = Quaternion.LookRotation((pos - position).normalized).eulerAngles;
							eulerAngles.y -= 90f;
							eulerAngles.y = Mathf.RoundToInt(eulerAngles.y / 90f) * 90;
							if (eulerAngles.y < 0f)
							{
								eulerAngles.y += 360f;
							}
							lastHeadTargetDir = headTargetDir;
							headTargetDir = eulerAngles;
							setNextTargetDir = true;
							timeSet = Time.time;
						}
						if (mortar.CanShoot)
						{
							mortar.Shoot(enemyTargets.TargetVisuals[i].pos, enemyTargets.TargetVisuals[i].tf.forward, 0f);
							enemyTargets.TargetVisuals[i].SetOff();
							animations.PlayAnim(mortarShootHash, restart: true);
							setNextTargetDir = false;
						}
					}
				}
				float num11 = Mathf.Lerp(lastHeadTargetDir.y, headTargetDir.y, Time.time - timeSet);
				animations.SetFloatParameter(mortarAimDirHash, num11 / 360f);
				if (mortar.Reloading)
				{
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(positioningInfo.FlatDirToPlayer), Core.dt);
					animations.PlayAnim(stopMortarHash);
				}
				if (CheckIfDied())
				{
					break;
				}
				if (needToLoseArm)
				{
					for (int j = 0; j < enemyTargets.MaxTargetVisuals; j++)
					{
						enemyTargets.TargetVisuals[j].SetOff();
					}
					SetState(SnakeBossState.LOSE_ARM);
				}
				else if (mortar.Reloading && mortar.TimeReloading >= 3.3f && !enemyGrindLines.PlayerActiveOnEnemy)
				{
					SetState(SnakeBossState.IDLE);
				}
				break;
			}
			case SnakeBossState.LOSE_ARM:
				if (((stateTime > 6f && !enemyGrindLines.PlayerActiveOnEnemy) || stateTime > 8f) && needToLoseArm)
				{
					if (enemyGrindLines.PlayerActiveOnEnemy)
					{
						player.hitBounceAbility.FreeformBounce(HitBounceAbility.BounceType.SNAKEBOSS_BOUNCE);
					}
					stateTime = 6f;
					needToLoseArm = false;
					if (LoseArm(leftArm1Data))
					{
						animations.PlayAnim(loseArmRecoilLowLeftHash);
						leftMachineGun.Equip(equip: false);
						launcherLeft1.Equip(equip: false);
					}
					if (LoseArm(leftArm2Data))
					{
						animations.PlayAnim(loseArmRecoilMidLeftHash);
						leftMachineGun2.Equip(equip: false);
						launcherLeft2.Equip(equip: false);
					}
					if (LoseArm(leftArm3Data))
					{
						animations.PlayAnim(loseArmRecoilHighLeftHash);
						leftMachineGun3.Equip(equip: false);
						launcherLeft3.Equip(equip: false);
					}
					if (LoseArm(rightArm1Data))
					{
						animations.PlayAnim(loseArmRecoilLowRightHash);
						rightMachineGun.Equip(equip: false);
						launcherRight1.Equip(equip: false);
					}
					if (LoseArm(rightArm2Data))
					{
						animations.PlayAnim(loseArmRecoilMidRightHash);
						rightMachineGun2.Equip(equip: false);
						launcherRight2.Equip(equip: false);
					}
					if (LoseArm(rightArm3Data))
					{
						animations.PlayAnim(loseArmRecoilHighRightHash);
						rightMachineGun3.Equip(equip: false);
						launcherRight3.Equip(equip: false);
					}
					GameplayCamera.StartScreenShake(ScreenShakeType.HEAVY, 0.5f);
				}
				if (!CheckIfDied())
				{
					if (!rightArm1Data.armAlive && !rightArm2Data.armAlive && !rightArm3Data.armAlive && !leftArm1Data.armAlive && !leftArm2Data.armAlive && !leftArm3Data.armAlive && chestState != ChestState.OPEN && stateTime > 6.5f)
					{
						Core.Instance.AudioManager.StartCustomTrack(customMusicTrack, ref customMusicTrackInstance);
						pastChestState = ChestState.CLOSED;
						chestState = ChestState.OPEN;
					}
					if (stateTime > 9.33f && !needToLoseArm)
					{
						SetState(SnakeBossState.IDLE);
					}
				}
				break;
			case SnakeBossState.DIE:
				if (stateTime > 5f)
				{
					SetBasicState(BasicEnemyState.UNACTIVE);
				}
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
				rigidbody.GetComponent<Rigidbody>().AddForce(armData.armExtension.GSpot.transform.forward * 6f, ForceMode.VelocityChange);
				armData.DeactivateAllGrindLines();
				armData.armRoot.localScale = Vector3.zero;
				armData.armRoot.gameObject.SetActive(value: false);
				armData.armExplosion.Play();
				audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.MachineryExplosion, GetAudioSourceAtLocation(armData.armRoot.position));
				HP--;
				return true;
			}
			return false;
		}

		protected override void LateUpdateActive()
		{
			base.LateUpdateActive();
			Vector3 vector = player.tf.position - upperSpineBone.position;
			float sqrMagnitude = new Vector3(vector.x, 0f, vector.y).sqrMagnitude;
			if ((chestState == ChestState.PEAKING || chestState == ChestState.OPEN) && overallLookAtWeight < 1f && sqrMagnitude > 25f)
			{
				overallLookAtWeight += Core.dt;
			}
			else if (overallLookAtWeight > 0f)
			{
				overallLookAtWeight -= Core.dt;
			}
			if (overallLookAtWeight > 0f)
			{
				Quaternion quaternion = Quaternion.LookRotation(tf.forward, Vector3.up);
				Vector3 eulerAngles = (Quaternion.Inverse(quaternion) * upperSpineBone.rotation).eulerAngles;
				Quaternion b = Quaternion.LookRotation((player.tf.position - upperSpineBone.position).normalized, Vector3.up);
				float num = 1f - Mathf.Clamp01(Mathf.Abs(player.tf.position.y - upperSpineBone.position.y) / 10f);
				Quaternion rotation = Quaternion.Lerp(quaternion, b, num * overallLookAtWeight);
				upperSpineBone.rotation = rotation;
				upperSpineBone.Rotate(eulerAngles);
			}
			if (snakeBossState == SnakeBossState.MACHINEGUN_ATTACK && !enemyGrindLines.PlayerActiveOnEnemy)
			{
				Quaternion identity = Quaternion.identity;
				Quaternion identity2 = Quaternion.identity;
				identity = shRBone01.rotation;
				identity2 = Quaternion.LookRotation(rightMachineGun.Muzzle.forward, Vector3.up);
				Vector3 vector2 = Vector3.ProjectOnPlane(rightMachineGun.Muzzle.position - shRBone01.position, identity2 * Vector3.forward);
				Vector3 eulerAngles2 = (Quaternion.Inverse(identity2) * shRBone01.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shRBone01.position - vector2).normalized, Vector3.up);
				shRBone01.rotation = identity2;
				shRBone01.Rotate(eulerAngles2);
				aimBlend2 = Mathf.Clamp01(aimBlend2 + ((armWithSmallestDist == 1 || armWithSecondSmallestDist == 1) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shRBone01.rotation = Quaternion.Lerp(shRBone01.rotation, identity, 1f - aimBlend2);
				identity = shLBone01.rotation;
				identity2 = Quaternion.LookRotation(leftMachineGun.Muzzle.forward, Vector3.up);
				vector2 = Vector3.ProjectOnPlane(leftMachineGun.Muzzle.position - shLBone01.position, identity2 * Vector3.forward);
				eulerAngles2 = (Quaternion.Inverse(identity2) * shLBone01.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shLBone01.position - vector2).normalized, Vector3.up);
				shLBone01.rotation = identity2;
				shLBone01.Rotate(eulerAngles2);
				aimBlend1 = Mathf.Clamp01(aimBlend1 + ((armWithSmallestDist == 0 || armWithSecondSmallestDist == 0) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shLBone01.rotation = Quaternion.Lerp(shLBone01.rotation, identity, 1f - aimBlend1);
				identity = shLBone02.rotation;
				identity2 = Quaternion.LookRotation(leftMachineGun2.Muzzle.forward, Vector3.up);
				vector2 = Vector3.ProjectOnPlane(leftMachineGun2.Muzzle.position - shLBone02.position, identity2 * Vector3.forward);
				eulerAngles2 = (Quaternion.Inverse(identity2) * shLBone02.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shLBone02.position - vector2).normalized, Vector3.up);
				shLBone02.rotation = identity2;
				shLBone02.Rotate(eulerAngles2);
				aimBlend3 = Mathf.Clamp01(aimBlend3 + ((armWithSmallestDist == 2 || armWithSecondSmallestDist == 2) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shLBone02.rotation = Quaternion.Lerp(shLBone02.rotation, identity, 1f - aimBlend3);
				identity = shRBone02.rotation;
				identity2 = Quaternion.LookRotation(rightMachineGun2.Muzzle.forward, Vector3.up);
				vector2 = Vector3.ProjectOnPlane(rightMachineGun2.Muzzle.position - shRBone02.position, identity2 * Vector3.forward);
				eulerAngles2 = (Quaternion.Inverse(identity2) * shRBone02.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shRBone02.position - vector2).normalized, Vector3.up);
				shRBone02.rotation = identity2;
				shRBone02.Rotate(eulerAngles2);
				aimBlend4 = Mathf.Clamp01(aimBlend4 + ((armWithSmallestDist == 3 || armWithSecondSmallestDist == 3) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shRBone02.rotation = Quaternion.Lerp(shRBone02.rotation, identity, 1f - aimBlend4);
				identity = shLBone03.rotation;
				identity2 = Quaternion.LookRotation(leftMachineGun3.Muzzle.forward, Vector3.up);
				vector2 = Vector3.ProjectOnPlane(leftMachineGun3.Muzzle.position - shLBone03.position, identity2 * Vector3.forward);
				eulerAngles2 = (Quaternion.Inverse(identity2) * shLBone03.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shLBone03.position - vector2).normalized, Vector3.up);
				shLBone03.rotation = identity2;
				shLBone03.Rotate(eulerAngles2);
				aimBlend5 = Mathf.Clamp01(aimBlend5 + ((armWithSmallestDist == 4 || armWithSecondSmallestDist == 4) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shLBone03.rotation = Quaternion.Lerp(shLBone03.rotation, identity, 1f - aimBlend5);
				identity = shRBone03.rotation;
				identity2 = Quaternion.LookRotation(rightMachineGun3.Muzzle.forward, Vector3.up);
				vector2 = Vector3.ProjectOnPlane(rightMachineGun3.Muzzle.position - shRBone03.position, identity2 * Vector3.forward);
				eulerAngles2 = (Quaternion.Inverse(identity2) * shRBone03.rotation).eulerAngles;
				identity2 = Quaternion.LookRotation((aiming.GetTargetPos() - shRBone03.position - vector2).normalized, Vector3.up);
				shRBone03.rotation = identity2;
				shRBone03.Rotate(eulerAngles2);
				aimBlend6 = Mathf.Clamp01(aimBlend6 + ((armWithSmallestDist == 5 || armWithSecondSmallestDist == 5) ? (Core.dt * 2f) : ((0f - Core.dt) * 2f)));
				shRBone03.rotation = Quaternion.Lerp(shRBone03.rotation, identity, 1f - aimBlend6);
			}
		}

		private void ExitState(SnakeBossState targetState)
		{
			switch (snakeBossState)
			{
			case SnakeBossState.RAILRIDER:
			{
				SwitchAllArmsToLongGrinds();
				for (int j = 0; j < lowCableEffects.Length; j++)
				{
					lowCableEffects[j].Stop();
				}
				for (int k = 0; k < midCableEffects.Length; k++)
				{
					midCableEffects[k].Stop();
				}
				for (int l = 0; l < highCableEffects.Length; l++)
				{
					highCableEffects[l].Stop();
				}
				break;
			}
			case SnakeBossState.MACHINEGUN_ATTACK:
				SwitchAllArmsToLongGrinds();
				break;
			case SnakeBossState.VENT:
				if (leftArm1Data.armExtension.GSpot.state != GraffitiState.FINISHED && leftArm1Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					leftArm1Data.armExtension.CloseExtension();
				}
				if (leftArm2Data.armExtension.GSpot.state != GraffitiState.FINISHED && leftArm2Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					leftArm2Data.armExtension.CloseExtension();
				}
				if (leftArm3Data.armExtension.GSpot.state != GraffitiState.FINISHED && leftArm3Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					leftArm3Data.armExtension.CloseExtension();
				}
				if (rightArm1Data.armExtension.GSpot.state != GraffitiState.FINISHED && rightArm1Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					rightArm1Data.armExtension.CloseExtension();
				}
				if (rightArm2Data.armExtension.GSpot.state != GraffitiState.FINISHED && rightArm2Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					rightArm2Data.armExtension.CloseExtension();
				}
				if (rightArm3Data.armExtension.GSpot.state != GraffitiState.FINISHED && rightArm3Data.armExtension.State == EnemyExtension.ExtensionState.OPEN)
				{
					rightArm3Data.armExtension.CloseExtension();
				}
				SwitchAllArmsToLongGrinds();
				break;
			case SnakeBossState.TANKGUN_ATTACK:
			{
				for (int i = 0; i < enemyTargets.MaxTargetVisuals; i++)
				{
					if (!enemyTargets.TargetVisuals[i].off)
					{
						enemyTargets.TargetVisuals[i].SetOff();
					}
				}
				break;
			}
			case SnakeBossState.IDLE:
			case SnakeBossState.MACHINEGUN_PREPARE:
			case SnakeBossState.TANKGUN_AIM:
			case SnakeBossState.LOSE_ARM:
				break;
			}
		}

		protected override void EnteringUnactiveState()
		{
			base.EnteringUnactiveState();
			snakeBossState = SnakeBossState.IDLE;
			teleporterHigh.SetActive(value: false);
			teleporterLow.SetActive(value: true);
		}

		private void OnGSpotStateChanged(GraffitiSpotFinisher gSpot, GraffitiState result)
		{
			if (result == GraffitiState.PAINTING)
			{
				WorldHandler.instance.AddCombatBlocker(5f);
				if (rightArm1Data.armExtension.GSpot == gSpot && rightArm1Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceRightArm1);
				}
				if (rightArm2Data.armExtension.GSpot == gSpot && rightArm2Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceRightArm2);
				}
				if (rightArm3Data.armExtension.GSpot == gSpot && rightArm3Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceRightArm3);
				}
				if (leftArm1Data.armExtension.GSpot == gSpot && leftArm1Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceLeftArm1);
				}
				if (leftArm2Data.armExtension.GSpot == gSpot && leftArm2Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceLeftArm2);
				}
				if (leftArm3Data.armExtension.GSpot == gSpot && leftArm3Data.StopSteam())
				{
					audioManager.StopLoopingSfx(curVentAudioSourceLeftArm3);
				}
			}
			if (result == GraffitiState.CANCEL)
			{
				WorldHandler.instance.AddCombatBlocker(5f);
				if (rightArm1Data.armExtension.GSpot == gSpot && rightArm1Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && rightArm1Data.PlaySteam())
				{
					curVentAudioSourceRightArm1 = GetAudioSourceAtLocation(rightArm1Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceRightArm1);
				}
				if (rightArm2Data.armExtension.GSpot == gSpot && rightArm2Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && rightArm2Data.PlaySteam())
				{
					curVentAudioSourceRightArm2 = GetAudioSourceAtLocation(rightArm2Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceRightArm2);
				}
				if (rightArm3Data.armExtension.GSpot == gSpot && rightArm3Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && rightArm3Data.PlaySteam())
				{
					curVentAudioSourceRightArm3 = GetAudioSourceAtLocation(rightArm3Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceRightArm3);
				}
				if (leftArm1Data.armExtension.GSpot == gSpot && leftArm1Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && leftArm1Data.PlaySteam())
				{
					curVentAudioSourceLeftArm1 = GetAudioSourceAtLocation(leftArm1Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceLeftArm1);
				}
				if (leftArm2Data.armExtension.GSpot == gSpot && leftArm2Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && leftArm2Data.PlaySteam())
				{
					curVentAudioSourceLeftArm2 = GetAudioSourceAtLocation(leftArm2Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceLeftArm2);
				}
				if (leftArm3Data.armExtension.GSpot == gSpot && leftArm3Data.armExtension.State == EnemyExtension.ExtensionState.OPEN && leftArm3Data.PlaySteam())
				{
					curVentAudioSourceLeftArm3 = GetAudioSourceAtLocation(leftArm3Data.armRoot.position);
					audioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.tankwalkerVent, curVentAudioSourceLeftArm3);
				}
			}
			if (result == GraffitiState.FINISHED)
			{
				player.SetPosAndRotHard(gSpot.GetGraffitiSpotPlayerPos(), Quaternion.LookRotation(-gSpot.transform.up.normalized));
				player.hitBounceAbility.FreeformBounce(HitBounceAbility.BounceType.SNAKEBOSS_BOUNCE);
				stopVenting = true;
				if (gSpot == leftArm1Data.armExtension.GSpot)
				{
					leftArm1Data.armAlive = false;
					leftArm1Data.PlayOverheat();
					needToLoseArm = true;
				}
				if (gSpot == leftArm2Data.armExtension.GSpot)
				{
					leftArm2Data.armAlive = false;
					leftArm2Data.PlayOverheat();
					needToLoseArm = true;
				}
				if (gSpot == leftArm3Data.armExtension.GSpot)
				{
					leftArm3Data.armAlive = false;
					leftArm3Data.PlayOverheat();
					needToLoseArm = true;
				}
				if (gSpot == rightArm1Data.armExtension.GSpot)
				{
					rightArm1Data.armAlive = false;
					rightArm1Data.PlayOverheat();
					needToLoseArm = true;
				}
				if (gSpot == rightArm2Data.armExtension.GSpot)
				{
					rightArm2Data.armAlive = false;
					rightArm2Data.PlayOverheat();
					needToLoseArm = true;
				}
				if (gSpot == rightArm3Data.armExtension.GSpot)
				{
					rightArm3Data.armAlive = false;
					rightArm3Data.PlayOverheat();
					needToLoseArm = true;
				}
			}
		}

		private bool CheckIfDied()
		{
			bool num = HP <= 0;
			if (num)
			{
				SetState(SnakeBossState.DIE);
			}
			return num;
		}

		public override bool IsDead()
		{
			if (HP <= 0)
			{
				return stateTime > 1f;
			}
			return false;
		}

		private void DeactivateAllArmGrinds()
		{
			leftArm1Data.DeactivateAllGrindLines();
			leftArm2Data.DeactivateAllGrindLines();
			leftArm3Data.DeactivateAllGrindLines();
			rightArm1Data.DeactivateAllGrindLines();
			rightArm2Data.DeactivateAllGrindLines();
			rightArm3Data.DeactivateAllGrindLines();
		}

		private void SwitchAllArmsToLongGrinds()
		{
			leftArm1Data.SwitchToLongGrindLines();
			leftArm2Data.SwitchToLongGrindLines();
			leftArm3Data.SwitchToLongGrindLines();
			rightArm1Data.SwitchToLongGrindLines();
			rightArm2Data.SwitchToLongGrindLines();
			rightArm3Data.SwitchToLongGrindLines();
		}

		private AudioSource GetAudioSourceAtLocation(Vector3 position)
		{
			curAudioSourceIndex++;
			if (curAudioSourceIndex >= audioSources.Length)
			{
				curAudioSourceIndex = 0;
			}
			AudioSource obj = audioSources[curAudioSourceIndex];
			obj.transform.position = position;
			return obj;
		}

		public AudioSource GetAudioSourceForAnimationEvents()
		{
			Vector3 position = tf.position;
			position.y = player.tf.position.y;
			return GetAudioSourceAtLocation(position);
		}
	}
}
