using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Reptile.Phone;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Reptile
{
	public class Player : MonoBehaviour
	{
		public delegate void OnExitTriggerHitHandle(Player player, StageTransition exit);

		public delegate void OnReviveHandler();

		public class AnimInfo
		{
			public Dictionary<int, float> fadeFrom = new Dictionary<int, float>();

			public Dictionary<int, float> fadeTo = new Dictionary<int, float>();

			public int nextAnim;

			public float nextAnimAtTime;

			public float duration;

			public bool feetIK;

			public AnimType animType;

			public bool skipStartrun;

			public float interruptIntoFallFrom;

			public AnimInfo(Player player, string name, AnimType setAnimType = AnimType.NORMAL, string nextAnimation = "", float setInterruptIntoFallFrom = 0f, float nextAnimationAtTime = 0f, float setDuration = 1f)
			{
				player.animInfosSets[(int)player.moveStyle].Add(Animator.StringToHash(name), this);
				nextAnim = Animator.StringToHash(nextAnimation);
				animType = setAnimType;
				duration = setDuration;
				nextAnimAtTime = nextAnimationAtTime;
				interruptIntoFallFrom = setInterruptIntoFallFrom;
			}
		}

		public enum AnimType
		{
			NORMAL = 0,
			IDLE = 1,
			RUN = 2,
			STOPRUN = 3,
			LAND = 4,
			OVERRULE_IDLE = 5,
			AIRTRICK = 6,
			GROUNDTRICK = 7
		}

		public enum SpraycanState
		{
			NONE = 0,
			START = 1,
			SHAKE = 2,
			SPRAY = 3
		}

		private enum StringStatusScore
		{
			EMPTY_STRING = -1,
			NO_COMBO = -2,
			COMBO = -3
		}

		public class PickupDisplay
		{
			public GameObject pickupUIElement;

			public Pickup.PickUpType pickupType;

			public Vector2 startPosition;

			public float displayTimeLeft;

			public float displayDuration;
		}

		public class GrafSpotUIIndicatorData : UIIndicatorData
		{
			public GraffitiSpot graffitiSpot;

			public bool isClaimed;
		}

		public class UIIndicatorData
		{
			public int id;

			public Transform trans;

			public Animation uiObjectAnimation;

			public CanvasGroup uiObjectCanvasGroup;

			public bool inView;

			public bool isActive;

			public bool isOccluded;

			public bool inRange;

			public GameObject uiObject;

			public float timeTillShow;

			private string currentAnimationState = string.Empty;

			public void SetComponents()
			{
				uiObjectAnimation = uiObject.GetComponent<Animation>();
				uiObjectCanvasGroup = uiObject.GetComponentInChildren<CanvasGroup>();
			}

			public void PlayAnimationForIndicator(string animationState)
			{
				currentAnimationState = animationState;
				uiObjectAnimation.Play(animationState);
			}

			public bool CompareCurrentAnimationState(string animationState)
			{
				return animationState.Equals(currentAnimationState);
			}
		}

		public enum MovementType
		{
			RUNNING = 0,
			WALKING = 1,
			NONE = 2
		}

		public enum TrickType
		{
			NONE = 0,
			GRIND_START = 1,
			GRIND = 2,
			GRIND_BOOST = 3,
			GROUND = 4,
			GROUND_BOOST = 5,
			AIR = 6,
			AIR_VERT = 7,
			AIR_BOOST = 8,
			SPECIAL_AIR = 9,
			WALLRUN = 10,
			GRIND_SWITCH_MOVESTYLE = 11,
			SLIDE = 12,
			HANDPLANT = 13,
			POLE_FLIP = 14,
			HIT_BOUNCE = 15,
			GRAFFITI_S = 16,
			GRAFFITI_M = 17,
			GRAFFITI_L = 18,
			GRAFFITI_XL = 19,
			NEW_HARD_CORNER = 20,
			HARD_CORNER = 21,
			SOFT_CORNER = 22
		}

		public class Trick
		{
			public int points;

			public int standardPoints;

			public int pointsDegradeAmount;

			public int minimumPoints;

			public Trick(int setStandardPoints, int setMinimumPoints = 20, int setPointsDegradeAmount = 10)
			{
				standardPoints = setStandardPoints;
				pointsDegradeAmount = setPointsDegradeAmount;
				minimumPoints = setMinimumPoints;
				points = standardPoints;
			}

			public bool Use(Player p)
			{
				p.currentTrickPoints = points;
				points = Mathf.Max(points - pointsDegradeAmount, minimumPoints);
				if (points >= standardPoints - pointsDegradeAmount)
				{
					return true;
				}
				return false;
			}

			public void Refresh(ref bool changed)
			{
				if (points != standardPoints)
				{
					changed = true;
				}
				points = standardPoints;
			}
		}

		internal bool userInputEnabled = true;

		[HideInInspector]
		public Transform tf;

		private Vector3 velBeforePause;

		private Vector3 platformVelBeforePause;

		private Vector3 platformAngularVelBeforePause;

		private bool kinematicBeforePause;

		private bool isDisabled;

		protected Vector3 velBeforeDisable;

		protected Vector3 platformVelBeforeDisable;

		protected Vector3 platformAngularVelBeforeDisable;

		protected bool kinematicBeforeDisable;

		protected int disableCount;

		private Vector3 teleportPos;

		private UserInputHandler.InputBuffer inputBuffer;

		private UserInputHandler playerInput;

		internal Vector2 cameraInputBuffer;

		private SafeLocation saveLocation;

		public Vector3 lastNavMeshPosition;

		internal float timeSinceSpawn;

		public GameObject phonePrefab;

		internal Reptile.Phone.Phone phone;

		public Collider interactionCollider;

		internal PlayerType playerType;

		internal bool isAI;

		internal bool isGirl;

		internal PlayerAI AI;

		internal Crew crew = Crew.PLAYERS;

		internal Vector3 debugDirYellow = Vector3.zero;

		internal Vector3 debugOriYellow = Vector3.zero;

		internal Vector3 debugDirRed = Vector3.zero;

		internal Vector3 debugOriRed = Vector3.zero;

		internal Vector3 debugDirBlue = Vector3.zero;

		internal Vector3 debugOriBlue = Vector3.zero;

		internal AudioSource playerUltraBoostLoopAudioSource;

		internal AudioSource playerMovementLoopAudioSource;

		internal AudioSource playerAccessoryAudioSource;

		internal AudioSource playerOneShotAudioSource;

		internal AudioSource playerSlideAudioSource;

		internal AudioSource playerGrindLoopAudioSource;

		internal AudioSource playerGameplayVoicesAudioSource;

		internal VoicePriority currentVoicePriority;

		private CharacterConstructor characterConstructor;

		private AudioManager audioManager;

		private StoryManager storyManager;

		private MoveStyle movestyleAudioCurrent;

		private AudioClip movestyleAudioTargetClip;

		internal GameObject hitboxLeftLeg;

		internal GameObject hitboxRightLeg;

		internal GameObject hitboxUpperBody;

		private PlayerAchievements playerAchievements;

		private const float safeLocationsHistoryLength = 0.5f;

		private const float saveLocationConsiderationTime = 1f;

		private float timeGrounded;

		private float overwriteFirstSafeLocationTime;

		public float soundRangeMin;

		public float soundRangeMax;

		internal List<Ability> abilities;

		internal AirDashAbility airDashAbility;

		internal SlideAbility slideAbility;

		internal WallrunLineAbility wallrunAbility;

		internal AirTrickAbility airTrickAbility;

		internal GroundTrickAbility groundTrickAbility;

		internal SwitchMoveStyleAbility switchMoveStyleAbility;

		internal TalkAbility talkAbility;

		internal DanceAbility danceAbility;

		internal CharacterSelectAbility characterSelectAbility;

		internal GrindAbility grindAbility;

		internal HandplantAbility handplantAbility;

		internal SpecialAirAbility specialAirAbility;

		internal SitAbility sitAbility;

		internal HeadspinAbility headspinAbility;

		internal DieAbility dieAbility;

		internal HitBounceAbility hitBounceAbility;

		internal LedgeClimbAbility ledgeClimbAbility;

		internal BoostAbility boostAbility;

		internal KnockbackAbility knockbackAbility;

		internal CaughtAbility caughtAbility;

		internal RecoverAbility recoverAbility;

		internal StandbyAbility standbyAbility;

		internal PseudoGraffitiAbility pseudoGraffitiAbility;

		internal FlipOutJumpAbility flipOutJumpAbility;

		internal VertAbility vertAbility;

		internal float abilityTimer;

		internal Ability ability;

		internal Ability preAbility;

		internal float hitpause;

		internal Vector3 preHitpauseVel;

		internal bool preHitpauseGrounded;

		[Header("Abilities")]
		public GameObject hitbox;

		public GameObject airialHitbox;

		public GameObject radialHitbox;

		public GameObject sprayHitbox;

		public GameObject obstacleDetection;

		public GameObject autoAimCheck;

		public PlayableDirector CharacterSelectNoSpaceSequence;

		public PlayableDirector CharacterSelectWanted;

		public PlayableDirector CharacterSelectFinished;

		internal bool phoneLocked;

		internal bool spraycanLocked;

		internal bool fortuneAppLocked;

		internal bool cameraAppLocked;

		internal float camCustomLookAtOffset;

		internal const int trickingLength = 3;

		public bool smoothRotation;

		private Quaternion visualRotOrig = Quaternion.identity;

		private Quaternion visualRotTarget = Quaternion.identity;

		private Quaternion? visualRotLocalOverride;

		private float visualRotOrigTime;

		private float visualRotTargetTime;

		private Quaternion visualUp;

		public Dictionary<int, AnimInfo>[] animInfosSets = new Dictionary<int, AnimInfo>[5];

		internal int curAnim;

		internal bool firstFrameAnim;

		[Header("Animation")]
		public RuntimeAnimatorController animatorController;

		public RuntimeAnimatorController animatorControllerBMX;

		public RuntimeAnimatorController animatorControllerSkateboard;

		public RuntimeAnimatorController animatorControllerSkates;

		internal Animator anim;

		internal int charTestCycleNum;

		internal int moveStyleTestCycleNum;

		internal int outfitTestCycleNum;

		private float startRunAnimTime;

		internal Transform phoneDirBone;

		private float spraycanLayerWeight;

		private float phoneLayerWeight;

		internal float rotLean;

		private float rotLeanSecondary;

		private float rotLeanCurve;

		private float rotLeanCurveSmooth;

		private float rotLeanSkateboard;

		private float rotLeanSkateboardSmooth;

		public float rotLeanFactor = 20f;

		public float grabPhoneSpeed = 5.2f;

		public float grabSpraycanSpeed = 2.56f;

		private bool newSpraycan = true;

		private GameObject lookAtSubject;

		private float curAnimActiveTime;

		private float idleVariationAfter = 8f;

		private float blinkTimer = 4.6f;

		private float blinkDuration = 0.2f;

		public bool blink;

		private Vector2 customPhoneHandValue;

		private int canSprayHash;

		private int canShakeHash;

		private int canStartShakeHash;

		private int landRunHash;

		private int startRunHash;

		private int stopRunHash;

		private int walkHash;

		private int runHash;

		private int fallHash;

		private int idleHash;

		private int idleFidget1Hash;

		private int phoneDirectionXHash;

		private int phoneDirectionYHash;

		private int turnDirectionXHash;

		private int turnDirectionX2Hash;

		private int turnDirectionX3Hash;

		private int turnDirectionSkateboardHash;

		private int bounceHash;

		private int softBounce1Hash;

		private int softBounce2Hash;

		private int softBounce3Hash;

		private int softBounce4Hash;

		private int softBounce5Hash;

		private int softBounce6Hash;

		private int softBounce7Hash;

		private int softBounce8Hash;

		private int softBounce9Hash;

		private int softBounce10Hash;

		private int softBounce11Hash;

		private int softBounce12Hash;

		private int softBounce13Hash;

		private int softBounce14Hash;

		private int softBounce15Hash;

		private int softBounce16Hash;

		private int softBounce17Hash;

		private int softBounce18Hash;

		private float rotLeanPrevious;

		private float rotLeanSecondaryPrevious;

		private float rotLeanCurveSmoothPrevious;

		private float rotLeanSkateboardSmoothPrevious;

		private SpraycanState spraycanState;

		private List<GraffitiSpot> graffitiSpotsAvailable = new List<GraffitiSpot>();

		public Transform closestEnemy;

		private float closestEnemyDist = float.MaxValue;

		[SerializeField]
		private GameObject chainBreakEffect;

		[SerializeField]
		private GameObject cuffBreakEffect;

		[SerializeField]
		private GameObject cuffPrefab;

		internal int maxCuffs = 5;

		internal CuffMissileEnemy[] attachedCuffs = new CuffMissileEnemy[5];

		internal float outOfCombatTimer;

		internal float claimedTimer;

		internal float maxHP = 6f;

		internal float HP;

		private int breakCuffPower;

		private float dieTimer;

		private float outOfCombatDuration = 3f;

		private float recoverDamageTimer;

		private float recoverDamageInterval = 3.5f;

		private float lastHit;

		private float invulnerabilityDuration = 0.35f;

		private PlayerCuff[] cuffs = new PlayerCuff[5];

		private Coroutine doSmallHitCoroutine;

		private int getHitSmallHash;

		private float originalFontSize;

		private float prefChargeValue;

		internal int graffitiContextAvailable;

		internal int talkContextAvailable;

		internal int breakChainContextAvailable;

		public LayerMask uiIndicatorOcclusionLayerMask;

		[SerializeField]
		private float UIIndicatorAppearDelay = 5f;

		[SerializeField]
		public float grafSpotUIIndicatorRange = 10000f;

		[SerializeField]
		private bool occludeGrafSpotUIIndicators;

		private int showAddRep;

		private float showAddRepTimer;

		internal float showRepLingerTimer;

		private float rep;

		private float showFreshTimer;

		private float showStaleTimer;

		private float showLastScoreTimer;

		private float showLastScoreDuration = 2.2f;

		private float showLastScoreAnimationTimer;

		private float showLastScoreAnimationDuration = 0.165f;

		private float showAddCharge;

		private float showInformationTimer;

		private float showMusicUnlockInformationTimer;

		private float showMusicUnlockInformationDuration;

		private string showMusicUnlockInformationTitle;

		private string showMusicUnlockInformationArtist;

		private float showingREP;

		private float showReqREP;

		private bool showAPB;

		private float healthBarAnimTimer = 1f;

		private float prefFillAmount = 1f;

		private float safeTimer;

		private bool safe = true;

		private Vector3 currentTrickUIStartPos;

		private Vector3 healthBarStartPos;

		private List<PickupDisplay> pickupDisplays = new List<PickupDisplay>();

		private const float pickupDuration = 0.35f;

		private List<GrafSpotUIIndicatorData> grafSpotUIIndicatorDatas = new List<GrafSpotUIIndicatorData>();

		private UIIndicatorData[] storyUIIndicatorData = new UIIndicatorData[20];

		private int amountOfActiveStoryData;

		internal GameplayUI ui;

		private StringBuilder repStringBuilder;

		private ComboMascotSystem currentComboMascotSystem;

		private CultureInfo currentCulture;

		private IGameTextLocalizer localizer;

		private string noTricksUIText = string.Empty;

		private string trickComboUIText = string.Empty;

		private string trickUIText = string.Empty;

		private const string MULTIPLIER_FORMAT = "{0} X";

		private const string TRICKS_IN_COMBO_FORMAT = "{0} {1}";

		private const string COMBO_MASCOT_SCORE_FORMAT = "{0}/{1}";

		private const string NO_COMBO_LOCALIZATION_KEY = "GAMEPLAY_NO_COMBO";

		private const string TRICK_COMBO_LOCALIZATION_KEY = "GAMEPLAY_TRICK_COMBO";

		private const string TRICK_LOCALIZATION_KEY = "GAMEPLAY_TRICK";

		private float scoreCalcPreviousFrame = -1f;

		private float scoreMultiplierPreviousFrame = -1f;

		private float tricksInComboPreviousFrame = -1f;

		public GraffitiGame graffitiGamePrefab;

		public CursorLockMode graffitiGameLockMode;

		internal bool inGraffitiGame;

		private bool ignoreCamInput;

		internal Vector3 preVelocity;

		internal Vector3 preDir;

		public Vector3 reallyMoved;

		public Vector3 posPrev;

		public Vector3 reallyMovedVelocity;

		[Header("Movement")]
		public MovementStats trickingMovementStats;

		public MovementStats bmxMovementStats;

		public MovementStats skateboardMovementStats;

		public MovementStats inlineMovementStats;

		public MovementStats specialSkateboardMovementStats;

		internal MovementStats stats;

		internal float runSpeedModifier;

		internal MovementType targetMovement = MovementType.NONE;

		public float aboveMaxAirDecc;

		public float vertDecc;

		public float normalBoostSpeed;

		public float boostCharge;

		internal float maxBoostCharge = 80f;

		internal bool boosting;

		internal float boostTrickCost = 13f;

		public float aimBackForAirBrake;

		internal bool inSpeedZone;

		internal bool inWalkZone;

		internal CharacterSelectSpot inCharacterSelectSpot;

		internal Transform onLauncher;

		internal GameplayCamera cam;

		public MovementMotor motor;

		internal MoveStyle moveStyle;

		internal MoveStyle moveStyleEquipped = MoveStyle.MAX;

		internal bool usingEquippedMovestyle;

		internal bool switchToEquippedMovestyleLocked;

		[Header("Jumping")]
		public float jumpSpeed;

		public float bonusJumpSpeedHandplantJump = 0.9f;

		public float bonusJumpSpeedGrind = 1.5f;

		public float bonusJumpSpeedWallrun = 2f;

		internal float abilityShorthopFactor = 0.5f;

		public float jumpSpeedLauncher = 3f;

		public float JumpPreGroundingGraceTime;

		public float JumpPostGroundingGraceTime;

		internal float lastElevationForSlideBoost;

		internal Vector3 moveInput;

		internal Vector2 moveInputPlain;

		internal Vector3 moveTap;

		internal Vector2 preMoveInputPlain;

		internal bool isJumping;

		internal bool jumpRequested;

		internal bool jumpConsumed;

		internal bool jumpedThisFrame;

		internal bool maintainSpeedJump;

		internal bool jumpLow;

		internal bool skipLanding;

		internal float timeSinceJumpRequested = float.PositiveInfinity;

		internal float timeSinceLastJump = float.PositiveInfinity;

		internal float timeSinceLastAbleToJump;

		internal float timeSinceGrinding = float.PositiveInfinity;

		internal float timeSinceWallrunning = float.PositiveInfinity;

		private int landHash;

		private int jumpHash;

		internal SequenceState sequenceState = SequenceState.NONE;

		private int preStopAnimState;

		private float preStopAnimTime;

		private Vector3 prePauseVelocity;

		internal float score;

		internal float scoreFactor = 1f;

		internal TrickType currentTrickType;

		private bool currentTrickOnFoot;

		private string currentTrickName;

		internal int currentTrickPoints;

		internal int tricksInLastCombo;

		internal int tricksInCombo;

		internal bool didAbilityTrick;

		internal float baseScore;

		internal float scoreMultiplier;

		internal float lastScore;

		internal int lastCornered;

		private List<GrindNode> corneredNodes = new List<GrindNode>();

		private List<Collider> comboColliders = new List<Collider>();

		private List<string> graffitiTitlesDone = new List<string>();

		internal float comboTimeOutTimer = 1f;

		private const float comboTimeOutDuration = 1f;

		private float comboTimeOutShowResetTimer;

		private const int standardPointsGraffitiSmall = 500;

		private const int standardPointsGraffitiMedium = 1000;

		private const int standardPointsGraffitiLarge = 1600;

		private const int standardPointsGraffitiExtralarge = 2000;

		internal int newGraffitiBonusMulti = 1;

		private const int newGraffitiBonusMultiAmount = 2;

		private float moveInScoreTimer;

		public AnimationCurve gainBoostChargeCurve;

		private List<Trick> groundTricks = new List<Trick>
		{
			new Trick(140),
			new Trick(140),
			new Trick(140)
		};

		private List<Trick> airTricks = new List<Trick>
		{
			new Trick(80),
			new Trick(80),
			new Trick(80)
		};

		private List<Trick> airVertTricks = new List<Trick>
		{
			new Trick(200),
			new Trick(200),
			new Trick(200)
		};

		private List<Trick> airBoostTricks = new List<Trick>
		{
			new Trick(3000, 500, 500),
			new Trick(3000, 500, 500),
			new Trick(3000, 500, 500)
		};

		private List<Trick> groundBoostTricks = new List<Trick>
		{
			new Trick(2000, 400, 400),
			new Trick(2000, 400, 400),
			new Trick(2000, 400, 400)
		};

		private List<Trick> grindTricks = new List<Trick>
		{
			new Trick(120),
			new Trick(120),
			new Trick(120)
		};

		private List<Trick> grindStartTricks = new List<Trick>
		{
			new Trick(80),
			new Trick(80),
			new Trick(80)
		};

		private List<Trick> grindBoostTricks = new List<Trick>
		{
			new Trick(1600, 400, 400),
			new Trick(1600, 400, 400),
			new Trick(1600, 400, 400)
		};

		private Trick specialAirTrick = new Trick(400, 400, 0);

		private Trick slideTrick = new Trick(4, 4, 0);

		private Trick wallrunTrick = new Trick(200, 25, 50);

		private Trick handplantTrick = new Trick(200, 200, 0);

		private Trick hitbounceTrick = new Trick(60, 60, 0);

		private Trick poleFlipTrick = new Trick(50);

		private VertShape vertShape;

		private Vector3 vertNormal;

		private float vertSpeed;

		private float vertPosYRel;

		private Vector2 vertPos;

		private Vector2 vertDir;

		private bool vertGoJump;

		[Header("Vert")]
		public float vertRollDownForce = 5f;

		public float vertMaxSpeed = 15f;

		public float vertJumpAway = 0.01f;

		public float vertMaxSideJumpAngle = 38f;

		public float vertTopJumpSpeed = 16f;

		public float vertSteepIfAbove = 0.5f;

		public float vertBottomExitSpeed = 15f;

		public float vertBottomExitSpeedThreshold = 6f;

		private bool preBoostpackActive;

		[Header("Visuals")]
		internal Transform visualTf;

		internal Transform headTf;

		internal float heightToHead;

		internal Characters character = Characters.NONE;

		internal CharacterVisual characterVisual;

		private Mesh characterMesh;

		[Header("Visuals")]
		public PlayerVisualEffects VFXPrefabs;

		public PlayerMoveStyleProps MoveStylePropsPrefabs;

		private float boostTrailTimer;

		private float trailWidth;

		private TrailRenderer boostpackTrail;

		internal ParticleSystem spraypaintParticles;

		internal ParticleSystem ringParticles;

		private float boostpackTrailDefaultWidth;

		private float boostpackTrailDefaultTime;

		public CharacterConstructor CharacterConstructor => characterConstructor;

		public AudioManager AudioManager => audioManager;

		public bool IsSelectingCharacter
		{
			get
			{
				if (ability != null)
				{
					return ability.Equals(characterSelectAbility);
				}
				return false;
			}
		}

		public Dictionary<int, AnimInfo> animInfos => animInfosSets[(int)moveStyle];

		public Vector2 PhoneHand
		{
			get
			{
				return customPhoneHandValue;
			}
			set
			{
				customPhoneHandValue = Vector2.ClampMagnitude(value, 1f);
			}
		}

		public float forwardInput => Vector3.Dot(moveInput, base.transform.forward);

		internal Vector3 dir => motor.dir;

		internal Vector3 forward => tf.forward;

		internal Vector3 capsuleCenter => tf.position + motor.GetCapsule().center;

		internal Vector3 capsuleQuarter => tf.position + motor.GetCapsule().center / 2f;

		public float maxMoveSpeed => stats.runSpeed + runSpeedModifier;

		public float minMoveSpeed => stats.walkSpeed;

		public float boostSpeed => normalBoostSpeed;

		public bool preSprayButtonHeld { get; private set; }

		public bool sprayButtonHeld { get; private set; }

		public bool sprayButtonNew { get; private set; }

		public bool danceButtonHeld { get; private set; }

		public bool danceButtonNew { get; private set; }

		public bool boostButtonHeld { get; private set; }

		public bool boostButtonNew { get; private set; }

		public bool switchStyleButtonHeld { get; private set; }

		public bool switchStyleButtonNew { get; private set; }

		public bool jumpButtonHeld { get; private set; }

		public bool jumpButtonNew { get; private set; }

		public bool trick1ButtonHeld { get; private set; }

		public bool trick1ButtonNew { get; private set; }

		public bool trick2ButtonHeld { get; private set; }

		public bool trick2ButtonNew { get; private set; }

		public bool trick3ButtonHeld { get; private set; }

		public bool trick3ButtonNew { get; private set; }

		public bool slideButtonHeld { get; private set; }

		public bool slideButtonNew { get; private set; }

		public float moveAxisX { get; private set; }

		public float moveAxisY { get; private set; }

		public float cameraAxisX { get; private set; }

		public float cameraAxisY { get; private set; }

		public float cameraAxisZoom { get; private set; }

		public bool resetCameraNew { get; private set; }

		public bool phoneButtonNew { get; private set; }

		public bool walkButtonHeld { get; private set; }

		public bool walkButtonNew { get; private set; }

		public bool IsPerformingManualTrick
		{
			get
			{
				if (currentTrickType == TrickType.SLIDE)
				{
					return slideAbility.IsPerformingManual;
				}
				return false;
			}
		}

		public static event OnExitTriggerHitHandle OnExitTriggerHit;

		public event OnReviveHandler OnRevived;

		public void InitAnimInfosBMX()
		{
			moveStyle = MoveStyle.BMX;
			int key = Animator.StringToHash("land");
			int key2 = Animator.StringToHash("halfStartRun");
			int key3 = Animator.StringToHash("halfStopRun");
			int key4 = Animator.StringToHash("turnRun");
			new AnimInfo(this, "walk")
			{
				fadeFrom = 
				{
					{ runHash, 0.2f },
					{ idleHash, 0.1f },
					{ key, 0.35f },
					{ stopRunHash, 0.4f },
					{ startRunHash, 0.4f },
					{ key2, 1f },
					{ key3, 1f }
				}
			};
			new AnimInfo(this, "groundTrick0", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundTrick1", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundTrick2", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundTrick3", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundBoostTrick0", AnimType.GROUNDTRICK, "run", 0.75f, 0f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.1f }
				}
			};
			new AnimInfo(this, "groundBoostTrick1", AnimType.GROUNDTRICK, "run", 0.75f, 0f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.1f }
				}
			};
			new AnimInfo(this, "groundBoostTrick2", AnimType.GROUNDTRICK, "run", 0.75f, 0f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.1f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleMoving", AnimType.NORMAL, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "land", AnimType.LAND, "idle", 0f, 0f, 0.9f)
			{
				fadeFrom = 
				{
					{ walkHash, 0.25f },
					{ runHash, 0.2f }
				},
				fadeTo = { { idleHash, 0.4f } },
				feetIK = true
			};
			new AnimInfo(this, "run", AnimType.RUN)
			{
				fadeFrom = 
				{
					{ walkHash, 0.1f },
					{ key4, 0.3f },
					{ stopRunHash, 0.25f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "startRun", AnimType.RUN, "run")
			{
				fadeFrom = 
				{
					{ idleHash, 0.05f },
					{ walkHash, 0.05f }
				},
				skipStartrun = true
			};
			int key5 = Animator.StringToHash("jump");
			int key6 = Animator.StringToHash("airTrick0");
			int key7 = Animator.StringToHash("airTrick1");
			int key8 = Animator.StringToHash("airTrick2");
			int key9 = Animator.StringToHash("airTrick3");
			int key10 = Animator.StringToHash("airDash");
			int key11 = Animator.StringToHash("wallRunLeft");
			int key12 = Animator.StringToHash("wallRunRight");
			int key13 = Animator.StringToHash("grindTrick0Hold");
			int key14 = Animator.StringToHash("grindTrick1Hold");
			int key15 = Animator.StringToHash("grindTrick2Hold");
			int key16 = Animator.StringToHash("grindBoostTrick0Hold");
			int key17 = Animator.StringToHash("grindBoostTrick1Hold");
			int key18 = Animator.StringToHash("grindBoostTrick2Hold");
			int key19 = Animator.StringToHash("grindBoostTrick0");
			int key20 = Animator.StringToHash("grindBoostTrick1");
			int key21 = Animator.StringToHash("grindBoostTrick2");
			int key22 = Animator.StringToHash("groundBoostTrick0");
			int key23 = Animator.StringToHash("groundBoostTrick1");
			int key24 = Animator.StringToHash("groundBoostTrick2");
			int key25 = Animator.StringToHash("grindTrick3Hold");
			int key26 = Animator.StringToHash("vaultSlide");
			int key27 = Animator.StringToHash("boostRun");
			int key28 = Animator.StringToHash("groundTrick0");
			int key29 = Animator.StringToHash("groundTrick1");
			int key30 = Animator.StringToHash("groundTrick2");
			int key31 = Animator.StringToHash("groundTrick3");
			int key32 = Animator.StringToHash("grindTrick0");
			int key33 = Animator.StringToHash("grindTrick1");
			int key34 = Animator.StringToHash("grindTrick2");
			int key35 = Animator.StringToHash("grindTrick3");
			int key36 = Animator.StringToHash("slide");
			int key37 = Animator.StringToHash("roll");
			int key38 = Animator.StringToHash("slideToRun");
			int key39 = Animator.StringToHash("boostBrake");
			int key40 = Animator.StringToHash("startBoost");
			int key41 = Animator.StringToHash("switchToEquippedMovestyleGrind");
			int key42 = Animator.StringToHash("switchToEquippedMovestyleMoving");
			new AnimInfo(this, "fall", AnimType.NORMAL, "fallIdle")
			{
				fadeFrom = 
				{
					{ runHash, 0.5f },
					{ walkHash, 0.5f },
					{ idleHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ startRunHash, 0.5f },
					{ key5, 0.2f },
					{ key6, 0.2f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.5f },
					{ key11, 0.3f },
					{ key12, 0.3f },
					{ key13, 0.25f },
					{ key14, 0.25f },
					{ key15, 0.25f },
					{ key25, 0.25f },
					{ key16, 0.25f },
					{ key17, 0.25f },
					{ key18, 0.25f },
					{ key19, 0.25f },
					{ key20, 0.25f },
					{ key21, 0.25f },
					{ key22, 0.5f },
					{ key23, 0.5f },
					{ key24, 0.5f },
					{ key, 0.5f },
					{ landRunHash, 0.5f },
					{ key26, 0.4f },
					{ key27, 1f },
					{ key28, 0.5f },
					{ key29, 0.5f },
					{ key30, 0.5f },
					{ key31, 0.5f },
					{ key32, 0.5f },
					{ key33, 0.5f },
					{ key34, 0.5f },
					{ key35, 0.5f },
					{ key36, 0.35f },
					{ key37, 0.35f },
					{ key38, 0.4f },
					{ key39, 0.2f },
					{ key40, 0.4f },
					{ key41, 0.5f },
					{ key42, 0.5f }
				}
			};
			AnimInfo animInfo = new AnimInfo(this, "grindTrick0", AnimType.NORMAL, "grindTrick0Hold", 0.5f, 0f, 0.75f);
			animInfo.fadeTo.Add(key32, 0.15f);
			animInfo.fadeTo.Add(key33, 0.15f);
			animInfo.fadeTo.Add(key34, 0.15f);
			animInfo.fadeFrom.Add(key13, 0.1f);
			animInfo.fadeFrom.Add(key14, 0.1f);
			animInfo.fadeFrom.Add(key15, 0.1f);
			animInfo.fadeFrom.Add(key25, 0.1f);
			animInfo.fadeTo.Add(key13, 0.2f);
			AnimInfo animInfo2 = new AnimInfo(this, "grindTrick1", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f);
			animInfo2.fadeTo.Add(key32, 0.15f);
			animInfo2.fadeTo.Add(key33, 0.15f);
			animInfo2.fadeTo.Add(key34, 0.15f);
			animInfo2.fadeFrom.Add(key13, 0.1f);
			animInfo2.fadeFrom.Add(key14, 0.1f);
			animInfo2.fadeFrom.Add(key15, 0.1f);
			animInfo2.fadeFrom.Add(key25, 0.1f);
			animInfo2.fadeTo.Add(key14, 0.2f);
			AnimInfo animInfo3 = new AnimInfo(this, "grindTrick2", AnimType.NORMAL, "grindTrick2Hold", 0.5f, 0f, 0.75f);
			animInfo3.fadeTo.Add(key32, 0.15f);
			animInfo3.fadeTo.Add(key33, 0.15f);
			animInfo3.fadeTo.Add(key34, 0.15f);
			animInfo3.fadeFrom.Add(key13, 0.1f);
			animInfo3.fadeFrom.Add(key14, 0.1f);
			animInfo3.fadeFrom.Add(key15, 0.1f);
			animInfo3.fadeFrom.Add(key25, 0.1f);
			animInfo3.fadeTo.Add(key15, 0.2f);
			AnimInfo animInfo4 = new AnimInfo(this, "grindTrick3", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f);
			animInfo4.fadeTo.Add(key32, 0.15f);
			animInfo4.fadeTo.Add(key33, 0.15f);
			animInfo4.fadeTo.Add(key34, 0.15f);
			animInfo4.fadeFrom.Add(key13, 0.1f);
			animInfo4.fadeFrom.Add(key14, 0.1f);
			animInfo4.fadeFrom.Add(key15, 0.1f);
			animInfo4.fadeFrom.Add(key25, 0.1f);
			animInfo4.fadeTo.Add(key25, 0.2f);
			new AnimInfo(this, "grindRetourEnd", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f)
			{
				fadeTo = 
				{
					{ key32, 0.15f },
					{ key33, 0.15f },
					{ key34, 0.15f },
					{ key25, 0.2f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleGrind", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key14, 0.2f);
			int key43 = Animator.StringToHash("fallIdle");
			int key44 = Animator.StringToHash("poleFreezeIdle");
			int key45 = Animator.StringToHash("idleFidget2");
			int key46 = Animator.StringToHash("dance1");
			new AnimInfo(this, "fallIdle", AnimType.NORMAL, "", -1f);
			new AnimInfo(this, "poleFlip", AnimType.NORMAL, "fall", -1f);
			new AnimInfo(this, "hitBounce", AnimType.NORMAL, "fallIdle", -1f).fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "poleFreeze", AnimType.NORMAL, "poleFreezeIdle", 0f, 0f, 0.9f).fadeTo.Add(key44, 0.3f);
			new AnimInfo(this, "grindBoostTrick0", AnimType.NORMAL, "grindBoostTrick0Hold", 0.75f, 0f, 0.75f).fadeTo.Add(key16, 0.2f);
			new AnimInfo(this, "grindBoostTrick1", AnimType.NORMAL, "grindBoostTrick1Hold", 0.75f, 0f, 0.75f).fadeTo.Add(key17, 0.2f);
			new AnimInfo(this, "grindBoostTrick2", AnimType.NORMAL, "grindBoostTrick2Hold", 0.75f, 0f, 0.75f).fadeTo.Add(key18, 0.2f);
			new AnimInfo(this, "jumpTrick1", AnimType.NORMAL, "fallIdle");
			new AnimInfo(this, "airTrick0", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airTrick1", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airTrick2", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airTrick3", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airBoostTrick0", AnimType.AIRTRICK, "fallIdle", -1f).fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airBoostTrick1", AnimType.AIRTRICK, "fallIdle", -1f).fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "airBoostTrick2", AnimType.AIRTRICK, "fallIdle", -1f).fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "landRun", AnimType.LAND, "run")
			{
				fadeTo = { { runHash, 0.25f } },
				fadeFrom = { { walkHash, 0.25f } },
				skipStartrun = true
			};
			new AnimInfo(this, "turnRun");
			new AnimInfo(this, "stopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ key26, 0.25f },
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ runHash, 0.5f },
					{ startRunHash, 0.25f },
					{ key38, 0.5f }
				},
				feetIK = true,
				skipStartrun = true
			};
			new AnimInfo(this, "halfStopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ startRunHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "boostBrake", AnimType.NORMAL, "idle").fadeTo.Add(startRunHash, 0.5f);
			new AnimInfo(this, "sit");
			new AnimInfo(this, "startSit", AnimType.NORMAL, "sit");
			new AnimInfo(this, "stopSit", AnimType.NORMAL, "idle");
			new AnimInfo(this, "idle", AnimType.IDLE)
			{
				fadeFrom = 
				{
					{ walkHash, 0.15f },
					{ key39, 0.3f },
					{ landRunHash, 0.2f },
					{ startRunHash, 0.2f },
					{ stopRunHash, 1f }
				},
				feetIK = true,
				fadeTo = 
				{
					{ idleFidget1Hash, 0.2f },
					{ key45, 0.2f }
				}
			};
			new AnimInfo(this, "idleFidget1", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true,
				fadeTo = { { idleHash, 0.2f } }
			};
			new AnimInfo(this, "idleFidget2", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true,
				fadeTo = { { idleHash, 0.2f } }
			};
			new AnimInfo(this, "airDash", AnimType.NORMAL, "fall");
			new AnimInfo(this, "roll", AnimType.NORMAL, "slide")
			{
				fadeFrom = 
				{
					{ key28, 0.3f },
					{ key29, 0.3f },
					{ key30, 0.3f },
					{ key22, 0.1f },
					{ key23, 0.1f },
					{ key24, 0.1f },
					{ idleHash, 0.2f },
					{ runHash, 0.2f }
				}
			};
			new AnimInfo(this, "slide").fadeFrom.Add(key37, 0.3f);
			new AnimInfo(this, "slideToRun", AnimType.NORMAL, "run", 0f, 0.3f, 0.6f)
			{
				skipStartrun = true,
				fadeTo = { { runHash, 0.5f } }
			};
			new AnimInfo(this, "vault", AnimType.NORMAL, "idle");
			new AnimInfo(this, "vaultSlide", AnimType.NORMAL, "idle")
			{
				fadeTo = { { runHash, 0.3f } },
				skipStartrun = true
			};
			new AnimInfo(this, "halfStartRun", AnimType.NORMAL, "run")
			{
				fadeFrom = 
				{
					{ stopRunHash, 0.2f },
					{ key3, 0.2f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "dance1", AnimType.OVERRULE_IDLE).fadeFrom.Add(idleHash, 0.15f);
			new AnimInfo(this, "dance2", AnimType.OVERRULE_IDLE).fadeFrom.Add(key46, 0.1f);
			new AnimInfo(this, "dance3", AnimType.OVERRULE_IDLE).fadeFrom.Add(key46, 0.1f);
			new AnimInfo(this, "dance4", AnimType.OVERRULE_IDLE).fadeFrom.Add(key46, 0.1f);
			new AnimInfo(this, "headSpinRStart", AnimType.NORMAL, "headSpinR");
			new AnimInfo(this, "headSpinRStop", AnimType.NORMAL, "idle");
			new AnimInfo(this, "headSpinLStart", AnimType.NORMAL, "headSpinL");
			new AnimInfo(this, "headSpinLStop", AnimType.NORMAL, "idle");
		}

		public void InitAnimInfosSkateboard()
		{
			moveStyle = MoveStyle.SKATEBOARD;
			int key = Animator.StringToHash("land");
			int key2 = Animator.StringToHash("halfStartRun");
			int key3 = Animator.StringToHash("halfStopRun");
			int key4 = Animator.StringToHash("turnRun");
			new AnimInfo(this, "walk")
			{
				fadeFrom = 
				{
					{ runHash, 0.2f },
					{ idleHash, 0.25f },
					{ key, 0.35f },
					{ stopRunHash, 0.4f },
					{ startRunHash, 0.4f },
					{ key2, 1f },
					{ key3, 1f }
				}
			};
			new AnimInfo(this, "grindTrick0Hold");
			new AnimInfo(this, "grindTrick1Hold");
			new AnimInfo(this, "grindTrick2Hold");
			new AnimInfo(this, "grindTrick3Hold");
			new AnimInfo(this, "groundBoostTrick0", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f)
			{
				skipStartrun = true,
				fadeTo = { { stopRunHash, 0.3f } }
			};
			new AnimInfo(this, "groundBoostTrick1", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f)
			{
				skipStartrun = true,
				fadeTo = { { stopRunHash, 0.3f } }
			};
			new AnimInfo(this, "groundBoostTrick2", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f)
			{
				skipStartrun = true,
				fadeTo = { { stopRunHash, 0.3f } }
			};
			new AnimInfo(this, "switchToEquippedMovestyleMoving", AnimType.NORMAL, "run")
			{
				skipStartrun = true,
				fadeTo = { { stopRunHash, 0.3f } }
			};
			new AnimInfo(this, "switchToEquippedMovestyleStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "land", AnimType.LAND, "idle", 0f, 0f, 0.9f)
			{
				fadeFrom = 
				{
					{ walkHash, 0.25f },
					{ runHash, 0.2f }
				},
				fadeTo = { { idleHash, 0.4f } },
				feetIK = true
			};
			new AnimInfo(this, "run", AnimType.RUN)
			{
				fadeFrom = 
				{
					{ walkHash, 0.05f },
					{ key4, 0.3f },
					{ stopRunHash, 0.05f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "startRun", AnimType.RUN, "run", 0f, 0.032f)
			{
				fadeFrom = 
				{
					{ idleHash, 0.05f },
					{ walkHash, 0.05f }
				},
				skipStartrun = true
			};
			int key5 = Animator.StringToHash("jump");
			int key6 = Animator.StringToHash("airTrick0");
			int key7 = Animator.StringToHash("airTrick1");
			int key8 = Animator.StringToHash("airTrick2");
			int key9 = Animator.StringToHash("airTrick3");
			int key10 = Animator.StringToHash("airDash");
			int key11 = Animator.StringToHash("wallRunLeft");
			int key12 = Animator.StringToHash("wallRunRight");
			int key13 = Animator.StringToHash("grindTrick0Hold");
			int key14 = Animator.StringToHash("grindTrick1Hold");
			int key15 = Animator.StringToHash("grindTrick2Hold");
			int key16 = Animator.StringToHash("grindTrick3Hold");
			int key17 = Animator.StringToHash("grindBoostTrick0");
			int key18 = Animator.StringToHash("grindBoostTrick1");
			int key19 = Animator.StringToHash("grindBoostTrick2");
			int key20 = Animator.StringToHash("grindBoostTrick0Hold");
			int key21 = Animator.StringToHash("grindBoostTrick1Hold");
			int key22 = Animator.StringToHash("grindBoostTrick2Hold");
			int key23 = Animator.StringToHash("vaultSlide");
			int key24 = Animator.StringToHash("boostRun");
			int key25 = Animator.StringToHash("groundTrick0");
			int key26 = Animator.StringToHash("groundTrick1");
			int key27 = Animator.StringToHash("groundTrick2");
			int key28 = Animator.StringToHash("groundTrick3");
			int key29 = Animator.StringToHash("groundBoostTrick0");
			int key30 = Animator.StringToHash("groundBoostTrick1");
			int key31 = Animator.StringToHash("groundBoostTrick2");
			int key32 = Animator.StringToHash("grindTrick0");
			int key33 = Animator.StringToHash("grindTrick1");
			int key34 = Animator.StringToHash("grindTrick2");
			int key35 = Animator.StringToHash("grindTrick3");
			int key36 = Animator.StringToHash("slide");
			int key37 = Animator.StringToHash("roll");
			int key38 = Animator.StringToHash("slideToRun");
			int key39 = Animator.StringToHash("boostBrake");
			int key40 = Animator.StringToHash("startBoost");
			int key41 = Animator.StringToHash("switchToEquippedMovestyleGrind");
			int key42 = Animator.StringToHash("switchToEquippedMovestyleMoving");
			new AnimInfo(this, "fall", AnimType.NORMAL, "fallIdle")
			{
				fadeFrom = 
				{
					{ runHash, 0.05f },
					{ walkHash, 0.5f },
					{ idleHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ startRunHash, 0.5f },
					{ key5, 0.2f },
					{ key6, 0.2f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.4f },
					{ key11, 0.75f },
					{ key12, 0.75f },
					{ key13, 0.25f },
					{ key14, 0.25f },
					{ key15, 0.25f },
					{ key16, 0.25f },
					{ key17, 0.25f },
					{ key18, 0.25f },
					{ key19, 0.25f },
					{ key20, 0.25f },
					{ key21, 0.25f },
					{ key22, 0.25f },
					{ key, 0.5f },
					{ landRunHash, 0.5f },
					{ key23, 0.4f },
					{ key24, 1f },
					{ key25, 0.5f },
					{ key26, 0.5f },
					{ key27, 0.5f },
					{ key28, 0.5f },
					{ key29, 0.5f },
					{ key30, 0.5f },
					{ key31, 0.5f },
					{ key32, 0.5f },
					{ key33, 0.5f },
					{ key34, 0.5f },
					{ key35, 0.5f },
					{ key36, 0.35f },
					{ key37, 0.4f },
					{ key38, 0.4f },
					{ key39, 0.2f },
					{ key40, 0.4f },
					{ key41, 0.5f },
					{ key42, 0.5f }
				}
			};
			AnimInfo animInfo = new AnimInfo(this, "grindTrick0", AnimType.NORMAL, "grindTrick0Hold", 0.5f, 0f, 0.75f);
			animInfo.fadeTo.Add(key32, 0.15f);
			animInfo.fadeTo.Add(key33, 0.15f);
			animInfo.fadeTo.Add(key34, 0.15f);
			animInfo.fadeFrom.Add(key13, 0.1f);
			animInfo.fadeFrom.Add(key14, 0.1f);
			animInfo.fadeFrom.Add(key15, 0.1f);
			animInfo.fadeFrom.Add(key16, 0.1f);
			animInfo.fadeTo.Add(key13, 0.2f);
			AnimInfo animInfo2 = new AnimInfo(this, "grindTrick1", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f);
			animInfo2.fadeTo.Add(key32, 0.15f);
			animInfo2.fadeTo.Add(key33, 0.15f);
			animInfo2.fadeTo.Add(key34, 0.15f);
			animInfo2.fadeFrom.Add(key13, 0.1f);
			animInfo2.fadeFrom.Add(key14, 0.1f);
			animInfo2.fadeFrom.Add(key15, 0.1f);
			animInfo2.fadeFrom.Add(key16, 0.1f);
			animInfo2.fadeTo.Add(key14, 0.2f);
			AnimInfo animInfo3 = new AnimInfo(this, "grindTrick2", AnimType.NORMAL, "grindTrick2Hold", 0.5f, 0f, 0.75f);
			animInfo3.fadeTo.Add(key32, 0.15f);
			animInfo3.fadeTo.Add(key33, 0.15f);
			animInfo3.fadeTo.Add(key34, 0.15f);
			animInfo3.fadeFrom.Add(key13, 0.1f);
			animInfo3.fadeFrom.Add(key14, 0.1f);
			animInfo3.fadeFrom.Add(key15, 0.1f);
			animInfo3.fadeFrom.Add(key16, 0.1f);
			animInfo3.fadeTo.Add(key15, 0.2f);
			AnimInfo animInfo4 = new AnimInfo(this, "grindTrick3", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f);
			animInfo4.fadeTo.Add(key32, 0.15f);
			animInfo4.fadeTo.Add(key33, 0.15f);
			animInfo4.fadeTo.Add(key34, 0.15f);
			animInfo4.fadeFrom.Add(key13, 0.1f);
			animInfo4.fadeFrom.Add(key14, 0.1f);
			animInfo4.fadeFrom.Add(key15, 0.1f);
			animInfo4.fadeFrom.Add(key16, 0.1f);
			animInfo4.fadeTo.Add(key16, 0.2f);
			new AnimInfo(this, "switchToEquippedMovestyleGrind", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key14, 0.2f);
			new AnimInfo(this, "groundTrick0", AnimType.GROUNDTRICK, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ key25, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f }
				},
				fadeFrom = { { runHash, 0.01f } }
			};
			new AnimInfo(this, "groundTrick1", AnimType.GROUNDTRICK, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ key25, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f }
				}
			};
			new AnimInfo(this, "groundTrick2", AnimType.GROUNDTRICK, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ key25, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f }
				}
			};
			new AnimInfo(this, "groundTrick3", AnimType.GROUNDTRICK, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ key25, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f }
				}
			};
			new AnimInfo(this, "grindBoostTrick0", AnimType.NORMAL, "grindBoostTrick0Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key20, 0.2f);
			new AnimInfo(this, "grindBoostTrick1", AnimType.NORMAL, "grindBoostTrick1Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key21, 0.2f);
			new AnimInfo(this, "grindBoostTrick2", AnimType.NORMAL, "grindBoostTrick2Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key22, 0.2f);
			new AnimInfo(this, "grindRetourEnd", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f)
			{
				fadeTo = 
				{
					{ key32, 0.15f },
					{ key33, 0.15f },
					{ key34, 0.15f },
					{ key16, 0.2f }
				}
			};
			int key43 = Animator.StringToHash("fallIdle");
			Animator.StringToHash("poleFreezeIdle");
			int key44 = Animator.StringToHash("idleFidget2");
			int key45 = Animator.StringToHash("dance1");
			new AnimInfo(this, "fallIdle", AnimType.NORMAL, "", -1f);
			new AnimInfo(this, "poleFlip", AnimType.NORMAL, "fall", -1f);
			new AnimInfo(this, "hitBounce", AnimType.NORMAL, "fallIdle", -1f).fadeTo.Add(key43, 0.75f);
			new AnimInfo(this, "poleFreeze", AnimType.NORMAL, "poleFreezeIdle");
			new AnimInfo(this, "jumpTrick1", AnimType.NORMAL, "fallIdle");
			new AnimInfo(this, "airTrick0", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airTrick1", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airTrick2", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airTrick3", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airBoostTrick0", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airBoostTrick1", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "airBoostTrick2", AnimType.AIRTRICK, "fallIdle", -1f);
			new AnimInfo(this, "landRun", AnimType.LAND, "run")
			{
				fadeFrom = { { walkHash, 0.25f } },
				skipStartrun = true
			};
			new AnimInfo(this, "turnRun");
			new AnimInfo(this, "stopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ key23, 0.25f },
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ runHash, 0.04f },
					{ startRunHash, 0.2f },
					{ key38, 0.5f },
					{ landRunHash, 0.5f }
				},
				feetIK = true,
				skipStartrun = true
			};
			new AnimInfo(this, "halfStopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ startRunHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "boostBrake", AnimType.NORMAL, "idle")
			{
				fadeTo = { { startRunHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "sit");
			new AnimInfo(this, "startSit", AnimType.NORMAL, "sit");
			new AnimInfo(this, "stopSit", AnimType.NORMAL, "idle");
			new AnimInfo(this, "idle", AnimType.IDLE)
			{
				fadeFrom = 
				{
					{ walkHash, 0.15f },
					{ key39, 0.3f },
					{ landRunHash, 0.2f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 1f }
				},
				feetIK = true,
				fadeTo = 
				{
					{ idleFidget1Hash, 0.2f },
					{ key44, 0.2f }
				}
			};
			new AnimInfo(this, "idleFidget1", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true,
				fadeTo = { { idleHash, 0.2f } }
			};
			new AnimInfo(this, "idleFidget2", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true,
				fadeTo = { { idleHash, 0.2f } }
			};
			new AnimInfo(this, "airDash", AnimType.NORMAL, "fall");
			new AnimInfo(this, "roll", AnimType.NORMAL, "slide")
			{
				fadeFrom = 
				{
					{ key25, 0.2f },
					{ key26, 0.2f },
					{ key27, 0.2f },
					{ key29, 0.14f },
					{ key30, 0.14f },
					{ key31, 0.14f },
					{ idleHash, 0.1f },
					{ runHash, 0.025f }
				}
			};
			new AnimInfo(this, "slide").fadeFrom.Add(key37, 0.3f);
			new AnimInfo(this, "slideToRun", AnimType.NORMAL, "run").skipStartrun = true;
			new AnimInfo(this, "vault", AnimType.NORMAL, "idle");
			new AnimInfo(this, "vaultSlide", AnimType.NORMAL, "idle")
			{
				fadeTo = { { runHash, 0.04f } },
				skipStartrun = true
			};
			new AnimInfo(this, "halfStartRun", AnimType.NORMAL, "run")
			{
				fadeFrom = 
				{
					{ stopRunHash, 0.2f },
					{ key3, 0.2f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "dance1", AnimType.OVERRULE_IDLE).fadeFrom.Add(idleHash, 0.15f);
			new AnimInfo(this, "dance2", AnimType.OVERRULE_IDLE).fadeFrom.Add(key45, 0.1f);
			new AnimInfo(this, "dance3", AnimType.OVERRULE_IDLE).fadeFrom.Add(key45, 0.1f);
			new AnimInfo(this, "dance4", AnimType.OVERRULE_IDLE).fadeFrom.Add(key45, 0.1f);
			new AnimInfo(this, "headSpinRStart", AnimType.NORMAL, "headSpinR");
			new AnimInfo(this, "headSpinRStop", AnimType.NORMAL, "idle");
			new AnimInfo(this, "headSpinLStart", AnimType.NORMAL, "headSpinL");
			new AnimInfo(this, "headSpinLStop", AnimType.NORMAL, "idle");
		}

		public void InitAnimInfosSkates()
		{
			moveStyle = MoveStyle.INLINE;
			int key = Animator.StringToHash("land");
			int key2 = Animator.StringToHash("halfStartRun");
			int key3 = Animator.StringToHash("halfStopRun");
			int key4 = Animator.StringToHash("turnRun");
			new AnimInfo(this, "walk")
			{
				fadeFrom = 
				{
					{ runHash, 0.2f },
					{ idleHash, 0.1f },
					{ key, 0.35f },
					{ stopRunHash, 0.4f },
					{ startRunHash, 0.4f },
					{ key2, 1f },
					{ key3, 1f }
				}
			};
			new AnimInfo(this, "grindTrick0Hold");
			new AnimInfo(this, "grindTrick1Hold");
			new AnimInfo(this, "grindTrick2Hold");
			new AnimInfo(this, "grindTrick3Hold");
			new AnimInfo(this, "groundBoostTrick0", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundBoostTrick1", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "groundBoostTrick2", AnimType.GROUNDTRICK, "run", 0.75f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleMoving", AnimType.NORMAL, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "land", AnimType.LAND, "idle", 0f, 0f, 0.9f)
			{
				fadeFrom = 
				{
					{ walkHash, 0.25f },
					{ runHash, 0.2f }
				},
				fadeTo = { { idleHash, 0.4f } },
				feetIK = true
			};
			new AnimInfo(this, "run", AnimType.RUN)
			{
				fadeFrom = 
				{
					{ walkHash, 0.1f },
					{ key4, 0.3f },
					{ stopRunHash, 0.25f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "startRun", AnimType.RUN, "run")
			{
				fadeFrom = 
				{
					{ idleHash, 0.05f },
					{ walkHash, 0.05f }
				},
				skipStartrun = true
			};
			int key5 = Animator.StringToHash("jump");
			Animator.StringToHash("airTrick0");
			Animator.StringToHash("airTrick1");
			Animator.StringToHash("airTrick2");
			Animator.StringToHash("airTrick3");
			int key6 = Animator.StringToHash("airDash");
			int key7 = Animator.StringToHash("wallRunLeft");
			int key8 = Animator.StringToHash("wallRunRight");
			int key9 = Animator.StringToHash("grindTrick0Hold");
			int key10 = Animator.StringToHash("grindTrick1Hold");
			int key11 = Animator.StringToHash("grindTrick2Hold");
			int key12 = Animator.StringToHash("grindTrick3Hold");
			int key13 = Animator.StringToHash("grindBoostTrick0");
			int key14 = Animator.StringToHash("grindBoostTrick1");
			int key15 = Animator.StringToHash("grindBoostTrick2");
			int key16 = Animator.StringToHash("grindBoostTrick0Hold");
			int key17 = Animator.StringToHash("grindBoostTrick1Hold");
			int key18 = Animator.StringToHash("grindBoostTrick2Hold");
			int key19 = Animator.StringToHash("groundBoostTrick0");
			int key20 = Animator.StringToHash("groundBoostTrick1");
			int key21 = Animator.StringToHash("groundBoostTrick2");
			int key22 = Animator.StringToHash("vaultSlide");
			int key23 = Animator.StringToHash("boostRun");
			int key24 = Animator.StringToHash("groundTrick0");
			int key25 = Animator.StringToHash("groundTrick1");
			int key26 = Animator.StringToHash("groundTrick2");
			int key27 = Animator.StringToHash("groundTrick3");
			int key28 = Animator.StringToHash("grindTrick0");
			int key29 = Animator.StringToHash("grindTrick1");
			int key30 = Animator.StringToHash("grindTrick2");
			int key31 = Animator.StringToHash("grindTrick3");
			int key32 = Animator.StringToHash("slide");
			int key33 = Animator.StringToHash("roll");
			int key34 = Animator.StringToHash("slideToRun");
			int key35 = Animator.StringToHash("boostBrake");
			int key36 = Animator.StringToHash("startBoost");
			int key37 = Animator.StringToHash("switchToEquippedMovestyleGrind");
			int key38 = Animator.StringToHash("switchToEquippedMovestyleMoving");
			new AnimInfo(this, "fall", AnimType.NORMAL, "fallIdle")
			{
				fadeFrom = 
				{
					{ runHash, 0.5f },
					{ walkHash, 0.5f },
					{ idleHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ startRunHash, 0.5f },
					{ key5, 0.2f },
					{ key6, 0.5f },
					{ key7, 0.75f },
					{ key8, 0.75f },
					{ key9, 0.25f },
					{ key10, 0.25f },
					{ key11, 0.25f },
					{ key12, 0.25f },
					{ key, 0.5f },
					{ landRunHash, 0.5f },
					{ key22, 0.4f },
					{ key23, 1f },
					{ key24, 0.5f },
					{ key25, 0.5f },
					{ key26, 0.5f },
					{ key27, 0.5f },
					{ key28, 0.5f },
					{ key29, 0.5f },
					{ key30, 0.5f },
					{ key31, 0.5f },
					{ key13, 0.25f },
					{ key14, 0.25f },
					{ key15, 0.25f },
					{ key16, 0.25f },
					{ key17, 0.25f },
					{ key18, 0.25f },
					{ key19, 0.5f },
					{ key20, 0.5f },
					{ key21, 0.5f },
					{ key32, 0.35f },
					{ key33, 0.35f },
					{ key34, 0.4f },
					{ key35, 0.2f },
					{ key36, 0.4f },
					{ key37, 0.5f },
					{ key38, 0.5f }
				}
			};
			AnimInfo animInfo = new AnimInfo(this, "grindTrick0", AnimType.NORMAL, "grindTrick0Hold", 0.5f, 0f, 0.75f);
			animInfo.fadeTo.Add(key28, 0.15f);
			animInfo.fadeTo.Add(key29, 0.15f);
			animInfo.fadeTo.Add(key30, 0.15f);
			animInfo.fadeFrom.Add(key9, 0.1f);
			animInfo.fadeFrom.Add(key10, 0.1f);
			animInfo.fadeFrom.Add(key11, 0.1f);
			animInfo.fadeFrom.Add(key12, 0.1f);
			animInfo.fadeTo.Add(key9, 0.2f);
			AnimInfo animInfo2 = new AnimInfo(this, "grindTrick1", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f);
			animInfo2.fadeTo.Add(key28, 0.15f);
			animInfo2.fadeTo.Add(key29, 0.15f);
			animInfo2.fadeTo.Add(key30, 0.15f);
			animInfo2.fadeFrom.Add(key9, 0.1f);
			animInfo2.fadeFrom.Add(key10, 0.1f);
			animInfo2.fadeFrom.Add(key11, 0.1f);
			animInfo2.fadeFrom.Add(key12, 0.1f);
			animInfo2.fadeTo.Add(key10, 0.2f);
			new AnimInfo(this, "grindTrick2", AnimType.NORMAL, "grindTrick2Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key11, 0.2f);
			AnimInfo animInfo3 = new AnimInfo(this, "grindTrick3", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f);
			animInfo3.fadeTo.Add(key28, 0.15f);
			animInfo3.fadeTo.Add(key29, 0.15f);
			animInfo3.fadeTo.Add(key30, 0.15f);
			animInfo3.fadeFrom.Add(key9, 0.1f);
			animInfo3.fadeFrom.Add(key10, 0.1f);
			animInfo3.fadeFrom.Add(key11, 0.1f);
			animInfo3.fadeFrom.Add(key12, 0.1f);
			animInfo3.fadeTo.Add(key12, 0.2f);
			new AnimInfo(this, "switchToEquippedMovestyleGrind", AnimType.NORMAL, "grindTrick1Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key10, 0.2f);
			new AnimInfo(this, "groundTrick0", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f },
					{ key24, 0.1f },
					{ key25, 0.1f },
					{ key26, 0.1f }
				},
				fadeFrom = { { runHash, 0.01f } }
			};
			new AnimInfo(this, "groundTrick1", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f },
					{ key24, 0.1f },
					{ key25, 0.1f },
					{ key26, 0.1f }
				}
			};
			new AnimInfo(this, "groundTrick2", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f },
					{ key24, 0.1f },
					{ key25, 0.1f },
					{ key26, 0.1f }
				}
			};
			new AnimInfo(this, "groundTrick3", AnimType.GROUNDTRICK, "run", 0f, 0.95f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.25f },
					{ key24, 0.1f },
					{ key25, 0.1f },
					{ key26, 0.1f }
				}
			};
			new AnimInfo(this, "grindRetourEnd", AnimType.NORMAL, "grindTrick3Hold", 0.5f, 0f, 0.75f)
			{
				fadeTo = 
				{
					{ key28, 0.15f },
					{ key29, 0.15f },
					{ key30, 0.15f },
					{ key12, 0.2f }
				}
			};
			new AnimInfo(this, "grindBoostTrick0", AnimType.NORMAL, "grindBoostTrick0Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key16, 0.2f);
			new AnimInfo(this, "grindBoostTrick1", AnimType.NORMAL, "grindBoostTrick1Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key17, 0.2f);
			new AnimInfo(this, "grindBoostTrick2", AnimType.NORMAL, "grindBoostTrick2Hold", 0.5f, 0f, 0.75f).fadeTo.Add(key18, 0.2f);
			int key39 = Animator.StringToHash("fallIdle");
			int key40 = Animator.StringToHash("poleFreezeIdle");
			int key41 = Animator.StringToHash("idleFidget2");
			int key42 = Animator.StringToHash("dance1");
			new AnimInfo(this, "fallIdle", AnimType.NORMAL, "", -1f);
			new AnimInfo(this, "poleFlip", AnimType.NORMAL, "fall", -1f);
			new AnimInfo(this, "hitBounce", AnimType.NORMAL, "fallIdle", -1f).fadeTo.Add(key39, 0.75f);
			new AnimInfo(this, "poleFreeze", AnimType.NORMAL, "poleFreezeIdle", 0f, 0f, 0.9f).fadeTo.Add(key40, 0.3f);
			new AnimInfo(this, "jumpTrick1", AnimType.NORMAL, "fallIdle");
			new AnimInfo(this, "airTrick0", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.8f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airTrick1", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.8f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airTrick2", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.8f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airTrick3", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.8f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airBoostTrick0", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.85f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airBoostTrick1", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.85f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "airBoostTrick2", AnimType.AIRTRICK, "fallIdle", -1f, 0f, 0.85f).fadeTo.Add(key39, 0.5f);
			new AnimInfo(this, "landRun", AnimType.LAND, "run", 0f, 0f, 0.75f)
			{
				fadeTo = { { runHash, 0.25f } },
				fadeFrom = { { walkHash, 0.25f } },
				skipStartrun = true
			};
			new AnimInfo(this, "turnRun");
			new AnimInfo(this, "stopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ key22, 0.25f },
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ runHash, 0.5f },
					{ startRunHash, 0.25f },
					{ key34, 0.5f }
				},
				feetIK = true,
				skipStartrun = true
			};
			new AnimInfo(this, "halfStopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ startRunHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "boostBrake", AnimType.NORMAL, "idle")
			{
				fadeTo = { { startRunHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "sit");
			new AnimInfo(this, "startSit", AnimType.NORMAL, "sit");
			new AnimInfo(this, "stopSit", AnimType.NORMAL, "idle");
			new AnimInfo(this, "idle", AnimType.IDLE)
			{
				fadeFrom = 
				{
					{ walkHash, 0.15f },
					{ runHash, 0.25f },
					{ key35, 0.3f },
					{ landRunHash, 0.1f },
					{ startRunHash, 0.2f }
				},
				feetIK = true,
				fadeTo = 
				{
					{ idleFidget1Hash, 0.2f },
					{ key41, 0.2f }
				}
			};
			new AnimInfo(this, "idleFidget1", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "idleFidget2", AnimType.NORMAL, "idle")
			{
				fadeFrom = { { idleHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "airDash", AnimType.NORMAL, "fall");
			new AnimInfo(this, "roll", AnimType.NORMAL, "slide")
			{
				fadeFrom = 
				{
					{ key24, 0.2f },
					{ key26, 0.2f },
					{ key19, 0.14f },
					{ key20, 0.14f },
					{ key21, 0.14f }
				}
			};
			new AnimInfo(this, "slide").fadeFrom.Add(key33, 0.3f);
			new AnimInfo(this, "slideToRun", AnimType.NORMAL, "run", 0f, 0.3f, 0.85f)
			{
				skipStartrun = true,
				fadeTo = { { runHash, 0.3f } }
			};
			new AnimInfo(this, "vault", AnimType.NORMAL, "idle");
			new AnimInfo(this, "vaultSlide", AnimType.NORMAL, "idle")
			{
				fadeTo = { { runHash, 0.25f } },
				skipStartrun = true
			};
			new AnimInfo(this, "halfStartRun", AnimType.NORMAL, "run")
			{
				fadeFrom = 
				{
					{ stopRunHash, 0.2f },
					{ key3, 0.2f }
				},
				skipStartrun = true
			};
			new AnimInfo(this, "dance1", AnimType.OVERRULE_IDLE).fadeFrom.Add(idleHash, 0.15f);
			new AnimInfo(this, "dance2", AnimType.OVERRULE_IDLE).fadeFrom.Add(key42, 0.1f);
			new AnimInfo(this, "dance3", AnimType.OVERRULE_IDLE).fadeFrom.Add(key42, 0.1f);
			new AnimInfo(this, "dance4", AnimType.OVERRULE_IDLE).fadeFrom.Add(key42, 0.1f);
			new AnimInfo(this, "headSpinRStart", AnimType.NORMAL, "headSpinR");
			new AnimInfo(this, "headSpinRStop", AnimType.NORMAL, "idle");
			new AnimInfo(this, "headSpinLStart", AnimType.NORMAL, "headSpinL");
			new AnimInfo(this, "headSpinLStop", AnimType.NORMAL, "idle");
		}

		public void InitAnimInfosTricking()
		{
			moveStyle = MoveStyle.ON_FOOT;
			int key = Animator.StringToHash("land");
			int key2 = Animator.StringToHash("halfStartRun");
			int key3 = Animator.StringToHash("halfStopRun");
			int key4 = Animator.StringToHash("turnRun");
			int key5 = Animator.StringToHash("jump");
			int key6 = Animator.StringToHash("airTrick0");
			int key7 = Animator.StringToHash("airTrick1");
			int key8 = Animator.StringToHash("airTrick2");
			int key9 = Animator.StringToHash("airTrick3");
			int key10 = Animator.StringToHash("airBoostTrick0");
			int key11 = Animator.StringToHash("airBoostTrick1");
			int key12 = Animator.StringToHash("airBoostTrick2");
			int key13 = Animator.StringToHash("airDash");
			int key14 = Animator.StringToHash("wallRunLeft");
			int key15 = Animator.StringToHash("wallRunRight");
			int key16 = Animator.StringToHash("grindTrick0Hold");
			int key17 = Animator.StringToHash("grindTrick1Hold");
			int key18 = Animator.StringToHash("grindTrick2Hold");
			int key19 = Animator.StringToHash("grindTrick3Hold");
			int key20 = Animator.StringToHash("vaultSlide");
			int key21 = Animator.StringToHash("boostRun");
			int key22 = Animator.StringToHash("groundTrick0");
			int key23 = Animator.StringToHash("groundTrick1");
			int key24 = Animator.StringToHash("groundTrick2");
			int key25 = Animator.StringToHash("groundTrick3");
			int key26 = Animator.StringToHash("groundBoostTrick0");
			int key27 = Animator.StringToHash("groundBoostTrick1");
			int key28 = Animator.StringToHash("groundBoostTrick2");
			Animator.StringToHash("groundBoostTrick3");
			int key29 = Animator.StringToHash("grindTrick0");
			int key30 = Animator.StringToHash("grindTrick1");
			int key31 = Animator.StringToHash("grindTrick2");
			int key32 = Animator.StringToHash("grindTrick3");
			int key33 = Animator.StringToHash("slide");
			int key34 = Animator.StringToHash("roll");
			int key35 = Animator.StringToHash("slideToRun");
			int key36 = Animator.StringToHash("boostBrake");
			int key37 = Animator.StringToHash("startBoost");
			int key38 = Animator.StringToHash("switchToEquippedMovestyleGrind");
			int key39 = Animator.StringToHash("switchToEquippedMovestyleMoving");
			int key40 = Animator.StringToHash("switchFromEquippedMovestyleSkateboardMoving");
			int key41 = Animator.StringToHash("switchFromEquippedMovestyleBMXMoving");
			int key42 = Animator.StringToHash("switchFromEquippedMovestyleSkatesMoving");
			int key43 = Animator.StringToHash("turretCaught");
			int key44 = Animator.StringToHash("turretCaughtNoChains");
			int key45 = Animator.StringToHash("turretStruggleA");
			int key46 = Animator.StringToHash("turretStruggleNoChainsA");
			int key47 = Animator.StringToHash("turretStruggleB");
			int key48 = Animator.StringToHash("turretStruggleNoChainsB");
			new AnimInfo(this, "walk")
			{
				fadeFrom = 
				{
					{ runHash, 0.4f },
					{ idleHash, 0.25f },
					{ key, 0.35f },
					{ stopRunHash, 1f },
					{ startRunHash, 1f },
					{ key2, 1f },
					{ key3, 1f }
				}
			};
			new AnimInfo(this, "grindTrick0", AnimType.NORMAL, "grindTrick0Hold", 0.5f);
			new AnimInfo(this, "grindTrick1", AnimType.NORMAL, "grindTrick1Hold", 0.5f);
			new AnimInfo(this, "grindTrick2", AnimType.NORMAL, "grindTrick2Hold", 0.5f);
			new AnimInfo(this, "grindTrick3", AnimType.NORMAL, "grindTrick3Hold", 0.5f);
			new AnimInfo(this, "switchToEquippedMovestyleGrind", AnimType.NORMAL, "grindTrick0Hold", 0.5f);
			new AnimInfo(this, "groundTrick0", AnimType.NORMAL, "run", 0f, 0.75f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.15f },
					{ key22, 0.15f },
					{ key23, 0.15f },
					{ key24, 0.15f },
					{ key26, 0.15f },
					{ key27, 0.15f },
					{ key28, 0.15f }
				}
			};
			new AnimInfo(this, "groundTrick1", AnimType.NORMAL, "run", 0f, 0.75f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.15f },
					{ key22, 0.15f },
					{ key23, 0.15f },
					{ key24, 0.15f },
					{ key26, 0.15f },
					{ key27, 0.15f },
					{ key28, 0.15f }
				}
			};
			new AnimInfo(this, "groundTrick2", AnimType.NORMAL, "run", 0f, 0.75f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.15f },
					{ key22, 0.15f },
					{ key23, 0.15f },
					{ key24, 0.15f },
					{ key26, 0.15f },
					{ key27, 0.15f },
					{ key28, 0.15f }
				}
			};
			new AnimInfo(this, "groundTrick3", AnimType.NORMAL, "run", 0f, 0.75f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f }
				}
			};
			new AnimInfo(this, "groundBoostTrick0", AnimType.NORMAL, "run", 0.75f, 0f, 0.8f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.1f },
					{ key22, 0.1f },
					{ key23, 0.1f },
					{ key24, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f },
					{ key28, 0.1f }
				}
			};
			new AnimInfo(this, "groundBoostTrick1", AnimType.NORMAL, "run", 0.75f, 0f, 0.8f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.1f },
					{ key22, 0.1f },
					{ key23, 0.1f },
					{ key24, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f },
					{ key28, 0.1f }
				}
			};
			new AnimInfo(this, "groundBoostTrick2", AnimType.NORMAL, "run", 0.75f, 0f, 0.8f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f },
					{ key34, 0.1f },
					{ key22, 0.1f },
					{ key23, 0.1f },
					{ key24, 0.1f },
					{ key26, 0.1f },
					{ key27, 0.1f },
					{ key28, 0.1f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleMoving", AnimType.NORMAL, "run")
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f }
				}
			};
			new AnimInfo(this, "switchToEquippedMovestyleStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = { { startRunHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "switchFromEquippedMovestyleSkateboardMoving", AnimType.NORMAL, "run", 0f, 0.25f, 0.75f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f }
				}
			};
			new AnimInfo(this, "switchFromEquippedMovestyleSkateboardStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "switchFromEquippedMovestyleBMXMoving", AnimType.NORMAL, "run", 0f, 0.5f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f }
				}
			};
			new AnimInfo(this, "switchFromEquippedMovestyleBMXStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "switchFromEquippedMovestyleSkatesMoving", AnimType.NORMAL, "run", 0f, 0.4f)
			{
				skipStartrun = true,
				fadeTo = 
				{
					{ stopRunHash, 0.3f },
					{ runHash, 0.3f }
				}
			};
			new AnimInfo(this, "switchFromEquippedMovestyleSkatesStanding", AnimType.NORMAL, "idle")
			{
				fadeTo = 
				{
					{ startRunHash, 0.1f },
					{ idleHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "land", AnimType.LAND, "idle")
			{
				fadeFrom = 
				{
					{ walkHash, 0.25f },
					{ runHash, 0.2f }
				},
				feetIK = true
			};
			new AnimInfo(this, "run", AnimType.RUN)
			{
				fadeFrom = 
				{
					{ walkHash, 0.2f },
					{ key4, 0.3f },
					{ stopRunHash, 0.2f }
				}
			};
			new AnimInfo(this, "startRun", AnimType.RUN, "run", 0f, 0.25f, 0.5f)
			{
				fadeFrom = 
				{
					{ idleHash, 0.05f },
					{ walkHash, 0.05f }
				},
				fadeTo = { { runHash, 0.5f } }
			};
			new AnimInfo(this, "fall", AnimType.NORMAL, "fallIdle")
			{
				fadeFrom = 
				{
					{ runHash, 0.5f },
					{ walkHash, 0.5f },
					{ idleHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ startRunHash, 0.5f },
					{ key5, 0.2f },
					{ key6, 0.2f },
					{ key7, 0.2f },
					{ key8, 0.2f },
					{ key9, 0.2f },
					{ key10, 0.2f },
					{ key11, 0.2f },
					{ key12, 0.2f },
					{ key13, 0.5f },
					{ key14, 0.75f },
					{ key15, 0.75f },
					{ key16, 0.25f },
					{ key17, 0.25f },
					{ key18, 0.25f },
					{ key19, 0.25f },
					{ key, 0.5f },
					{ landRunHash, 0.5f },
					{ key20, 0.4f },
					{ key21, 1f },
					{ key22, 0.5f },
					{ key23, 0.5f },
					{ key24, 0.5f },
					{ key25, 0.5f },
					{ key26, 0.5f },
					{ key27, 0.5f },
					{ key28, 0.5f },
					{ key29, 0.5f },
					{ key30, 0.5f },
					{ key31, 0.5f },
					{ key32, 0.5f },
					{ key33, 0.35f },
					{ key34, 0.4f },
					{ key35, 0.4f },
					{ key36, 0.2f },
					{ key37, 0.4f },
					{ key38, 0.5f },
					{ key39, 0.5f },
					{ key40, 0.5f },
					{ key41, 0.5f },
					{ key42, 0.5f }
				}
			};
			int key49 = Animator.StringToHash("fallIdle");
			Animator.StringToHash("poleFreezeIdle");
			int key50 = Animator.StringToHash("idleFidget2");
			Animator.StringToHash("dance1");
			new AnimInfo(this, "fallIdle", AnimType.NORMAL, "", -1f);
			new AnimInfo(this, "poleFlip", AnimType.NORMAL, "fall", -1f);
			new AnimInfo(this, "hitBounce", AnimType.NORMAL, "fallIdle", -1f).fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "poleFreeze", AnimType.NORMAL, "poleFreezeIdle");
			new AnimInfo(this, "jumpTrick1", AnimType.NORMAL, "fallIdle", -1f);
			new AnimInfo(this, "airTrick0", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airTrick1", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airTrick2", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airTrick3", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airBoostTrick0", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airBoostTrick1", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "airBoostTrick2", AnimType.AIRTRICK, "fallIdle").fadeTo.Add(key49, 0.75f);
			new AnimInfo(this, "landRun", AnimType.LAND, "run", 0f, 0.5f, 0.5f)
			{
				fadeFrom = { { walkHash, 0.25f } },
				fadeTo = { { runHash, 0.5f } },
				skipStartrun = true
			};
			new AnimInfo(this, "turnRun");
			new AnimInfo(this, "stopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ key20, 0.25f },
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ runHash, 0.2f },
					{ startRunHash, 0.2f },
					{ key35, 0.5f }
				},
				feetIK = true,
				skipStartrun = true
			};
			new AnimInfo(this, "halfStopRun", AnimType.STOPRUN, "idle")
			{
				fadeFrom = 
				{
					{ idleHash, 0.25f },
					{ key2, 0.2f },
					{ startRunHash, 0.1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "boostRun").fadeFrom.Add(key5, 0.2f);
			new AnimInfo(this, "boostBrake", AnimType.NORMAL, "idle")
			{
				fadeTo = { { startRunHash, 0.1f } },
				feetIK = true
			};
			new AnimInfo(this, "getHitFar", AnimType.NORMAL, "idle").fadeTo.Add(fallHash, 0.5f);
			new AnimInfo(this, "getHit", AnimType.NORMAL, "idle");
			new AnimInfo(this, "getHitShieldBash", AnimType.NORMAL, "idle");
			new AnimInfo(this, "sit");
			new AnimInfo(this, "startSit", AnimType.NORMAL, "sit");
			new AnimInfo(this, "stopSit", AnimType.NORMAL, "idle");
			new AnimInfo(this, "idle", AnimType.IDLE)
			{
				fadeFrom = 
				{
					{ walkHash, 0.15f },
					{ key36, 0.3f },
					{ landRunHash, 0.2f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 1f }
				},
				feetIK = true
			};
			new AnimInfo(this, "getUp", AnimType.OVERRULE_IDLE, "idle");
			new AnimInfo(this, "idleFidget1", AnimType.NORMAL, "idle").feetIK = true;
			new AnimInfo(this, "idleFidget2", AnimType.NORMAL, "idle").feetIK = true;
			new AnimInfo(this, "airDash", AnimType.NORMAL, "fall");
			new AnimInfo(this, "roll", AnimType.NORMAL, "slide", 0.5f);
			new AnimInfo(this, "slide").fadeFrom.Add(key34, 0.3f);
			new AnimInfo(this, "slideToRun", AnimType.NORMAL, "run").skipStartrun = true;
			new AnimInfo(this, "vault", AnimType.NORMAL, "idle");
			new AnimInfo(this, "vaultSlide", AnimType.NORMAL, "idle")
			{
				fadeTo = { { runHash, 0.3f } },
				skipStartrun = true,
				feetIK = true
			};
			new AnimInfo(this, "halfStartRun", AnimType.NORMAL, "run")
			{
				fadeTo = { { stopRunHash, 0.2f } },
				fadeFrom = { { key3, 0.2f } },
				skipStartrun = true
			};
			new AnimInfo(this, "bounce", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			bounceHash = Animator.StringToHash("bounce");
			new AnimInfo(this, "softBounce1", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce1Hash = Animator.StringToHash("softBounce1");
			new AnimInfo(this, "softBounce2", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce2Hash = Animator.StringToHash("softBounce2");
			new AnimInfo(this, "softBounce3", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce3Hash = Animator.StringToHash("softBounce3");
			new AnimInfo(this, "softBounce4", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce4Hash = Animator.StringToHash("softBounce4");
			new AnimInfo(this, "softBounce5", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce5Hash = Animator.StringToHash("softBounce5");
			new AnimInfo(this, "softBounce6", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce6Hash = Animator.StringToHash("softBounce6");
			new AnimInfo(this, "softBounce7", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce7Hash = Animator.StringToHash("softBounce7");
			new AnimInfo(this, "softBounce8", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce8Hash = Animator.StringToHash("softBounce8");
			new AnimInfo(this, "softBounce9", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce9Hash = Animator.StringToHash("softBounce9");
			new AnimInfo(this, "softBounce10", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce10Hash = Animator.StringToHash("softBounce10");
			new AnimInfo(this, "softBounce11", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce11Hash = Animator.StringToHash("softBounce11");
			new AnimInfo(this, "softBounce12", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce12Hash = Animator.StringToHash("softBounce12");
			new AnimInfo(this, "softBounce13", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce13Hash = Animator.StringToHash("softBounce13");
			new AnimInfo(this, "softBounce14", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce14Hash = Animator.StringToHash("softBounce14");
			new AnimInfo(this, "softBounce15", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce15Hash = Animator.StringToHash("softBounce15");
			new AnimInfo(this, "softBounce16", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce16Hash = Animator.StringToHash("softBounce16");
			new AnimInfo(this, "softBounce17", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce17Hash = Animator.StringToHash("softBounce17");
			new AnimInfo(this, "softBounce18", AnimType.OVERRULE_IDLE)
			{
				fadeTo = { { idleHash, 0.15f } },
				fadeFrom = 
				{
					{ idleFidget1Hash, 0.15f },
					{ key50, 0.15f },
					{ idleHash, 0.15f },
					{ walkHash, 0.15f },
					{ startRunHash, 0.5f },
					{ stopRunHash, 0.5f },
					{ runHash, 0.15f }
				}
			};
			softBounce18Hash = Animator.StringToHash("softBounce18");
			new AnimInfo(this, "popping", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "locking", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "breaking", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "hiphop", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "housedance", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "toprock", AnimType.OVERRULE_IDLE)
			{
				fadeFrom = 
				{
					{ bounceHash, 0.15f },
					{ softBounce1Hash, 0.075f },
					{ softBounce2Hash, 0.03f },
					{ softBounce3Hash, 0.15f },
					{ softBounce4Hash, 0.075f },
					{ softBounce5Hash, 0.15f },
					{ softBounce6Hash, 0.15f },
					{ softBounce7Hash, 0.05f },
					{ softBounce8Hash, 0.15f },
					{ softBounce9Hash, 0.15f },
					{ softBounce10Hash, 0.15f },
					{ softBounce11Hash, 0.075f },
					{ softBounce12Hash, 0.15f },
					{ softBounce13Hash, 0.15f },
					{ softBounce14Hash, 0.15f },
					{ softBounce15Hash, 0.15f },
					{ softBounce16Hash, 0.15f },
					{ softBounce17Hash, 0.3f },
					{ softBounce18Hash, 0.075f }
				}
			};
			new AnimInfo(this, "headSpinRStart", AnimType.NORMAL, "headSpinR");
			new AnimInfo(this, "headSpinRStop", AnimType.NORMAL, "idle");
			new AnimInfo(this, "headSpinLStart", AnimType.NORMAL, "headSpinL");
			new AnimInfo(this, "headSpinLStop", AnimType.NORMAL, "idle");
			new AnimInfo(this, "turretCaught")
			{
				fadeFrom = 
				{
					{ key45, 0.05f },
					{ key46, 0.05f },
					{ key47, 0.05f },
					{ key48, 0.05f }
				}
			};
			new AnimInfo(this, "turretCaughtNoChains")
			{
				fadeFrom = 
				{
					{ key45, 0.05f },
					{ key46, 0.05f },
					{ key47, 0.05f },
					{ key48, 0.05f },
					{ key43, 0.05f }
				}
			};
			new AnimInfo(this, "turreStruggleA")
			{
				fadeFrom = 
				{
					{ key46, 0.05f },
					{ key47, 0.05f },
					{ key48, 0.05f },
					{ key43, 0.05f },
					{ key44, 0.05f }
				}
			};
			new AnimInfo(this, "turretStruggleNoChainsA")
			{
				fadeFrom = 
				{
					{ key45, 0.02f },
					{ key47, 0.02f },
					{ key48, 0.02f },
					{ key43, 0.02f },
					{ key44, 0.02f }
				}
			};
			new AnimInfo(this, "turreStruggleB")
			{
				fadeFrom = 
				{
					{ key45, 0.05f },
					{ key46, 0.05f },
					{ key48, 0.05f },
					{ key43, 0.05f },
					{ key44, 0.05f }
				}
			};
			new AnimInfo(this, "turretStruggleNoChainsB")
			{
				fadeFrom = 
				{
					{ key47, 0.02f },
					{ key45, 0.02f },
					{ key46, 0.02f },
					{ key43, 0.02f },
					{ key44, 0.02f }
				}
			};
			new AnimInfo(this, "lockPick");
		}

		public void Awake()
		{
			tf = base.transform;
			playerInput = GetComponent<UserInputHandler>();
			Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
			Core.OnLateUpdate += LateUpdatePlayer;
		}

		public void OnDestroy()
		{
			UnsubscribeUpdateEvents();
			if (!isAI)
			{
				if (phone != null)
				{
					phone.Terminate();
				}
				CleanUpUIDisplay();
			}
		}

		internal void DisablePlayer()
		{
			if (!isDisabled)
			{
				velBeforeDisable = GetVelocity();
				platformVelBeforeDisable = motor.platformVelocity;
				platformAngularVelBeforeDisable = motor.platformAngularVelocity;
				kinematicBeforeDisable = motor.IsKinematic();
				motor.SetVelocityTotal(Vector3.zero, Vector3.zero, Vector3.zero);
				motor.SetKinematic(set: true);
				characterVisual.ClearLookAtSubject();
				isDisabled = true;
			}
		}

		internal void EnablePlayer(bool applyPreDisableVelocity = false)
		{
			if (isDisabled)
			{
				if (applyPreDisableVelocity)
				{
					motor.SetVelocityTotal(velBeforeDisable, platformVelBeforeDisable, platformAngularVelBeforeDisable);
				}
				motor.SetKinematic(kinematicBeforeDisable);
				isDisabled = false;
				disableCount = 0;
			}
		}

		private void UnsubscribeUpdateEvents()
		{
			Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
			Core.OnLateUpdate -= LateUpdatePlayer;
			ComboMascotSystem.OnStartedCombo -= OnStartedComboMascot;
		}

		private void OnCoreUpdatePaused()
		{
			velBeforePause = GetVelocity();
			platformVelBeforePause = motor.platformVelocity;
			platformAngularVelBeforePause = motor.platformAngularVelocity;
			kinematicBeforePause = motor.IsKinematic();
			motor.SetVelocityTotal(Vector3.zero, Vector3.zero, Vector3.zero);
			motor.SetKinematic(set: true);
			PauseAnim(set: true);
		}

		private void OnCoreUpdateUnPaused()
		{
			motor.SetVelocityTotal(velBeforePause, platformVelBeforePause, platformAngularVelBeforePause);
			motor.SetKinematic(kinematicBeforePause);
			PauseAnim(set: false);
		}

		public void SetAI(PlayerType pt)
		{
			AI = base.gameObject.AddComponent<PlayerAI>();
			AI.Init();
		}

		public void Init(CharacterConstructor characterConstructor, Characters setCharacter = Characters.NONE, int setOutfit = 0, PlayerType setPlayerType = PlayerType.HUMAN, MoveStyle setMoveStyleEquipped = MoveStyle.ON_FOOT, Crew setCrew = Crew.PLAYERS)
		{
			this.characterConstructor = characterConstructor;
			playerType = setPlayerType;
			isAI = playerType != PlayerType.HUMAN;
			if (isAI)
			{
				SetAI(setPlayerType);
			}
			crew = setCrew;
			if (setCharacter != Characters.NONE)
			{
				SetCharacter(setCharacter, setOutfit);
			}
			ResetHP();
			InitAudio();
			InitVisual();
			InitAnimation();
			InitMovement(setMoveStyleEquipped);
			InitAbilities();
			if ((bool)cam)
			{
				cam.SetPlayerToFollow(this);
			}
			if (!isAI)
			{
				InitUIDisplay();
				phone = UnityEngine.Object.Instantiate(phonePrefab).GetComponent<Reptile.Phone.Phone>();
				phone.PhoneInit(this);
				SetUpGrafSpotData();
				SetUpStoryObjectData();
				if (Core.Instance.Platform.HasAchievementsSupport)
				{
					playerAchievements = base.gameObject.AddComponent<PlayerAchievements>();
				}
			}
		}

		private void InitAudio()
		{
			audioManager = Core.Instance.AudioManager;
			float spatialBlend = Convert.ToInt32(isAI);
			GameObject gameObject = base.transform.FindRecursive("Audio").gameObject;
			playerOneShotAudioSource = gameObject.AddComponent<AudioSource>();
			playerAccessoryAudioSource = gameObject.AddComponent<AudioSource>();
			playerMovementLoopAudioSource = gameObject.AddComponent<AudioSource>();
			playerUltraBoostLoopAudioSource = gameObject.AddComponent<AudioSource>();
			playerSlideAudioSource = gameObject.AddComponent<AudioSource>();
			playerGrindLoopAudioSource = gameObject.AddComponent<AudioSource>();
			playerGameplayVoicesAudioSource = gameObject.AddComponent<AudioSource>();
			playerOneShotAudioSource.spatialBlend = spatialBlend;
			playerAccessoryAudioSource.spatialBlend = spatialBlend;
			playerMovementLoopAudioSource.spatialBlend = spatialBlend;
			playerUltraBoostLoopAudioSource.spatialBlend = spatialBlend;
			playerSlideAudioSource.spatialBlend = spatialBlend;
			playerGrindLoopAudioSource.spatialBlend = spatialBlend;
			playerGameplayVoicesAudioSource.spatialBlend = spatialBlend;
			playerOneShotAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerOneShotAudioSource.minDistance = soundRangeMin;
			playerOneShotAudioSource.maxDistance = soundRangeMax;
			playerAccessoryAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerAccessoryAudioSource.minDistance = soundRangeMin;
			playerAccessoryAudioSource.maxDistance = soundRangeMax;
			playerMovementLoopAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerMovementLoopAudioSource.minDistance = soundRangeMin;
			playerMovementLoopAudioSource.maxDistance = soundRangeMax;
			playerUltraBoostLoopAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerUltraBoostLoopAudioSource.minDistance = soundRangeMin;
			playerUltraBoostLoopAudioSource.maxDistance = soundRangeMax;
			playerSlideAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerSlideAudioSource.minDistance = soundRangeMin;
			playerSlideAudioSource.maxDistance = soundRangeMax;
			playerGrindLoopAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerGrindLoopAudioSource.minDistance = soundRangeMin;
			playerGrindLoopAudioSource.maxDistance = soundRangeMax;
			playerGameplayVoicesAudioSource.rolloffMode = AudioRolloffMode.Linear;
			playerGameplayVoicesAudioSource.minDistance = soundRangeMin;
			playerGameplayVoicesAudioSource.maxDistance = soundRangeMax;
			if ((bool)AI)
			{
				playerOneShotAudioSource.volume = 0.6f;
				playerAccessoryAudioSource.volume = 0.6f;
				playerMovementLoopAudioSource.volume = 0.6f;
				playerUltraBoostLoopAudioSource.volume = 0.6f;
				playerSlideAudioSource.volume = 0.6f;
				playerGrindLoopAudioSource.volume = 0.6f;
				playerGameplayVoicesAudioSource.volume = 0.6f;
			}
			playerOneShotAudioSource.playOnAwake = false;
			playerAccessoryAudioSource.playOnAwake = false;
			playerMovementLoopAudioSource.playOnAwake = false;
			playerUltraBoostLoopAudioSource.playOnAwake = false;
			playerSlideAudioSource.playOnAwake = false;
			playerGrindLoopAudioSource.playOnAwake = false;
			playerGameplayVoicesAudioSource.playOnAwake = false;
		}

		private void InitHitboxes()
		{
			hitboxLeftLeg = new GameObject();
			hitboxLeftLeg.transform.parent = characterVisual.lowerLegL;
			hitboxLeftLeg.transform.localPosition = Vector3.zero;
			hitboxLeftLeg.transform.localRotation = Quaternion.identity;
			hitboxLeftLeg.layer = 18;
			CapsuleCollider capsuleCollider = hitboxLeftLeg.AddComponent<CapsuleCollider>();
			capsuleCollider.isTrigger = true;
			capsuleCollider.radius = 0.4f;
			capsuleCollider.direction = 0;
			capsuleCollider.height = 2f;
			capsuleCollider.center = new Vector3(-1f, 0f, 0f);
			hitboxLeftLeg.SetActive(value: false);
			hitboxRightLeg = new GameObject();
			hitboxRightLeg.transform.parent = characterVisual.lowerLegR;
			hitboxRightLeg.transform.localPosition = Vector3.zero;
			hitboxRightLeg.transform.localRotation = Quaternion.identity;
			hitboxRightLeg.layer = 18;
			CapsuleCollider capsuleCollider2 = hitboxRightLeg.AddComponent<CapsuleCollider>();
			capsuleCollider2.isTrigger = true;
			capsuleCollider2.radius = 0.4f;
			capsuleCollider2.direction = 0;
			capsuleCollider2.height = 2f;
			capsuleCollider2.center = new Vector3(-1f, 0f, 0f);
			hitboxRightLeg.SetActive(value: false);
			hitboxUpperBody = new GameObject();
			hitboxUpperBody.transform.parent = characterVisual.head;
			hitboxUpperBody.transform.localPosition = Vector3.zero;
			hitboxUpperBody.transform.localRotation = Quaternion.identity;
			hitboxUpperBody.layer = 18;
			CapsuleCollider capsuleCollider3 = hitboxUpperBody.AddComponent<CapsuleCollider>();
			capsuleCollider3.isTrigger = true;
			capsuleCollider3.radius = 0.4f;
			capsuleCollider3.direction = 0;
			capsuleCollider3.height = 2f;
			capsuleCollider3.center = new Vector3(-1f, 0f, 0f);
			hitboxUpperBody.SetActive(value: false);
			hitbox.SetActive(value: false);
			airialHitbox.SetActive(value: false);
			radialHitbox.SetActive(value: false);
		}

		public void FixedUpdatePlayer()
		{
			if (isDisabled)
			{
				return;
			}
			if ((bool)cam)
			{
				cam.fixedFrameCounter++;
			}
			if (userInputEnabled)
			{
				if (isAI)
				{
					inputBuffer = AI.UpdatePlayerAI();
				}
				else
				{
					playerInput.PollInputs(ref inputBuffer);
				}
				if (!IsBusyWithSequence())
				{
					SetInputs(inputBuffer);
				}
			}
			else
			{
				FlushInput();
			}
			UpdateRotation();
			FixedUpdateAbilities();
			UpdateJumpStatus();
			UpdateAnim();
			Move();
			ResetMoveVariables();
			preVelocity = GetVelocity();
			CheckAvailableGraffitiSpots();
		}

		public void UpdatePlayer()
		{
			if (isDisabled)
			{
				return;
			}
			float dt = Core.dt;
			timeSinceSpawn += dt;
			if (userInputEnabled)
			{
				if (!isAI)
				{
					playerInput.PollInputs(ref inputBuffer);
				}
				if (!IsBusyWithSequence())
				{
					SetInputs(inputBuffer);
				}
			}
			else
			{
				FlushInput();
			}
			UpdateBoostpack();
			UpdateCombat();
			UpdateScore();
			UpdateAbilities();
			UpdateMoveStyleMovementAudio();
			if (!isAI)
			{
				phone.PhoneUpdate();
				SaveNavMeshPosition();
				if (userInputEnabled)
				{
					AddSafeLocation(dt);
				}
			}
			UpdateVisualRot();
		}

		public void UpdateMoveStyleMovementAudio()
		{
			if (ability == null && (targetMovement == MovementType.RUNNING || targetMovement == MovementType.WALKING) && (moveStyle == MoveStyle.SKATEBOARD || moveStyle == MoveStyle.INLINE) && IsGrounded())
			{
				SfxCollectionID sfxCollectionID = ((moveStyle == MoveStyle.SKATEBOARD) ? SfxCollectionID.MoveStyle_2_SkateBoard : SfxCollectionID.MoveStyle_3_Inline);
				if (moveStyle != movestyleAudioCurrent)
				{
					movestyleAudioTargetClip = audioManager.GetAudioClipFromID(sfxCollectionID, AudioClipID.moveLoop);
					movestyleAudioCurrent = moveStyle;
				}
				if (playerMovementLoopAudioSource.clip != movestyleAudioTargetClip)
				{
					audioManager.PlaySfxGameplayLooping(sfxCollectionID, AudioClipID.moveLoop, playerMovementLoopAudioSource);
				}
			}
			else if (!AudioManager.ClipIsFinished(playerMovementLoopAudioSource) && (playerMovementLoopAudioSource.clip == audioManager.GetAudioClipFromID(SfxCollectionID.MoveStyle_2_SkateBoard, AudioClipID.moveLoop) || playerMovementLoopAudioSource.clip == audioManager.GetAudioClipFromID(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.moveLoop)))
			{
				audioManager.StopLoopingSfx(playerMovementLoopAudioSource);
			}
		}

		public void LateUpdatePlayer()
		{
			if ((bool)cam)
			{
				cam.UpdateCamera();
			}
			LateUpdateAnimation();
			LateUpdateUIDisplay();
		}

		public void SwitchCharacterForTests(int add)
		{
			charTestCycleNum += add;
			if (charTestCycleNum >= 26)
			{
				charTestCycleNum = 0;
			}
			if (charTestCycleNum < 0)
			{
				charTestCycleNum = 25;
			}
			SetCharacter((Characters)charTestCycleNum);
			InitHitboxes();
			InitCuffs();
		}

		public void SwitchOutfitForTests(int add)
		{
			try
			{
				outfitTestCycleNum += add;
				if (outfitTestCycleNum > 3)
				{
					outfitTestCycleNum = 0;
				}
				if (outfitTestCycleNum < 0)
				{
					outfitTestCycleNum = 3;
				}
				SetOutfit(outfitTestCycleNum);
			}
			catch (NoOutfitException)
			{
				outfitTestCycleNum = 0;
			}
		}

		public void ChangeMoveStyleForTests(int add)
		{
			moveStyleTestCycleNum += add;
			if (moveStyleTestCycleNum >= 4)
			{
				moveStyleTestCycleNum = 0;
			}
			if (moveStyleTestCycleNum < 0)
			{
				moveStyleTestCycleNum = 3;
			}
			SetCurrentMoveStyleEquipped((MoveStyle)moveStyleTestCycleNum);
		}

		public void SetCurrentMoveStyleEquipped(MoveStyle setMoveStyleEquipped, bool changeProp = true, bool changeAnim = true)
		{
			if (setMoveStyleEquipped == MoveStyle.ON_FOOT)
			{
				setMoveStyleEquipped = MoveStyle.SKATEBOARD;
			}
			moveStyleEquipped = setMoveStyleEquipped;
			usingEquippedMovestyle = false;
			SetMoveStyle(MoveStyle.ON_FOOT, changeProp, changeAnim);
		}

		public void SwitchToEquippedMovestyle(bool set, bool doAirTrick = false, bool changeProp = true, bool showEffect = true)
		{
			usingEquippedMovestyle = set;
			MoveStyle moveStyle;
			if (usingEquippedMovestyle)
			{
				SetMoveStyle(moveStyleEquipped, changeProp);
				moveStyle = this.moveStyle;
			}
			else
			{
				moveStyle = this.moveStyle;
				SetMoveStyle(MoveStyle.ON_FOOT, changeProp);
			}
			if (doAirTrick)
			{
				switch (moveStyle)
				{
				case MoveStyle.BMX:
					AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.TailWhip, playerOneShotAudioSource);
					break;
				case MoveStyle.INLINE:
					AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.land, playerOneShotAudioSource);
					break;
				}
				if (moveStyle == MoveStyle.SKATEBOARD)
				{
					AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_2_SkateBoard, AudioClipID.groundTrick2, playerOneShotAudioSource);
				}
				ActivateAbility(airTrickAbility);
			}
			if (showEffect)
			{
				characterVisual.DoMoveStyleEquipEffect(moveStyleEquipped);
			}
			StopHoldSpraycan();
		}

		protected void SetMoveStyle(MoveStyle setMoveStyle, bool changeProp = true, bool changeAnim = true, GameObject specialSkateboard = null)
		{
			moveStyle = setMoveStyle;
			switch (setMoveStyle)
			{
			case MoveStyle.ON_FOOT:
				stats = trickingMovementStats;
				break;
			case MoveStyle.BMX:
				stats = bmxMovementStats;
				if (isAI)
				{
					stats.rotSpeedAtMaxSpeed = 3f;
					stats.rotSpeedInAir = 1.2f;
				}
				break;
			case MoveStyle.SKATEBOARD:
				stats = skateboardMovementStats;
				if (isAI)
				{
					stats.rotSpeedAtMaxSpeed = 5.5f;
					stats.rotSpeedInAir = 1.35f;
				}
				break;
			case MoveStyle.INLINE:
				stats = inlineMovementStats;
				if (isAI)
				{
					stats.rotSpeedAtMaxSpeed = 6f;
					stats.rotSpeedInAir = 1.35f;
				}
				break;
			case MoveStyle.SPECIAL_SKATEBOARD:
				stats = specialSkateboardMovementStats;
				break;
			}
			if (changeAnim)
			{
				characterVisual.SetMoveStyleVisualAnim(this, setMoveStyle, specialSkateboard);
				if (curAnim != 0)
				{
					int newAnim = curAnim;
					curAnim = 0;
					PlayAnim(newAnim);
				}
			}
			if (changeProp)
			{
				characterVisual.SetMoveStyleVisualProps(this, setMoveStyle);
			}
		}

		public void Revive()
		{
			if (OnRevived != null)
			{
				OnRevived();
			}
			WantedManager.instance.StopPlayerWantedStatus(showMessage: false);
			ResetHP();
			RemoveAllCuffs();
			ClearMultipliersDone();
		}

		public SafeLocation GetSafeLocation()
		{
			if (saveLocation.set)
			{
				return saveLocation;
			}
			return new SafeLocation
			{
				timeStamp = -1f
			};
		}

		private void AddSafeLocation(float deltaTime)
		{
			if (IsGrounded() && !motor.isOnPlatform && ability != grindAbility)
			{
				timeGrounded += deltaTime;
				if (timeGrounded >= 1f)
				{
					overwriteFirstSafeLocationTime += deltaTime;
					if (!saveLocation.set)
					{
						saveLocation = GenerateSafeLocation();
					}
					if (overwriteFirstSafeLocationTime > 0.5f)
					{
						saveLocation = GenerateSafeLocation();
						overwriteFirstSafeLocationTime = 0f;
					}
				}
			}
			else
			{
				timeGrounded = 0f;
			}
		}

		private SafeLocation GenerateSafeLocation()
		{
			SafeLocation result = default;
			result.position = tf.position;
			result.rotation = tf.rotation;
			result.timeStamp = Time.time;
			result.set = true;
			return result;
		}

		private void SaveNavMeshPosition()
		{
			if (IsGrounded() && !motor.isOnPlatform && ability != grindAbility && NavMesh.SamplePosition(tf.position, out var hit, 1f, -1))
			{
				lastNavMeshPosition = hit.position;
			}
		}

		public void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			if (debugDirYellow != Vector3.zero)
			{
				Gizmos.DrawLine((debugOriYellow != Vector3.zero) ? debugOriYellow : tf.position, ((debugOriYellow != Vector3.zero) ? debugOriYellow : tf.position) + debugDirYellow);
			}
			Gizmos.color = Color.red;
			if (debugDirRed != Vector3.zero)
			{
				Gizmos.DrawLine((debugOriRed != Vector3.zero) ? debugOriRed : tf.position, ((debugOriRed != Vector3.zero) ? debugOriRed : tf.position) + debugDirRed);
			}
			Gizmos.color = Color.blue;
			if (debugDirBlue != Vector3.zero)
			{
				Gizmos.DrawLine((debugOriBlue != Vector3.zero) ? debugOriBlue : tf.position, ((debugOriBlue != Vector3.zero) ? debugOriBlue : tf.position) + debugDirBlue);
			}
		}

		public void InitAbilities()
		{
			abilities = new List<Ability>();
			vertAbility = new VertAbility(this);
			airDashAbility = new AirDashAbility(this);
			slideAbility = new SlideAbility(this);
			airTrickAbility = new AirTrickAbility(this);
			groundTrickAbility = new GroundTrickAbility(this);
			switchMoveStyleAbility = new SwitchMoveStyleAbility(this);
			talkAbility = new TalkAbility(this);
			wallrunAbility = new WallrunLineAbility(this);
			grindAbility = new GrindAbility(this);
			handplantAbility = new HandplantAbility(this);
			specialAirAbility = new SpecialAirAbility(this);
			sitAbility = new SitAbility(this);
			headspinAbility = new HeadspinAbility(this);
			dieAbility = new DieAbility(this);
			hitBounceAbility = new HitBounceAbility(this);
			ledgeClimbAbility = new LedgeClimbAbility(this);
			boostAbility = new BoostAbility(this);
			flipOutJumpAbility = new FlipOutJumpAbility(this);
			danceAbility = new DanceAbility(this);
			characterSelectAbility = new CharacterSelectAbility(this);
			knockbackAbility = new KnockbackAbility(this);
			caughtAbility = new CaughtAbility(this);
			recoverAbility = new RecoverAbility(this);
			standbyAbility = new StandbyAbility(this);
			if (isAI)
			{
				pseudoGraffitiAbility = new PseudoGraffitiAbility(this);
				return;
			}
			airDashAbility.locked = (slideAbility.locked = (specialAirAbility.locked = (ledgeClimbAbility.locked = Core.Instance.SaveManager.CurrentSaveSlot.boostpackLocked)));
			boostAbility.locked = Core.Instance.SaveManager.CurrentSaveSlot.boostAbilityLocked;
			phoneLocked = Core.Instance.SaveManager.CurrentSaveSlot.phoneLocked;
			spraycanLocked = Core.Instance.SaveManager.CurrentSaveSlot.spraycanLocked;
			fortuneAppLocked = Core.Instance.SaveManager.CurrentSaveSlot.fortuneAppLocked;
			cameraAppLocked = Core.Instance.SaveManager.CurrentSaveSlot.cameraAppLocked;
			switchToEquippedMovestyleLocked = Core.Instance.SaveManager.CurrentSaveSlot.switchToEquippedMovestyleLocked;
		}

		public void PreludeAbilitySetup()
		{
			LockBoostpack(set: true);
			LockBoostAbility(set: true);
			LockCharacterSelect(set: true);
			LockPhone(set: true);
			LockSpraycan(set: true);
			LockFortuneApp(set: true);
			LockCameraApp(set: true);
			LockSwitchToEquippedMoveStyle(set: true);
			characterVisual.SetMoveStyleVisualProps(this, moveStyle, forceOff: true);
			Core.Instance.SaveManager.CurrentSaveSlot.taxiLocked = true;
			Core.Instance.SaveManager.CurrentSaveSlot.SetupStoryCharactersLocked();
		}

		public void DemoAbilitySetup()
		{
			LockCharacterSelect(set: true);
			LockFortuneApp(set: true);
		}

		public void LockBoostpack(bool set)
		{
			airDashAbility.locked = set;
			slideAbility.locked = set;
			specialAirAbility.locked = set;
			ledgeClimbAbility.locked = set;
			boostAbility.locked = set;
			Core.Instance.SaveManager.CurrentSaveSlot.boostAbilityLocked = set;
			Core.Instance.SaveManager.CurrentSaveSlot.boostpackLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
			if (!set)
			{
				characterVisual.SetMoveStyleVisualProps(this, moveStyle);
			}
		}

		public void LockSwitchToEquippedMoveStyle(bool set)
		{
			switchToEquippedMovestyleLocked = set;
			if (switchToEquippedMovestyleLocked)
			{
				SwitchToEquippedMovestyle(set: false);
			}
			Core.Instance.SaveManager.CurrentSaveSlot.switchToEquippedMovestyleLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockBoostAbility(bool set)
		{
			boostAbility.locked = set;
			Core.Instance.SaveManager.CurrentSaveSlot.boostAbilityLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockPhone(bool set)
		{
			phoneLocked = set;
			Core.Instance.SaveManager.CurrentSaveSlot.phoneLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockSpraycan(bool set)
		{
			spraycanLocked = set;
			Core.Instance.SaveManager.CurrentSaveSlot.spraycanLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockCharacterSelect(bool set)
		{
			Core.Instance.SaveManager.CurrentSaveSlot.characterSelectLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockCameraApp(bool set)
		{
			cameraAppLocked = set;
			phone.GetAppInstance<AppHomeScreen>().LockApp(set, HomeScreenApp.HomeScreenAppType.CAMERA);
			phone.GetAppInstance<AppHomeScreen>().LockApp(set, HomeScreenApp.HomeScreenAppType.PHOTO_ALBUM);
			Core.Instance.SaveManager.CurrentSaveSlot.cameraAppLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockFortuneApp(bool set)
		{
			fortuneAppLocked = set;
			phone.GetAppInstance<AppHomeScreen>().LockApp(set, HomeScreenApp.HomeScreenAppType.FORTUNE);
			Core.Instance.SaveManager.CurrentSaveSlot.fortuneAppLocked = set;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void ActivateAbility(Ability a)
		{
			BoostpackEffectMode boostpackEffectMode = characterVisual.boostpackEffectMode;
			StopCurrentAbility();
			ability = a;
			abilityTimer = 0f;
			didAbilityTrick = false;
			if (!a.keepJumping)
			{
				isJumping = false;
				SetDustEmission(0);
			}
			else
			{
				SetBoostpackAndFrictionEffects(boostpackEffectMode);
			}
			jumpLow = false;
			jumpRequested = false;
			timeSinceJumpRequested = float.PositiveInfinity;
			timeSinceLastAbleToJump = float.PositiveInfinity;
			motor.rotateWithPlatform = ability.rotateWithPlatform;
			ability.OnStartAbility();
		}

		public void CompletelyStop()
		{
			DropCombo();
			StopCurrentAbility();
			SetVelocity(Vector3.zero);
		}

		public void StopCurrentAbility()
		{
			preAbility = ability;
			motor.rotateWithPlatform = true;
			if (ability != null)
			{
				ability.OnStopAbility();
			}
			ability = null;
			SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF);
			hitbox.SetActive(value: false);
			radialHitbox.SetActive(value: false);
			hitboxLeftLeg.SetActive(value: false);
			hitboxRightLeg.SetActive(value: false);
			hitboxUpperBody.SetActive(value: false);
		}

		public void UpdateAbilities()
		{
			if (ability != null)
			{
				ability.UpdateAbility();
			}
		}

		public void FixedUpdateAbilities()
		{
			if (hitpause > 0f)
			{
				hitpause -= Core.dt;
				if (hitpause <= 0f)
				{
					StopHitpause();
				}
				return;
			}
			bool flag = IsGrounded();
			abilityTimer += Core.dt;
			if (flag)
			{
				RegainAirMobility();
			}
			grindAbility.PassiveUpdate();
			if (isAI)
			{
				pseudoGraffitiAbility.PassiveUpdate();
			}
			wallrunAbility.PassiveUpdate();
			handplantAbility.PassiveUpdate();
			if (ability == null)
			{
				if (!IsBusyWithSequence())
				{
					for (int i = 0; i < abilities.Count; i++)
					{
						if (abilities[i].CheckActivation())
						{
							return;
						}
					}
				}
				if (flag && inWalkZone)
				{
					if (usingEquippedMovestyle)
					{
						ActivateAbility(switchMoveStyleAbility);
					}
				}
				else if (switchStyleButtonNew && !switchToEquippedMovestyleLocked && !flag)
				{
					SwitchToEquippedMovestyle(!usingEquippedMovestyle, doAirTrick: true);
				}
			}
			else
			{
				ability.FixedUpdateAbility();
			}
			if ((ability == null) & flag)
			{
				if (!IsBusyWithSequence() && !motor.wasGrounded && slideButtonHeld && !slideAbility.locked)
				{
					ActivateAbility(slideAbility);
				}
				else
				{
					LandCombo();
				}
			}
		}

		public void RegainAirMobility()
		{
			boostAbility.haveAirStartBoost = true;
			airDashAbility.haveAirDash = true;
		}

		public bool CanStartGrind()
		{
			if (ability == null || ability.canStartGrind)
			{
				return !IsBusyWithSequence();
			}
			return false;
		}

		public bool CanStartWallrun()
		{
			if (ability == null || ability.canStartWallrun)
			{
				return !IsBusyWithSequence();
			}
			return false;
		}

		public void StartHitpause(float duration)
		{
			if (base.isActiveAndEnabled)
			{
				StartCoroutine(HitPauseHandling(duration));
			}
		}

		private IEnumerator HitPauseHandling(float duration)
		{
			yield return new WaitForEndOfFrame();
			if (!isAI)
			{
				cam.StartHitpause(duration);
			}
			if (!IsInHitpause())
			{
				preHitpauseGrounded = IsGrounded();
				preHitpauseVel = GetVelocity();
				SetVelocity(Vector3.zero);
			}
			hitpause = duration;
			PauseAnim(set: true);
		}

		public void StopHitpause()
		{
			if (!preHitpauseGrounded)
			{
				ForceUnground();
			}
			SetVelocity(preHitpauseVel);
			PauseAnim(set: false);
		}

		public bool IsInHitpause()
		{
			return hitpause > 0f;
		}

		public bool IsGrinding()
		{
			if (ability == grindAbility)
			{
				return grindAbility.nextNode;
			}
			return false;
		}

		public bool CheckBoostTrick()
		{
			if (!isAI || !(UnityEngine.Random.value > 0.85f))
			{
				if (boostButtonHeld && HasBoostCharge(boostTrickCost))
				{
					return !boostAbility.locked;
				}
				return false;
			}
			return true;
		}

		public void RecheckLoopingSounds()
		{
			if (ability != null)
			{
				ability.RecheckLoopingSoundInAbilityUse();
			}
			else if ((targetMovement == MovementType.RUNNING || targetMovement == MovementType.WALKING) && (moveStyle == MoveStyle.SKATEBOARD || moveStyle == MoveStyle.INLINE) && IsGrounded())
			{
				SfxCollectionID collectionId = ((moveStyle == MoveStyle.SKATEBOARD) ? SfxCollectionID.MoveStyle_2_SkateBoard : SfxCollectionID.MoveStyle_3_Inline);
				audioManager.PlaySfxGameplayLooping(collectionId, AudioClipID.moveLoop, playerMovementLoopAudioSource);
			}
			if (spraycanState == SpraycanState.SHAKE)
			{
				audioManager.PlaySfxGameplayLooping(SfxCollectionID.GraffitiSfx, AudioClipID.CanShakeLoop, playerAccessoryAudioSource);
			}
		}

		public void PlayVoice(AudioClipID audioClipID, VoicePriority voicePriority = VoicePriority.MOVEMENT, bool fromPlayer = true)
		{
			if (fromPlayer)
			{
				audioManager.PlayVoice(ref currentVoicePriority, character, audioClipID, playerGameplayVoicesAudioSource, voicePriority);
			}
			else
			{
				audioManager.PlaySfxGameplay(audioManager.characterToVoiceCollection[(int)character], audioClipID);
			}
		}

		public bool IsDead()
		{
			return ability == dieAbility;
		}

		public void InitAnimation()
		{
			for (int i = 0; i < 5; i++)
			{
				animInfosSets[i] = new Dictionary<int, AnimInfo>();
			}
			canSprayHash = Animator.StringToHash("canSpray");
			canShakeHash = Animator.StringToHash("canShake");
			canStartShakeHash = Animator.StringToHash("canStartShake");
			landRunHash = Animator.StringToHash("landRun");
			startRunHash = Animator.StringToHash("startRun");
			stopRunHash = Animator.StringToHash("stopRun");
			walkHash = Animator.StringToHash("walk");
			runHash = Animator.StringToHash("run");
			fallHash = Animator.StringToHash("fall");
			idleHash = Animator.StringToHash("idle");
			idleFidget1Hash = Animator.StringToHash("idleFidget1");
			phoneDirectionXHash = Animator.StringToHash("phoneDirectionX");
			phoneDirectionYHash = Animator.StringToHash("phoneDirectionY");
			turnDirectionXHash = Animator.StringToHash("turnDirectionX");
			turnDirectionX2Hash = Animator.StringToHash("turnDirectionX2");
			turnDirectionX3Hash = Animator.StringToHash("turnDirectionX3");
			turnDirectionSkateboardHash = Animator.StringToHash("turnDirectionSkateboard");
			MoveStyle moveStyle = this.moveStyle;
			InitAnimInfosTricking();
			InitAnimInfosSkateboard();
			InitAnimInfosBMX();
			InitAnimInfosSkates();
			this.moveStyle = moveStyle;
			animInfosSets[4] = animInfosSets[2];
			smoothRotation = !isAI;
			motor.smoothRotation = smoothRotation;
		}

		public void UpdateAnim()
		{
			UpdateHoldProps();
			Dictionary<int, AnimInfo> dictionary = animInfos;
			AnimInfo animInfo = (dictionary.ContainsKey(curAnim) ? dictionary[curAnim] : null);
			AnimType animType = AnimType.NORMAL;
			float num = 0f;
			if (animInfo != null)
			{
				animType = animInfo.animType;
				num = animInfo.interruptIntoFallFrom;
			}
			curAnimActiveTime += Core.dt;
			if (ability == null || ability.normalAnimation)
			{
				if (IsGrounded() || vertShape != null)
				{
					bool flag = curAnim == landRunHash;
					bool flag2 = animType == AnimType.LAND && GetAnimTime() <= 0.3f;
					bool flag3 = animInfo?.skipStartrun ?? false;
					if (curAnim == startRunHash)
					{
						startRunAnimTime = GetAnimTime();
					}
					else if (animType == AnimType.RUN || animType == AnimType.IDLE)
					{
						startRunAnimTime = 0f;
					}
					if (targetMovement == MovementType.NONE)
					{
						if (((animType != AnimType.LAND) | flag) && animType != AnimType.STOPRUN && animType != AnimType.OVERRULE_IDLE && animType != AnimType.GROUNDTRICK)
						{
							if (GetForwardSpeed() >= 5f)
							{
								PlayAnim(stopRunHash);
							}
							else if (curAnimActiveTime > idleVariationAfter)
							{
								PlayAnim(idleFidget1Hash);
							}
							else if (curAnim != idleFidget1Hash)
							{
								PlayAnim(idleHash);
							}
						}
					}
					else if (targetMovement == MovementType.WALKING)
					{
						if (!flag2 && animType != AnimType.STOPRUN && animType != AnimType.GROUNDTRICK)
						{
							if (GetForwardSpeed() >= 7f)
							{
								PlayAnim(stopRunHash);
							}
							else
							{
								PlayAnim(walkHash);
							}
						}
						else if (animType == AnimType.STOPRUN && animType != AnimType.GROUNDTRICK && GetAnimTime() >= 0.2f)
						{
							PlayAnim(walkHash);
						}
					}
					else if (targetMovement == MovementType.RUNNING && animType != AnimType.RUN && animType != AnimType.GROUNDTRICK && !flag)
					{
						if (flag3)
						{
							PlayAnim(runHash);
						}
						else if (GetForwardSpeed() < 2.8f)
						{
							startRunAnimTime = 0f;
							PlayAnim(startRunHash);
						}
						else
						{
							PlayAnim(startRunHash, forceOverwrite: false, instant: false, startRunAnimTime);
						}
					}
				}
				else if ((!isJumping || GetVelocity().y <= 0f) && num >= 0f && GetAnimTime() > num)
				{
					PlayAnim(fallHash);
				}
			}
			float num2 = 1f;
			if (dictionary.ContainsKey(curAnim))
			{
				num2 = dictionary[curAnim].duration;
			}
			if (GetAnimTime() > num2 && !anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).shortNameHash == curAnim && dictionary.ContainsKey(curAnim) && dictionary[curAnim].nextAnim != 0)
			{
				PlayAnim(dictionary[curAnim].nextAnim, forceOverwrite: false, instant: false, dictionary[curAnim].nextAnimAtTime);
			}
			firstFrameAnim = false;
			OrientVisualToSurface();
			UpdateLookAt();
		}

		public void UpdateLookAt()
		{
			if (!isAI)
			{
				characterVisual.lookAtVel = GetPracticalWorldVelocity();
				characterVisual.phoneActive = phone.IsOn && phoneLayerWeight >= 1f;
				characterVisual.lookAtSubject = null;
			}
		}

		public void LateUpdateAnimation()
		{
			if (!base.gameObject.activeSelf)
			{
				return;
			}
			float dt = Core.dt;
			if ((!isAI && phone.IsOn) || spraycanState != SpraycanState.NONE)
			{
				anim.speed = 0f;
				anim.SetFloat(phoneDirectionXHash, (0f - phoneDirBone.localRotation.x) * 2.5f + customPhoneHandValue.x);
				anim.SetFloat(phoneDirectionYHash, (0f - phoneDirBone.localRotation.y) * 2.5f + customPhoneHandValue.y);
				anim.Update(dt);
				anim.speed = 1f;
			}
			if (ability != handplantAbility)
			{
				Vector3 rhs = Vector3.Cross(Vector3.up, preDir);
				float num = Mathf.Clamp(Vector3.Dot(dir, rhs) * rotLeanFactor, -1f, 1f);
				rotLean = Utility.Lerp(rotLean, num, 12f * dt, 0.2f);
				rotLeanSecondary = Utility.Lerp(rotLeanSecondary, num, 3f * dt, 0.05f);
				rotLeanCurve = num * num;
				rotLeanSkateboard = num;
				if (num < 0f)
				{
					rotLeanCurve *= -1f;
				}
			}
			SetAnimationLean();
			if (characterVisual.canBlink)
			{
				UpdateBlinkAnimation(characterVisual.mainRenderer, characterMesh, ref blinkTimer, ref blink, ref blinkDuration, dt);
			}
		}

		internal static void UpdateBlinkAnimation(SkinnedMeshRenderer mainRenderer, Mesh characterMesh, ref float blinkTimer, ref bool blink, ref float blinkDuration, float deltaTime)
		{
			blinkTimer -= deltaTime;
			if (blinkTimer <= 0f && characterMesh.blendShapeCount > 0)
			{
				blink = true;
				mainRenderer.SetBlendShapeWeight(0, 100f);
				blinkTimer = UnityEngine.Random.Range(2, 4);
			}
			if (blink)
			{
				blinkDuration -= deltaTime;
			}
			if (blinkDuration <= 0f && characterMesh.blendShapeCount > 0)
			{
				blink = false;
				mainRenderer.SetBlendShapeWeight(0, 0f);
				blinkDuration = 0.1f;
			}
		}

		private void SetAnimationLean()
		{
			if (curAnim != runHash)
			{
				rotLeanCurve = 0f;
				rotLeanSkateboard = 0f;
			}
			rotLeanCurveSmooth = Utility.Lerp(rotLeanCurveSmooth, rotLeanCurve, 6f * Core.dt, 0.1f);
			rotLeanSkateboardSmooth = Utility.Lerp(rotLeanSkateboardSmooth, rotLeanSkateboard, 12f * Core.dt, 0.2f);
			if (rotLeanPrevious != rotLean)
			{
				rotLeanPrevious = rotLean;
				anim.SetFloat(turnDirectionXHash, rotLean);
			}
			if (rotLeanSecondaryPrevious != rotLeanSecondary)
			{
				rotLeanSecondaryPrevious = rotLeanSecondary;
				anim.SetFloat(turnDirectionX2Hash, rotLeanSecondary);
			}
			if (rotLeanCurveSmoothPrevious != rotLeanCurveSmooth)
			{
				rotLeanCurveSmoothPrevious = rotLeanCurveSmooth;
				anim.SetFloat(turnDirectionX3Hash, rotLeanCurveSmooth);
			}
			if (rotLeanSkateboardSmoothPrevious != rotLeanSkateboardSmooth)
			{
				rotLeanSkateboardSmoothPrevious = rotLeanSkateboardSmooth;
				anim.SetFloat(turnDirectionSkateboardHash, rotLeanSkateboardSmooth);
			}
		}

		public void ResetLean()
		{
			rotLean = 0f;
			rotLeanSecondary = 0f;
			rotLeanCurveSmooth = 0f;
			rotLeanSkateboardSmooth = 0f;
		}

		public void OrientVisualToSurface()
		{
			if (ability != null)
			{
				return;
			}
			float num = 18f;
			bool flag = IsGrounded();
			bool flag2 = moveStyle == MoveStyle.ON_FOOT || targetMovement == MovementType.NONE;
			bool flag3 = vertShape != null;
			Vector3 b = ((!flag3 && Vector3.Angle(motor.groundNormal, Vector3.up) > 15f) ? motor.groundPoint : tf.position);
			if (!flag2 && !flag)
			{
				num = 8f;
			}
			bool flag4 = flag2 && (targetMovement == MovementType.RUNNING || boosting);
			Vector3 vector = ((!flag3) ? (flag4 ? ((motor.groundNormalVisual + Vector3.up) * 0.5f) : motor.groundNormalVisual) : vertNormal);
			if (smoothRotation)
			{
				if (flag3)
				{
					visualUp = Quaternion.FromToRotation(Vector3.up, vector);
				}
				else
				{
					visualUp = Quaternion.Lerp(visualUp, (flag && (flag4 || !flag2)) ? Quaternion.FromToRotation(Vector3.up, vector) : Quaternion.identity, 1f - Mathf.Exp((0f - num) * Core.dt));
				}
				SetVisualRotXZ(visualUp);
			}
			else
			{
				if (flag && (flag4 || !flag2))
				{
					visualTf.up = Vector3.Lerp(visualTf.up, vector, 1f - Mathf.Exp((0f - num) * Core.dt));
				}
				else
				{
					visualTf.up = Vector3.Lerp(visualTf.up, Vector3.up, 1f - Mathf.Exp((0f - num) * Core.dt));
				}
				visualTf.Rotate(0f, motor.transform.localEulerAngles.y, 0f, Space.Self);
			}
			visualTf.position = Vector3.Lerp(visualTf.position, b, 1f - Mathf.Exp((0f - num) * Core.dt));
		}

		public void ResetVisualRot()
		{
			if (!smoothRotation)
			{
				visualTf.localRotation = Quaternion.identity;
				return;
			}
			visualUp = Quaternion.identity;
			SetVisualRot(Quaternion.Euler(0f, motor.transform.localEulerAngles.y, 0f));
		}

		public void SetVisualRotXZ(Quaternion rot)
		{
			SetVisualRot(rot * Quaternion.Euler(0f, motor.transform.localEulerAngles.y, 0f));
		}

		public void SetVisualRotLocalZ(float z)
		{
			if (!smoothRotation)
			{
				visualTf.localRotation = Quaternion.Euler(visualTf.localRotation.eulerAngles.x, visualTf.localRotation.eulerAngles.y, z);
			}
			else
			{
				visualRotLocalOverride = Quaternion.Euler(0f, 0f, z);
			}
		}

		public void SetVisualRotLocal0()
		{
			if (smoothRotation)
			{
				visualRotLocalOverride = Quaternion.identity;
			}
		}

		public void SetVisualRot(Quaternion rot, bool hard = false)
		{
			if (!smoothRotation)
			{
				visualTf.rotation = rot;
				return;
			}
			visualRotOrig = (hard ? rot : visualRotTarget);
			visualRotTarget = rot;
			visualRotOrigTime = Time.time;
			visualRotTargetTime = visualRotOrigTime + Time.fixedDeltaTime;
			visualRotLocalOverride = null;
		}

		public void UpdateVisualRot()
		{
			if (smoothRotation)
			{
				if (visualRotLocalOverride.HasValue)
				{
					visualTf.localRotation = visualRotLocalOverride.Value;
					return;
				}
				float t = Mathf.InverseLerp(visualRotOrigTime, visualRotTargetTime, Time.time);
				visualTf.rotation = Quaternion.Lerp(visualRotOrig, visualRotTarget, t);
			}
		}

		public void OrientVisualInstantReset()
		{
			ResetVisualRot();
			visualTf.position = tf.position;
		}

		public void OrientVisualInstant()
		{
			if (smoothRotation)
			{
				visualUp = Quaternion.FromToRotation(Vector3.up, motor.groundNormalVisual);
				SetVisualRotXZ(visualUp);
			}
			else
			{
				visualTf.up = motor.groundNormalVisual;
				visualTf.Rotate(0f, motor.transform.localEulerAngles.y, 0f, Space.Self);
			}
			visualTf.position = motor.groundPoint;
		}

		public void OrientVisualToSurfaceInAbility(float orientSpeed = 18f)
		{
			if (smoothRotation)
			{
				visualUp = Quaternion.Lerp(visualUp, Quaternion.FromToRotation(Vector3.up, motor.groundNormalVisual), 1f - Mathf.Exp((0f - orientSpeed) * Core.dt));
				SetVisualRotXZ(visualUp);
			}
			else
			{
				visualTf.up = Vector3.Lerp(visualTf.up, motor.groundNormalVisual, 1f - Mathf.Exp((0f - orientSpeed) * Core.dt));
				visualTf.Rotate(0f, motor.transform.localEulerAngles.y, 0f, Space.Self);
			}
			visualTf.position = Vector3.Lerp(visualTf.position, motor.groundPoint, 1f - Mathf.Exp((0f - orientSpeed) * Core.dt));
		}

		private void UpdateHoldProps()
		{
			UpdateSprayCanShake();
			if (isAI)
			{
				if (ability == pseudoGraffitiAbility)
				{
					characterVisual.SetSpraycan(set: true, character);
				}
				return;
			}
			float dt = Core.dt;
			if (phone.IsOn)
			{
				phoneLayerWeight += grabPhoneSpeed * dt;
				characterVisual.SetPhone(set: true);
				if (phoneLayerWeight >= 1f)
				{
					phoneLayerWeight = 1f;
				}
				anim.SetLayerWeight(3, phoneLayerWeight);
			}
			else
			{
				phoneLayerWeight -= grabPhoneSpeed * dt;
				if (phoneLayerWeight <= 0f)
				{
					phoneLayerWeight = 0f;
					characterVisual.SetPhone(set: false);
				}
				anim.SetLayerWeight(3, phoneLayerWeight);
			}
		}

		public void UpdateSprayCanShake()
		{
			if (IsDead())
			{
				return;
			}
			if (ability == talkAbility)
			{
				StopHoldProps();
			}
			bool flag = false;
			if (ability != null && !ability.canUseSpraycan)
			{
				if (spraycanState != SpraycanState.NONE)
				{
					StopHoldSpraycan();
				}
				flag = true;
			}
			switch (spraycanState)
			{
			case SpraycanState.NONE:
				if (sprayButtonHeld && talkContextAvailable <= 0 && !spraycanLocked && !flag)
				{
					SetSpraycanState(SpraycanState.START);
				}
				else if (spraycanLayerWeight > 0f)
				{
					spraycanLayerWeight -= grabSpraycanSpeed * Core.dt;
					if (spraycanLayerWeight <= 0f)
					{
						spraycanLayerWeight = 0f;
					}
					anim.SetLayerWeight(1, spraycanLayerWeight);
					anim.SetLayerWeight(2, spraycanLayerWeight);
					sprayHitbox.SetActive(value: false);
				}
				break;
			case SpraycanState.START:
				spraycanLayerWeight = Mathf.Min(spraycanLayerWeight + grabSpraycanSpeed * Core.dt, 1f);
				anim.SetLayerWeight(1, spraycanLayerWeight);
				anim.SetLayerWeight(2, spraycanLayerWeight);
				if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f)
				{
					if (sprayButtonHeld)
					{
						SetSpraycanState(SpraycanState.SHAKE);
					}
					else
					{
						SetSpraycanState(SpraycanState.NONE);
					}
				}
				else if (!sprayButtonHeld)
				{
					SetSpraycanState(SpraycanState.SPRAY);
				}
				break;
			case SpraycanState.SHAKE:
				if (!sprayButtonHeld)
				{
					SetSpraycanState(SpraycanState.SPRAY);
				}
				break;
			case SpraycanState.SPRAY:
			{
				AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(1);
				if (currentAnimatorStateInfo.shortNameHash == canSprayHash)
				{
					if (currentAnimatorStateInfo.normalizedTime >= 1f)
					{
						SetSpraycanState(SpraycanState.NONE);
					}
					else if (currentAnimatorStateInfo.normalizedTime >= 0.7f)
					{
						sprayHitbox.SetActive(value: false);
					}
				}
				break;
			}
			}
		}

		public void SetSpraycanState(SpraycanState state)
		{
			switch (state)
			{
			case SpraycanState.NONE:
				if (spraycanState == SpraycanState.SHAKE)
				{
					audioManager.StopLoopingSfx(playerAccessoryAudioSource);
				}
				characterVisual.SetSpraycan(set: false, character);
				break;
			case SpraycanState.START:
				spraycanLayerWeight = 0f;
				anim.SetLayerWeight(1, spraycanLayerWeight);
				anim.SetLayerWeight(2, spraycanLayerWeight);
				anim.Play(canStartShakeHash, 1, 0f);
				CreateCapPopEffect();
				characterVisual.SetSpraycan(set: true, character);
				break;
			case SpraycanState.SHAKE:
				spraycanLayerWeight = 1f;
				anim.SetLayerWeight(1, spraycanLayerWeight);
				anim.SetLayerWeight(2, spraycanLayerWeight);
				audioManager.PlaySfxGameplayLooping(SfxCollectionID.GraffitiSfx, AudioClipID.CanShakeLoop, playerAccessoryAudioSource);
				anim.Play(canShakeHash, 1, 0f);
				break;
			case SpraycanState.SPRAY:
				spraypaintParticles.Play();
				spraycanLayerWeight = 1f;
				anim.SetLayerWeight(1, spraycanLayerWeight);
				anim.SetLayerWeight(2, spraycanLayerWeight);
				anim.Play(canSprayHash, 1, 0f);
				audioManager.StopLoopingSfx(playerAccessoryAudioSource);
				audioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.Spray, playerOneShotAudioSource);
				sprayHitbox.SetActive(value: true);
				break;
			}
			spraycanState = state;
		}

		public void StopHoldProps()
		{
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
			SetSpraycanState(SpraycanState.NONE);
			characterVisual.SetPhone(set: false);
			anim.SetLayerWeight(3, 0f);
		}

		public void StopHoldSpraycan()
		{
			SetSpraycanState(SpraycanState.NONE);
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
		}

		public void PauseAnim(bool set)
		{
			anim.speed = ((!set) ? 1 : 0);
		}

		public void TempSetSpeedAnim(float speed)
		{
			anim.speed = speed;
		}

		public void PlayAnim(int newAnim, bool forceOverwrite = false, bool instant = false, float atTime = -1f)
		{
			if (!base.gameObject.activeSelf || (newAnim == curAnim && !forceOverwrite))
			{
				return;
			}
			float normalizedTransitionDuration = 0f;
			if (!instant)
			{
				if (animInfos.ContainsKey(newAnim) && animInfos[newAnim].fadeFrom.ContainsKey(curAnim))
				{
					normalizedTransitionDuration = animInfos[newAnim].fadeFrom[curAnim];
				}
				else if (animInfos.ContainsKey(curAnim) && animInfos[curAnim].fadeTo.ContainsKey(newAnim))
				{
					normalizedTransitionDuration = animInfos[curAnim].fadeTo[newAnim];
				}
			}
			if (atTime != -1f)
			{
				anim.CrossFade(newAnim, normalizedTransitionDuration, -1, atTime);
			}
			else
			{
				anim.CrossFade(newAnim, normalizedTransitionDuration);
			}
			curAnimActiveTime = 0f;
			firstFrameAnim = true;
			characterVisual.feetIK = animInfos.ContainsKey(newAnim) && animInfos[newAnim].feetIK;
			curAnim = newAnim;
		}

		public float GetAnimTime()
		{
			if (firstFrameAnim)
			{
				return 0f;
			}
			return anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer != 19)
			{
				return;
			}
			if (!isAI && other.CompareTag("StageExit"))
			{
				if (OnExitTriggerHit != null)
				{
					OnExitTriggerHit(this, other.GetComponentInParent<StageTransition>());
				}
			}
			else if (other.CompareTag("Teleport"))
			{
				other.transform.parent.GetComponent<Teleport>().DoTeleport(this);
			}
			else if (!isAI && other.CompareTag("CameraZone"))
			{
				if (cam.cameraZone == null)
				{
					cam.cameraZone = other.GetComponentInParent<CameraZone>();
				}
				if (cam.cameraZone.attachedToStageExit && phone.HandlePhoneCalling())
				{
					userInputEnabled = false;
				}
			}
			else if (!isAI && other.CompareTag("PublicToilet"))
			{
				PublicToilet componentInParent = other.GetComponentInParent<PublicToilet>();
				if (componentInParent != null && componentInParent.CanSwap)
				{
					componentInParent.DoSequence();
				}
			}
			else if (!isAI && other.CompareTag("GasBarrier"))
			{
				other.GetComponent<GassBarrier>().DoGas(this);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer == 19 && !isAI && other.CompareTag("CameraZone") && cam.cameraZone == other.GetComponentInParent<CameraZone>())
			{
				cam.cameraZone = null;
			}
		}

		internal static bool NoMenuOpen()
		{
			return !Core.Instance.UIManager.danceAbilityUI.IsEnabled;
		}

		internal bool IsNotInCutsceneOrGettingCalled()
		{
			return !IsBusyWithSequence() & !phone.IsBeingCalled;
		}

		public void OnTriggerStay(Collider other)
		{
			switch (other.gameObject.layer)
			{
			case 19:
				if (!isAI && (other.CompareTag("NPC") || ((bool)other.transform.parent && other.transform.parent.CompareTag("NPC"))) && IsNotInCutsceneOrGettingCalled() && NoMenuOpen())
				{
					NPC nPC = other.GetComponent<NPC>();
					if (!nPC)
					{
						nPC = other.GetComponentInParent<NPC>();
					}
					if (!nPC.activeTempRacerWithDialogue)
					{
						CheckNPCTriggerForConversation(nPC);
					}
				}
				else if (!isAI && other.CompareTag("TempRacerWithDialogue") && IsNotInCutsceneOrGettingCalled() && NoMenuOpen())
				{
					PlayerAI componentInParent = other.GetComponentInParent<PlayerAI>();
					if (componentInParent != null && CheckNPCTriggerForConversation(componentInParent.linkedNPC))
					{
						componentInParent.startsTalkingToPlayer = true;
					}
					CharacterSelectCharacter componentInParent2 = other.GetComponentInParent<CharacterSelectCharacter>();
					if (componentInParent2 != null)
					{
						CheckCharacterSelectCharacterTriggerForConversation(componentInParent2);
					}
				}
				else if (other.CompareTag("GraffitiSpot") && !IsBusyWithSequence())
				{
					GraffitiSpot component = other.GetComponent<GraffitiSpot>();
					if (!inGraffitiGame && component.CanDoGraffiti(this))
					{
						graffitiContextAvailable = 10;
						if (!isAI)
						{
							characterVisual.LookAtSubject(component.gameObject, component.transform.position - component.transform.forward * 0.25f);
						}
						graffitiSpotsAvailable.Add(component);
					}
				}
				else if (!isAI && other.CompareTag("ProgressObject") && !IsBusyWithSequence())
				{
					if (other.GetComponent<ProgressObject>() != null)
					{
						other.GetComponent<ProgressObject>().Trigger();
					}
					else
					{
						other.transform.parent.GetComponent<ProgressObject>().Trigger();
					}
				}
				else if (!isAI && other.CompareTag("Encounter") && !IsBusyWithSequence())
				{
					if (other.GetComponentInParent<Encounter>() != null)
					{
						other.GetComponentInParent<Encounter>().TriggerDetectPlayer(other, this);
					}
					else
					{
						other.transform.parent.GetComponentInParent<Encounter>().TriggerDetectPlayer(other, this);
					}
				}
				else if (!isAI && other.CompareTag("Spawner"))
				{
					other.GetComponentInParent<PlayerSpawner>().SetReached();
				}
				else if (!isAI && other.CompareTag("Battle"))
				{
					other.GetComponentInParent<Battle>().ActivateBattle();
				}
				else if (!isAI && other.CompareTag("Checkpoint"))
				{
					other.GetComponentInParent<Encounter>().CheckpointReached(other);
				}
				else if (other.CompareTag("MovingObject"))
				{
					other.GetComponentInParent<MoveAlongPoints>().TriggerDetectLayer(9);
				}
				else if (other.CompareTag("launcherZone"))
				{
					onLauncher = other.transform;
				}
				else if (other.CompareTag("walkZone"))
				{
					inWalkZone = true;
				}
				else if (other.CompareTag("speedZone"))
				{
					inSpeedZone = true;
				}
				else if (other.CompareTag("CypherZone"))
				{
					if (inCharacterSelectSpot == null)
					{
						inCharacterSelectSpot = other.GetComponentInParent<CharacterSelectSpot>();
					}
				}
				else if (other.CompareTag("SkateboardScrewPole"))
				{
					if (ability == null && moveStyle == MoveStyle.SKATEBOARD && handplantAbility.cooldownTimer <= 0f && !IsGrounded())
					{
						SkateboardScrewPole componentInParent3 = other.transform.parent.GetComponentInParent<SkateboardScrewPole>();
						handplantAbility.SetToPole(componentInParent3.point.position, componentInParent3);
					}
				}
				else if (other.CompareTag("SnakebossHead"))
				{
					other.transform.GetComponent<SnakeBossChestImpactReceiver>().WasHit = true;
				}
				break;
			case 30:
				if (other.CompareTag("MovingObject"))
				{
					other.GetComponentInParent<MoveAlongPoints>().TriggerDetectLayer(9);
				}
				break;
			case 16:
				if (isAI)
				{
					break;
				}
				if (other.CompareTag("Collectable"))
				{
					Collectable componentInParent7 = other.GetComponentInParent<Collectable>();
					if (componentInParent7.CanBePickedUp())
					{
						componentInParent7.PickupCollectable(this);
					}
				}
				else
				{
					Pickup componentInParent8 = other.GetComponentInParent<Pickup>();
					if (componentInParent8.CanBePickedUpBy(this))
					{
						componentInParent8.PickupPickup(this);
					}
				}
				break;
			case 11:
			{
				GrindLine component2 = other.GetComponent<GrindLine>();
				if (grindAbility.CanSetToLine(component2))
				{
					grindAbility.SetToLine(component2);
				}
				break;
			}
			case 20:
			{
				if (other.CompareTag("MovingObject"))
				{
					other.GetComponentInParent<MoveAlongPoints>().HitboxHitPlayer(this);
					break;
				}
				BasicEnemy componentInParent4 = other.GetComponentInParent<BasicEnemy>();
				if (componentInParent4 != null)
				{
					switch (componentInParent4.EnemyType)
					{
					case EnemyType.BASICCOP:
						GetHit(1, (componentInParent4.tf.position - tf.position).normalized, KnockbackAbility.KnockbackType.STATIONARY);
						other.gameObject.SetActive(value: false);
						break;
					case EnemyType.SHIELDCOP:
						GetHit(1, (componentInParent4.tf.position - tf.position).normalized, KnockbackAbility.KnockbackType.FAR);
						other.gameObject.SetActive(value: false);
						break;
					case EnemyType.BOSS_PRELUDE:
						if (ability != slideAbility)
						{
							GetHit(2, (componentInParent4.tf.position - tf.position).normalized, KnockbackAbility.KnockbackType.FAR);
							other.gameObject.SetActive(value: false);
						}
						break;
					case EnemyType.TANKWALKER:
						GetHit(1, (other.transform.position - tf.position).normalized, KnockbackAbility.KnockbackType.BIG);
						other.gameObject.SetActive(value: false);
						break;
					case EnemyType.TURRET:
						GetCaught(other.transform.GetChild(0), componentInParent4 as TurretEnemy);
						other.gameObject.SetActive(value: false);
						break;
					default:
						GetHit(1, (other.transform.position - tf.position).normalized, KnockbackAbility.KnockbackType.FAR);
						break;
					}
					DoGetHitEffect(componentInParent4.tf);
					EnemyMeleeWeapon componentInParent5 = other.GetComponentInParent<EnemyMeleeWeapon>();
					if (componentInParent5 != null)
					{
						componentInParent5.SetHitLanded();
					}
				}
				RailRider componentInParent6 = other.GetComponentInParent<RailRider>();
				if (componentInParent6 != null)
				{
					GetHit(1, (other.transform.position - tf.position).normalized, KnockbackAbility.KnockbackType.BIG);
					other.gameObject.SetActive(value: false);
					componentInParent6.HitPlayer();
				}
				break;
			}
			case 0:
			case 4:
			case 10:
			case 21:
			case 23:
				if (obstacleDetection.activeSelf)
				{
					AI.detectedObstacle = other;
				}
				break;
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			CheckWallrun(other);
			if (GetTotalSpeed() < maxMoveSpeed * 0.75f || other.gameObject.layer != 21)
			{
				return;
			}
			if (other.transform.parent != null)
			{
				JunkHolder componentInParent = other.transform.GetComponentInParent<JunkHolder>();
				if (componentInParent != null)
				{
					componentInParent.FallApart(other.contacts[0].point);
					return;
				}
			}
			Junk component = other.transform.GetComponent<Junk>();
			if ((bool)component)
			{
				component.FallApart();
			}
		}

		private void OnCollisionStay(Collision other)
		{
			CheckWallrun(other);
		}

		private void CheckWallrun(Collision other)
		{
			if (other.collider.gameObject.layer != 13 || IsGrounded() || !CanStartWallrun())
			{
				return;
			}
			bool flag = other.collider is BoxCollider;
			WallrunLine component = other.collider.GetComponent<WallrunLine>();
			if (!(component == null) && (!component.IsSameFamily(wallrunAbility.wallrunLine) || (wallrunAbility.CanStart() && ability != wallrunAbility)))
			{
				Vector3 normal = other.GetContact(0).normal;
				if (!flag || (normal.y > -0.1f && Vector3.Dot(other.collider.transform.forward, normal) > -0.1f))
				{
					wallrunAbility.wallrunLine = component;
					wallrunAbility.wallrunFaceNormal = other.collider.transform.forward;
					ActivateAbility(wallrunAbility);
				}
			}
		}

		private bool CheckNPCTriggerForConversation(NPC npc)
		{
			if (IsDead())
			{
				return false;
			}
			if (npc.CanDoConversationStarter(NPC.ConversationStarter.TRIGGER))
			{
				npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TRIGGER);
				return true;
			}
			if (npc.CanDoConversationStarter(NPC.ConversationStarter.TALK))
			{
				npc.lookAtPlayer = 3f;
				talkContextAvailable = 10;
				if (!isAI)
				{
					characterVisual.LookAtSubject(npc.gameObject, npc.GetLookAtPos());
				}
				if (sprayButtonNew)
				{
					if ((character == Characters.metalHead || character == Characters.legendFace || character == Characters.legendMetalHead) && npc.CanDoConversationStarter(NPC.ConversationStarter.TALK_RED))
					{
						npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TALK_RED);
					}
					else if (isGirl && npc.CanDoConversationStarter(NPC.ConversationStarter.TALK_GIRL))
					{
						npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TALK_GIRL);
					}
					else if (npc.CanDoConversationStarter(NPC.ConversationStarter.TALK_AND_REQUIRED_REP) && Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().reputation >= npc.requirement)
					{
						npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TALK_AND_REQUIRED_REP);
					}
					else
					{
						npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TALK);
					}
					return true;
				}
			}
			if (ability == danceAbility && danceAbility.triggerDanceConversationStarter)
			{
				if (npc.CanDoConversationStarter(NPC.ConversationStarter.TAXI_DANCE) && !Core.Instance.SaveManager.CurrentSaveSlot.taxiLocked)
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.TAXI_DANCE);
					return true;
				}
				if (danceAbility.danceNumber == 0 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_TOPROCK))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_TOPROCK);
					return true;
				}
				if (danceAbility.danceNumber == 1 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_POPPING))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_POPPING);
					return true;
				}
				if (danceAbility.danceNumber == 2 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_HOUSE))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_HOUSE);
					return true;
				}
				if (danceAbility.danceNumber == 3 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_BREAK))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_BREAK);
					return true;
				}
				if (danceAbility.danceNumber == 4 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_HIPHOP))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_HIPHOP);
					return true;
				}
				if (danceAbility.danceNumber == 5 && npc.CanDoConversationStarter(NPC.ConversationStarter.DANCE_LOCKING))
				{
					npc.StartTalkingWithConvoStarter(NPC.ConversationStarter.DANCE_LOCKING);
					return true;
				}
			}
			if (npc.CanDoConversationStarter(NPC.ConversationStarter.REQUIRED_REP) || npc.CanDoConversationStarter(NPC.ConversationStarter.TALK_AND_REQUIRED_REP))
			{
				showRepLingerTimer = 2f;
			}
			return false;
		}

		private bool CheckCharacterSelectCharacterTriggerForConversation(CharacterSelectCharacter character)
		{
			if (IsDead())
			{
				return false;
			}
			talkContextAvailable = 10;
			if (!isAI)
			{
				characterVisual.LookAtSubject(character.gameObject, character.GetLookAtPos());
			}
			if (sprayButtonNew)
			{
				character.DoSwapConversation();
				return true;
			}
			return false;
		}

		public static Vector3 AlignSetPositionToGroundIfClose(Vector3 setPosition, float reach = 2f)
		{
			int layerMask = 1041;
			if (Physics.Raycast(setPosition, Vector3.down, out var hitInfo, reach, layerMask))
			{
				setPosition.y -= hitInfo.distance;
			}
			return setPosition;
		}

		private void CheckAvailableGraffitiSpots()
		{
			float num = float.MaxValue;
			GraffitiSpot graffitiSpot = null;
			foreach (GraffitiSpot item in graffitiSpotsAvailable)
			{
				if (!(item != null))
				{
					continue;
				}
				float num2 = (tf.position - item.transform.position).magnitude;
				if (item.ClaimedByPlayableCrew())
				{
					num2 *= 4f;
				}
				if (item.GetType() == typeof(GraffitiSpotFinisher))
				{
					if (!item.gameObject.activeSelf)
					{
						continue;
					}
					num2 *= 0.25f;
				}
				if (num2 < num)
				{
					num = num2;
					graffitiSpot = item;
				}
			}
			if (!(graffitiSpot != null))
			{
				return;
			}
			if (isAI)
			{
				if (crew != Crew.NONE && num < 4f && pseudoGraffitiAbility.cooldown <= 0f)
				{
					pseudoGraffitiAbility.graffitiSpot = graffitiSpot;
					ActivateAbility(pseudoGraffitiAbility);
				}
			}
			else if ((preSprayButtonHeld && !sprayButtonHeld) || sprayButtonNew)
			{
				StartGraffitiMode(graffitiSpot);
			}
			graffitiSpotsAvailable.Clear();
		}

		public void GiveBackSpeedAfterHittingBreakable()
		{
			if (ability != null && ability.targetSpeed > 0f)
			{
				float num = ability.targetSpeed;
				if (ability.setSpeedOnHittingBreakable > 0f)
				{
					num = ability.setSpeedOnHittingBreakable;
				}
				Vector3 vector = num * dir;
				SetVelocity(new Vector3(vector.x, motor.velocity.y, vector.z));
			}
			else
			{
				SetVelocity(preVelocity);
			}
		}

		public void StartClosestCheckEnemyForAutoAim()
		{
			autoAimCheck.SetActive(value: true);
			closestEnemyDist = float.MaxValue;
			closestEnemy = null;
		}

		public void DetectEnemyForAutoAim(Transform enemyTf)
		{
			if (enemyTf != closestEnemy)
			{
				float sqrMagnitude = (enemyTf.position - tf.position).sqrMagnitude;
				if (sqrMagnitude < closestEnemyDist)
				{
					closestEnemyDist = sqrMagnitude;
					closestEnemy = enemyTf;
				}
			}
		}

		public void GetHit(int forDamage)
		{
			GetHit(forDamage, Vector3.zero, KnockbackAbility.KnockbackType.STATIONARY);
		}

		public void GetHit(int forDamage, Vector3 fromDir, KnockbackAbility.KnockbackType knockbackType)
		{
			if (isAI || ability == recoverAbility || ability == caughtAbility || ability == recoverAbility || ability == knockbackAbility || (!(Time.time > lastHit + invulnerabilityDuration) && knockbackType == KnockbackAbility.KnockbackType.STATIONARY) || ability == dieAbility)
			{
				return;
			}
			if (knockbackType == KnockbackAbility.KnockbackType.STATIONARY && (GetVelocity().magnitude > minMoveSpeed || !IsGrounded()))
			{
				if (doSmallHitCoroutine != null)
				{
					StopCoroutine(doSmallHitCoroutine);
				}
				doSmallHitCoroutine = StartCoroutine(DoSmallHit());
			}
			else if (knockbackType != KnockbackAbility.KnockbackType.NO_KNOCKBACK)
			{
				knockbackAbility.SetKnockback(fromDir, knockbackType);
				ActivateAbility(knockbackAbility);
			}
			StartScreenShake(ScreenShakeType.MEDIUM, 0.2f);
			SetInCombat();
			if (knockbackType != KnockbackAbility.KnockbackType.FALL)
			{
				PlayVoice(AudioClipID.VoiceGetHit, VoicePriority.COMBAT);
			}
			ChangeHP(forDamage);
			lastHit = Time.time;
		}

		private void GetCaught(Transform clawTransform, TurretEnemy turret)
		{
			caughtAbility.target = clawTransform;
			caughtAbility.turret = turret;
			ActivateAbility(caughtAbility);
		}

		private IEnumerator DoSmallHit()
		{
			anim.SetLayerWeight(4, 1f);
			PlayAnim(getHitSmallHash, forceOverwrite: true, instant: true, 0f);
			yield return new WaitForSeconds(0.25f);
			anim.SetLayerWeight(4, 0f);
		}

		public void ChangeHP(int dmg)
		{
			recoverDamageTimer = 0f;
			HP = Mathf.Clamp(HP - (float)dmg, 0f, maxHP);
			if (HP <= 0f)
			{
				ActivateAbility(dieAbility);
			}
		}

		public void UpdateCombat()
		{
			if (claimedTimer > 0f)
			{
				claimedTimer -= Core.dt;
			}
			if (ability != dieAbility)
			{
				int num = AmountOfCuffs();
				outOfCombatTimer -= Core.dt;
				if (num <= 0 && outOfCombatTimer <= 0f)
				{
					recoverDamageTimer += Core.dt;
					if (recoverDamageTimer > recoverDamageInterval)
					{
						ChangeHP(-1);
					}
				}
				else
				{
					recoverDamageTimer = 0f;
				}
				if (num == maxCuffs)
				{
					breakChainContextAvailable = 10;
				}
				else if (num <= 2)
				{
					breakChainContextAvailable = 0;
					dieTimer = 0f;
				}
			}
			else
			{
				breakChainContextAvailable = 0;
			}
		}

		public void SetInCombat()
		{
			outOfCombatTimer = outOfCombatDuration;
		}

		public float GetNormalizedHP()
		{
			return HP / maxHP;
		}

		private void InitCuffs()
		{
			for (int i = 0; i < attachedCuffs.Length; i++)
			{
				if (attachedCuffs[i] != null)
				{
					attachedCuffs[i].SetState(CuffMissileEnemy.CuffMissileState.RETURNING);
					attachedCuffs[i].transform.GetChild(0).gameObject.SetActive(value: true);
					attachedCuffs[i] = null;
				}
			}
			cuffs[0] = UnityEngine.Object.Instantiate(cuffPrefab, characterVisual.handR).GetComponent<PlayerCuff>();
			cuffs[0].SetState(PlayerCuff.CuffState.Hidden);
			cuffs[1] = UnityEngine.Object.Instantiate(cuffPrefab, characterVisual.handL).GetComponent<PlayerCuff>();
			cuffs[1].SetState(PlayerCuff.CuffState.Hidden);
			cuffs[2] = UnityEngine.Object.Instantiate(cuffPrefab, characterVisual.footR).GetComponent<PlayerCuff>();
			cuffs[2].SetState(PlayerCuff.CuffState.Hidden);
			cuffs[2].transform.localPosition = new Vector3(-0.025f, 0f, 0.05f);
			cuffs[2].transform.localEulerAngles = new Vector3(0f, 300f, 0f);
			cuffs[3] = UnityEngine.Object.Instantiate(cuffPrefab, characterVisual.footL).GetComponent<PlayerCuff>();
			cuffs[3].SetState(PlayerCuff.CuffState.Hidden);
			cuffs[3].transform.localPosition = new Vector3(0.025f, 0f, 0.05f);
			cuffs[3].transform.localEulerAngles = new Vector3(0f, 300f, 0f);
			cuffs[4] = UnityEngine.Object.Instantiate(cuffPrefab, characterVisual.head).GetComponent<PlayerCuff>();
			cuffs[4].SetState(PlayerCuff.CuffState.Hidden);
			cuffs[4].transform.localPosition = new Vector3(0.065f, 0f, -0.065f);
			cuffs[4].transform.localEulerAngles = new Vector3(0f, 350f, 0f);
		}

		public void RemoveCuff()
		{
			if (isAI)
			{
				return;
			}
			for (int num = maxCuffs - 1; num >= 0; num--)
			{
				if (attachedCuffs[num] != null)
				{
					StartHitpause(0.1f);
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.chainBreak, playerOneShotAudioSource);
					UnityEngine.Object.Instantiate(chainBreakEffect, attachedCuffs[num].transform.position, Quaternion.identity, null);
					cuffs[num].SetState(PlayerCuff.CuffState.Free);
					if (attachedCuffs[num] != null)
					{
						attachedCuffs[num].SetState(CuffMissileEnemy.CuffMissileState.RETURNING);
						attachedCuffs[num].transform.GetChild(0).gameObject.SetActive(value: true);
						attachedCuffs[num] = null;
					}
					breakCuffPower = 0;
					break;
				}
			}
		}

		public void TryToRemoveCuff(int power = 1)
		{
			if (isAI)
			{
				return;
			}
			breakCuffPower += power;
			int num = 0;
			if (breakCuffPower >= 1)
			{
				for (int num2 = maxCuffs - 1; num2 >= 0; num2--)
				{
					if (attachedCuffs[num2] != null)
					{
						num++;
						UnityEngine.Object.Instantiate(chainBreakEffect, attachedCuffs[num2].transform.position, Quaternion.identity, null);
						cuffs[num2].SetState(PlayerCuff.CuffState.Free);
						if (attachedCuffs[num2] != null)
						{
							attachedCuffs[num2].SetState(CuffMissileEnemy.CuffMissileState.RETURNING);
							attachedCuffs[num2].transform.GetChild(0).gameObject.SetActive(value: true);
							attachedCuffs[num2] = null;
						}
						breakCuffPower--;
						if (breakCuffPower <= 0)
						{
							break;
						}
					}
				}
				if (num > 0)
				{
					StartHitpause(0.1f);
					audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.chainBreak, playerOneShotAudioSource);
				}
			}
			if (breakCuffPower < 50)
			{
				return;
			}
			for (int i = 0; i < maxCuffs; i++)
			{
				if (cuffs[i].State == PlayerCuff.CuffState.Free)
				{
					UnityEngine.Object.Instantiate(cuffBreakEffect, cuffs[i].transform.position, Quaternion.identity, null);
					cuffs[i].SetState(PlayerCuff.CuffState.Hidden);
					breakCuffPower = 0;
					break;
				}
			}
		}

		public void RemoveAllCuffs(CuffMissileLauncher ofOwner = null)
		{
			if (isAI)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < maxCuffs; i++)
			{
				if (attachedCuffs[i] != null && (ofOwner == null || attachedCuffs[i].owner == ofOwner))
				{
					if (!flag)
					{
						audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.chainBreak, playerOneShotAudioSource);
						flag = true;
					}
					cuffs[i].SetState(PlayerCuff.CuffState.Hidden);
					UnityEngine.Object.Instantiate(chainBreakEffect, base.transform.position, Quaternion.identity, null);
					UnityEngine.Object.Instantiate(cuffBreakEffect, cuffs[i].transform.position, Quaternion.identity, null);
					if (attachedCuffs[i] != null)
					{
						attachedCuffs[i].SetState(CuffMissileEnemy.CuffMissileState.RETURNING);
						attachedCuffs[i].transform.GetChild(0).gameObject.SetActive(value: true);
						attachedCuffs[i] = null;
					}
				}
			}
			if (AmountOfCuffs() == 0)
			{
				breakCuffPower = 0;
			}
		}

		public int AmountOfCuffs()
		{
			int num = 0;
			for (int i = 0; i < maxCuffs; i++)
			{
				if (attachedCuffs[i] != null)
				{
					num++;
				}
			}
			return num;
		}

		public Transform AttachCuff(CuffMissileEnemy cuff)
		{
			Transform result = null;
			for (int i = 0; i < maxCuffs; i++)
			{
				if (attachedCuffs[i] == null)
				{
					StartScreenShake(ScreenShakeType.LIGHT, 0.2f);
					attachedCuffs[i] = cuff;
					result = SetCuffToBody(i, cuff);
					breakCuffPower = 0;
					cuffs[i].SetState(PlayerCuff.CuffState.Connected);
					return result;
				}
			}
			return result;
		}

		public Transform SetCuffToBody(int num, CuffMissileEnemy cuff)
		{
			Transform result = characterVisual.head;
			switch (num)
			{
			case 0:
				result = characterVisual.handR;
				break;
			case 1:
				result = characterVisual.handL;
				break;
			case 2:
				result = characterVisual.footR;
				break;
			case 3:
				result = characterVisual.footL;
				break;
			}
			return result;
		}

		public Vector3 GetNettoCuffPull(float targetSpeed)
		{
			Vector3 zero = Vector3.zero;
			if (targetSpeed > 0f)
			{
				for (int i = 0; i < maxCuffs; i++)
				{
					if (attachedCuffs[i] != null)
					{
						zero += Vector3.ProjectOnPlane(attachedCuffs[i].owner.transform.position - base.transform.position, Vector3.up).normalized * 2f;
					}
				}
			}
			return zero;
		}

		public void ResetHP()
		{
			HP = maxHP;
		}

		private void InitUIDisplay()
		{
			localizer = Core.Instance.Localizer;
			storyManager = WorldHandler.instance.StoryManager;
			ui = Core.Instance.UIManager.gameplay;
			ui.assignedPlayer = this;
			currentTrickUIStartPos = ui.currentTrickTransform.anchoredPosition;
			healthBarStartPos = ui.healthBarGroup.anchoredPosition;
			originalFontSize = ui.scoreCalcLabel.fontSize;
			rep = Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().reputation;
			repStringBuilder = new StringBuilder(3);
			SetRepLabel(rep);
			currentCulture = CultureInfo.CurrentCulture;
			SetLocalizationTexts();
			InitializeComboMascotUI();
			localizer.OnLanguageChanged += OnLanguageChanged;
		}

		private void SetLocalizationTexts()
		{
			noTricksUIText = localizer.GetUserInterfaceText("GAMEPLAY_NO_COMBO");
			trickComboUIText = localizer.GetUserInterfaceText("GAMEPLAY_TRICK_COMBO");
			trickUIText = localizer.GetUserInterfaceText("GAMEPLAY_TRICK");
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			SetLocalizationTexts();
			if (!Core.Instance.BaseModule.IsInGamePaused)
			{
				ScoreDisplayUpdate();
			}
		}

		private void InitializeComboMascotUI()
		{
			ComboMascotSystem.OnStartedCombo += OnStartedComboMascot;
			ui.comboMascotIcon.gameObject.SetActive(value: false);
			ui.comboMascotLabel.text = string.Empty;
		}

		private void CleanUpUIDisplay()
		{
			localizer.OnLanguageChanged -= OnLanguageChanged;
		}

		public void UpdateUIDisplay()
		{
			RepDisplayUpdate();
			ChargeAndSpeedDisplayUpdate();
			ScoreDisplayUpdate();
			ContextIconsUpdate();
			PickupAddUpdate();
			HealthBarUpdate();
			if (showInformationTimer > 0f)
			{
				showInformationTimer -= Core.dt;
				if (showInformationTimer <= 0f)
				{
					ui.informationLabel.text = "";
				}
			}
			if (showMusicUnlockInformationTimer > 0f)
			{
				showMusicUnlockInformationTimer -= Core.dt;
				if (showMusicUnlockInformationTimer < showMusicUnlockInformationDuration && showMusicUnlockInformationTimer > 0f)
				{
					ui.musicTitleLabel.text = showMusicUnlockInformationTitle;
					ui.musicArtistLabel.text = showMusicUnlockInformationArtist;
					ui.musicTrack.gameObject.SetActive(value: true);
				}
				if (showMusicUnlockInformationTimer <= 0f)
				{
					ui.musicTitleLabel.text = "";
					ui.musicArtistLabel.text = "";
					ui.musicTrack.gameObject.SetActive(value: false);
				}
			}
		}

		public void LateUpdateUIDisplay()
		{
			if ((bool)phone && phone.IsOn)
			{
				if (Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().mapFound)
				{
					UpdateUIIndicatorData();
				}
				else
				{
					GrafSpotVisualizationStop();
				}
			}
			else
			{
				GrafSpotVisualizationStop();
				StoryObjectVisualizationStop();
			}
		}

		public void ShowAddRep(int add)
		{
			showAddRep += add;
			showRepLingerTimer = 4f;
		}

		private void RepDisplayUpdate()
		{
			if (showRepLingerTimer > 0f)
			{
				ui.repGroup.SetActive(value: true);
				showAddRepTimer += Core.dt;
				if (showAddRep != 0)
				{
					if (showAddRepTimer >= 0.1f)
					{
						showAddRepTimer = 0f;
						if (showAddRep > 0)
						{
							rep++;
							showAddRep--;
						}
						else
						{
							rep--;
							showAddRep++;
						}
					}
				}
				else
				{
					showRepLingerTimer -= Core.dt;
					if (showRepLingerTimer <= 0f)
					{
						ui.repGroup.SetActive(value: false);
					}
				}
			}
			if (showingREP != rep)
			{
				SetRepLabel(rep);
				showingREP = rep;
			}
		}

		private void SetRepLabel(float set)
		{
			repStringBuilder.Clear();
			if (set < 10f)
			{
				repStringBuilder.Append("00");
			}
			else if (set < 100f)
			{
				repStringBuilder.Append("0");
			}
			repStringBuilder.Append(set);
			ui.repLabel.text = repStringBuilder.ToString();
		}

		private void ChargeAndSpeedDisplayUpdate()
		{
			_ = ui.chargeBar;
			if (showAddCharge > 0f)
			{
				showAddCharge = Mathf.Max(0f, showAddCharge - 0.5f);
			}
			float num = (boostCharge - showAddCharge) / maxBoostCharge;
			if (num >= prefChargeValue)
			{
				ui.chargeBar.fillAmount += Core.dt * 0.5f;
				ui.chargeBarChunk.fillAmount = (boostCharge - showAddCharge) / maxBoostCharge;
				ui.chargeBar.fillAmount = Mathf.Min(ui.chargeBar.fillAmount, ui.chargeBarChunk.fillAmount);
			}
			else
			{
				Image chargeBar = ui.chargeBar;
				float fillAmount = (ui.chargeBarChunk.fillAmount = (boostCharge - showAddCharge) / maxBoostCharge);
				chargeBar.fillAmount = fillAmount;
			}
			ui.chargeBar.color = ui.chargeBarDefaultColor;
			prefChargeValue = num;
		}

		private void OnStartedComboMascot(ComboMascotSystem comboMascotSystem)
		{
			currentComboMascotSystem = comboMascotSystem;
		}

		private void ScoreDisplayUpdate()
		{
			if (showLastScoreTimer > 0f)
			{
				showLastScoreTimer -= Core.dt;
				showLastScoreAnimationTimer -= Core.dt;
				if (scoreCalcPreviousFrame != -3f)
				{
					scoreCalcPreviousFrame = -3f;
					ui.scoreCalcLabel.text = FormattingUtility.FormatPlayerScore(currentCulture, lastScore);
				}
				scoreMultiplierPreviousFrame = -1f;
				ui.scoreMultiplierLabel.text = string.Empty;
				ui.trickNameLabel.text = string.Empty;
				if ((int)(showLastScoreTimer * 6f) % 2 == 0)
				{
					if (tricksInLastCombo > 1)
					{
						tricksInComboPreviousFrame = -3f;
						FormatTrickInCombo(tricksInLastCombo, string.Empty, trickComboUIText);
					}
					else
					{
						ShowNoComboText();
					}
				}
				else
				{
					tricksInComboPreviousFrame = -1f;
					ui.tricksInComboLabel.text = string.Empty;
				}
				if (showLastScoreAnimationTimer > showLastScoreAnimationDuration * 0.5f)
				{
					ui.scoreCalcLabel.fontSize = Mathf.Lerp(originalFontSize * 1.5f, originalFontSize * 2f, 1f - (showLastScoreAnimationTimer / showLastScoreAnimationDuration - 0.5f) * 2f);
				}
				if (showLastScoreAnimationTimer < showLastScoreAnimationDuration * 0.5f)
				{
					ui.scoreCalcLabel.fontSize = Mathf.Lerp(originalFontSize * 2f, originalFontSize * 1.5f, 1f - showLastScoreAnimationTimer / showLastScoreAnimationDuration * 2f);
				}
				Image comboTimeOutBar = ui.comboTimeOutBar;
				float fillAmount = (ui.comboTimeOutBackdrop.fillAmount = 0f);
				comboTimeOutBar.fillAmount = fillAmount;
			}
			else
			{
				ui.scoreCalcLabel.fontSize = originalFontSize;
				if (comboTimeOutTimer < 1f)
				{
					ui.comboTimeOutBar.fillAmount = comboTimeOutTimer / 1f;
					ui.comboTimeOutBackdrop.fillAmount = 1f;
				}
				else if (comboTimeOutShowResetTimer > 0f && IsComboing())
				{
					comboTimeOutShowResetTimer -= Core.dt;
					if (ui.comboTimeOutBackdrop.fillAmount == 0f)
					{
						Image comboTimeOutBar2 = ui.comboTimeOutBar;
						float fillAmount = (ui.comboTimeOutBackdrop.fillAmount = 1f);
						comboTimeOutBar2.fillAmount = fillAmount;
					}
					else
					{
						Image comboTimeOutBar3 = ui.comboTimeOutBar;
						float fillAmount = (ui.comboTimeOutBackdrop.fillAmount = 0f);
						comboTimeOutBar3.fillAmount = fillAmount;
					}
				}
				else
				{
					Image comboTimeOutBar4 = ui.comboTimeOutBar;
					float fillAmount = (ui.comboTimeOutBackdrop.fillAmount = 0f);
					comboTimeOutBar4.fillAmount = fillAmount;
				}
			}
			if (IsComboing())
			{
				if (showLastScoreTimer <= 0f || baseScore * scoreMultiplier > lastScore)
				{
					if (baseScore != scoreCalcPreviousFrame)
					{
						scoreCalcPreviousFrame = baseScore;
						ui.scoreCalcLabel.text = $"{FormattingUtility.FormatPlayerScore(currentCulture, baseScore)} X";
					}
					if (scoreMultiplier != scoreMultiplierPreviousFrame)
					{
						scoreMultiplierPreviousFrame = scoreMultiplier;
						ui.scoreMultiplierLabel.text = scoreMultiplier.ToString();
					}
					ui.scoreTrickLabel.text = string.Empty;
					ui.trickNameLabel.text = currentTrickName;
					if ((float)tricksInCombo != tricksInComboPreviousFrame)
					{
						tricksInComboPreviousFrame = tricksInCombo;
						FormatTrickInCombo(tricksInCombo, trickUIText, trickComboUIText);
					}
					moveInScoreTimer = Mathf.Max(moveInScoreTimer - Core.dt * 8f, 0f);
					ui.currentTrickTransform.anchoredPosition = currentTrickUIStartPos - Vector3.right * moveInScoreTimer * 300f;
				}
			}
			else
			{
				if (showLastScoreTimer <= 0f)
				{
					scoreCalcPreviousFrame = -1f;
					scoreMultiplierPreviousFrame = -1f;
					tricksInComboPreviousFrame = -1f;
					TextMeshProUGUI scoreTrickLabel = ui.scoreTrickLabel;
					TextMeshProUGUI scoreCalcLabel = ui.scoreCalcLabel;
					TextMeshProUGUI scoreMultiplierLabel = ui.scoreMultiplierLabel;
					TextMeshProUGUI trickNameLabel = ui.trickNameLabel;
					string text = (ui.tricksInComboLabel.text = string.Empty);
					string text2 = (trickNameLabel.text = text);
					string text4 = (scoreMultiplierLabel.text = text2);
					string text6 = (scoreCalcLabel.text = text4);
					scoreTrickLabel.text = text6;
				}
				moveInScoreTimer = Mathf.Max(moveInScoreTimer - Core.dt * 8f, 0f);
				ui.currentTrickTransform.anchoredPosition = currentTrickUIStartPos - Vector3.right * moveInScoreTimer * 300f;
			}
			UpdateComboMascotScoreUI();
		}

		private void FormatTrickInCombo(int comboAmount, string singleComboText, string multipleComboText)
		{
			if (comboAmount > 1)
			{
				ui.tricksInComboLabel.text = $"{comboAmount} {multipleComboText}";
			}
			else
			{
				ui.tricksInComboLabel.text = $"{comboAmount} {singleComboText}";
			}
		}

		private void ShowNoComboText()
		{
			tricksInComboPreviousFrame = -2f;
			ui.tricksInComboLabel.text = noTricksUIText;
		}

		private void UpdateComboMascotScoreUI()
		{
			if (!(currentComboMascotSystem == null))
			{
				if (currentComboMascotSystem.IsCheckingComboMascots)
				{
					ui.comboMascotIcon.gameObject.SetActive(value: true);
					ui.comboMascotLabel.text = $"{currentComboMascotSystem.mascotsHit}/{currentComboMascotSystem.comboMascots.Length}";
				}
				else
				{
					currentComboMascotSystem = null;
					ui.comboMascotIcon.gameObject.SetActive(value: false);
					ui.comboMascotLabel.text = string.Empty;
				}
			}
		}

		private void SetUpGrafSpotData()
		{
			foreach (GraffitiSpot grafSpot in WorldHandler.instance.GetGrafSpots(onlyUnclaimed: false))
			{
				GrafSpotUIIndicatorData grafSpotUIIndicatorData = new GrafSpotUIIndicatorData();
				grafSpotUIIndicatorData.graffitiSpot = grafSpot;
				grafSpotUIIndicatorData.trans = grafSpot.transform;
				grafSpotUIIndicatorData.uiObject = CreateGrafSpotUIVisualization(phone.grafSpotUI);
				grafSpotUIIndicatorData.timeTillShow = UIIndicatorAppearDelay;
				grafSpotUIIndicatorData.id = grafSpotUIIndicatorDatas.Count;
				grafSpotUIIndicatorData.SetComponents();
				grafSpotUIIndicatorDatas.Add(grafSpotUIIndicatorData);
			}
		}

		private void SetUpStoryObjectData()
		{
			for (int i = 0; i < 20; i++)
			{
				UIIndicatorData obj = (storyUIIndicatorData[i] = new UIIndicatorData());
				obj.isActive = false;
				obj.uiObject = CreateGrafSpotUIVisualization(phone.storySpotUI);
				obj.SetComponents();
				obj.timeTillShow = UIIndicatorAppearDelay;
			}
		}

		private GameObject CreateGrafSpotUIVisualization(GameObject uiObject)
		{
			GameObject obj = UnityEngine.Object.Instantiate(uiObject);
			obj.transform.SetParent(phone.dynamicGameplayScreen, worldPositionStays: false);
			obj.transform.localScale = Vector2.one;
			obj.SetActive(value: false);
			return obj;
		}

		private void UpdateUIIndicatorData()
		{
			foreach (GrafSpotUIIndicatorData grafSpotUIIndicatorData in grafSpotUIIndicatorDatas)
			{
				grafSpotUIIndicatorData.isActive = grafSpotUIIndicatorData.trans.gameObject.activeInHierarchy;
				if (!grafSpotUIIndicatorData.isActive)
				{
					continue;
				}
				grafSpotUIIndicatorData.isClaimed = grafSpotUIIndicatorData.graffitiSpot.ClaimedByPlayableCrew();
				if (!grafSpotUIIndicatorData.isClaimed)
				{
					Vector3 position = grafSpotUIIndicatorData.trans.position;
					grafSpotUIIndicatorData.inRange = (((position - tf.position).sqrMagnitude < grafSpotUIIndicatorRange) ? true : false);
					if (grafSpotUIIndicatorData.inRange)
					{
						grafSpotUIIndicatorData.inView = InView(grafSpotUIIndicatorData, position);
						if (occludeGrafSpotUIIndicators && Time.frameCount % grafSpotUIIndicatorDatas.Count == grafSpotUIIndicatorData.id)
						{
							Vector3 position2 = cam.realTf.position;
							new Ray(position, position2 - position);
							_ = position - position2;
							grafSpotUIIndicatorData.isOccluded = true;
						}
					}
				}
				bool showUI = grafSpotUIIndicatorData.inView && !grafSpotUIIndicatorData.isClaimed && grafSpotUIIndicatorData.inRange && grafSpotUIIndicatorData.graffitiSpot.beTargetForObjective == Story.ObjectiveID.NONE;
				UpdateUIIndicatorAnimation(showUI, grafSpotUIIndicatorData, Vector3.zero);
			}
			GameObject[] currentObjectivesObjects = storyManager.GetCurrentObjectivesObjects(out amountOfActiveStoryData);
			for (int i = 0; i < amountOfActiveStoryData; i++)
			{
				UIIndicatorData uIIndicatorData = storyUIIndicatorData[i];
				GameObject gameObject = currentObjectivesObjects[i];
				uIIndicatorData.trans = gameObject.transform;
				uIIndicatorData.isActive = gameObject.activeInHierarchy;
				if (!uIIndicatorData.isActive)
				{
					continue;
				}
				Vector3 position3 = uIIndicatorData.trans.position;
				uIIndicatorData.inView = InView(uIIndicatorData, position3);
				if (uIIndicatorData.inView)
				{
					Vector3 position4 = cam.realTf.position;
					new Ray(position3, position4 - position3);
					Vector3 direction = position3 - position4;
					if (Physics.Raycast(position4, direction, out var hitInfo, 1000f, uiIndicatorOcclusionLayerMask, QueryTriggerInteraction.Collide))
					{
						uIIndicatorData.isOccluded = hitInfo.collider.transform != uIIndicatorData.trans;
					}
				}
				storyUIIndicatorData[i].trans.gameObject.GetComponent<GraffitiSpot>();
				bool inView = uIIndicatorData.inView;
				Vector3 offset = ((uIIndicatorData.trans.GetComponent<NPC>() != null) ? (Vector3.up * 1.5f) : Vector3.zero);
				UpdateUIIndicatorAnimation(inView, uIIndicatorData, offset);
			}
			for (int num = 19; num >= amountOfActiveStoryData; num--)
			{
				UIIndicatorData uIIndicatorData2 = storyUIIndicatorData[num];
				uIIndicatorData2.isActive = false;
				UpdateUIIndicatorAnimation(showUI: false, uIIndicatorData2, Vector3.zero);
			}
		}

		private void GrafSpotVisualizationStop()
		{
			foreach (GrafSpotUIIndicatorData grafSpotUIIndicatorData in grafSpotUIIndicatorDatas)
			{
				AnimateUIIndicatorOut(grafSpotUIIndicatorData);
			}
		}

		private void StoryObjectVisualizationStop()
		{
			for (int i = 0; i < amountOfActiveStoryData; i++)
			{
				AnimateUIIndicatorOut(storyUIIndicatorData[i]);
			}
		}

		private bool InView(UIIndicatorData uiIndicatordata, Vector3 position)
		{
			Vector3 vector = cam.cam.WorldToViewportPoint(uiIndicatordata.trans.position);
			if ((uiIndicatordata.inView && (double)vector.x > 1.1) || (double)vector.x < -0.1 || (double)vector.y > 1.1 || (double)vector.y < -0.1 || vector.z < 0f)
			{
				return false;
			}
			if (vector.x < 1f && vector.x > 0f && vector.y < 1f && vector.y > 0f && vector.z > 0f)
			{
				return true;
			}
			return false;
		}

		private void UpdateUIIndicatorAnimation(bool showUI, UIIndicatorData uiIndicatorData, Vector3 offset)
		{
			if (!showUI)
			{
				if (uiIndicatorData.uiObject.activeSelf)
				{
					uiIndicatorData.uiObject.SetActive(value: false);
				}
			}
			else if (uiIndicatorData.timeTillShow <= 0f)
			{
				Vector3 position = cam.cam.WorldToScreenPoint(uiIndicatorData.trans.position + offset);
				position.z = 0f;
				uiIndicatorData.uiObject.transform.position = position;
				bool gotBackInView = false;
				if (!uiIndicatorData.uiObject.activeSelf)
				{
					uiIndicatorData.uiObject.SetActive(value: true);
					gotBackInView = true;
				}
				if (uiIndicatorData.isOccluded)
				{
					UpdateScaleInUIIndicatorAnimation(uiIndicatorData, gotBackInView);
				}
				else
				{
					UpdateOccludedUIIndicatorAnimation(uiIndicatorData, gotBackInView);
				}
			}
			else
			{
				uiIndicatorData.timeTillShow -= Core.dt / ((uiIndicatorData.trans.position - cam.transform.position).magnitude * 0.002f);
			}
		}

		private void UpdateOccludedUIIndicatorAnimation(UIIndicatorData uiIndicatorData, bool gotBackInView)
		{
			if (uiIndicatorData.CompareCurrentAnimationState("AppearOccluded") || uiIndicatorData.CompareCurrentAnimationState("CloseToOccluded"))
			{
				if (!uiIndicatorData.CompareCurrentAnimationState("ScaleInFromOccluded") | gotBackInView)
				{
					uiIndicatorData.PlayAnimationForIndicator("ScaleInFromOccluded");
				}
			}
			else if (!uiIndicatorData.CompareCurrentAnimationState("ScaleIn") | gotBackInView)
			{
				uiIndicatorData.PlayAnimationForIndicator("ScaleIn");
			}
		}

		private void UpdateScaleInUIIndicatorAnimation(UIIndicatorData uiIndicatorData, bool gotBackInView)
		{
			if (uiIndicatorData.CompareCurrentAnimationState("ScaleIn") || uiIndicatorData.CompareCurrentAnimationState("ScaleInFromOccluded"))
			{
				if (!uiIndicatorData.CompareCurrentAnimationState("CloseToOccluded") | gotBackInView)
				{
					uiIndicatorData.PlayAnimationForIndicator("CloseToOccluded");
				}
			}
			else if (!uiIndicatorData.CompareCurrentAnimationState("AppearOccluded") | gotBackInView)
			{
				uiIndicatorData.PlayAnimationForIndicator("AppearOccluded");
			}
		}

		private void AnimateUIIndicatorOut(UIIndicatorData uiIndicatorData)
		{
			if ((bool)uiIndicatorData.uiObject)
			{
				bool flag = uiIndicatorData.CompareCurrentAnimationState("Hide");
				if (!flag)
				{
					uiIndicatorData.PlayAnimationForIndicator("Hide");
					flag = true;
				}
				if (flag && uiIndicatorData.uiObjectCanvasGroup.alpha == 0f && !uiIndicatorData.uiObject.activeSelf)
				{
					uiIndicatorData.uiObject.SetActive(value: false);
					uiIndicatorData.timeTillShow = UIIndicatorAppearDelay;
				}
			}
		}

		public void ContextIconsUpdate()
		{
			talkContextAvailable = Mathf.Max(talkContextAvailable - 1, 0);
			graffitiContextAvailable = Mathf.Max(graffitiContextAvailable - 1, 0);
			breakChainContextAvailable = Mathf.Max(breakChainContextAvailable - 1, 0);
			bool flag = graffitiContextAvailable > 0;
			bool flag2 = talkContextAvailable > 0;
			bool flag3 = breakChainContextAvailable > 0;
			ui.breakChainsContextIcon.gameObject.SetActive(flag3 && Mathf.Sin(Time.realtimeSinceStartup * 30f) > -0.5f);
			ui.contextGraffitiIcon.gameObject.SetActive(flag && !flag3);
			ui.contextTalkIcon.gameObject.SetActive(flag2 && !flag && !flag3);
			if (!flag2 && !flag && !flag3)
			{
				ui.HideContextLabelUI();
				return;
			}
			if (flag3)
			{
				ui.contextLabelUiButtonGlyphComponent.SetButtonTextGlyph(12);
			}
			else if (flag | flag2)
			{
				ui.contextLabelUiButtonGlyphComponent.SetButtonTextGlyph(10);
			}
			ui.ShowContextLabelUI();
		}

		public void DestroyPickupVisuals()
		{
			for (int num = pickupDisplays.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(pickupDisplays[num].pickupUIElement);
				pickupDisplays.RemoveAt(num);
			}
		}

		public void PickupAddUpdate()
		{
			for (int num = pickupDisplays.Count - 1; num >= 0; num--)
			{
				PickupDisplay pickupDisplay = pickupDisplays[num];
				if (pickupDisplay.displayTimeLeft > 0f)
				{
					pickupDisplay.displayTimeLeft -= Core.dt;
				}
				else
				{
					UnityEngine.Object.Destroy(pickupDisplay.pickupUIElement);
					pickupDisplays.RemoveAt(num);
				}
				float time = 1f - pickupDisplay.displayTimeLeft / pickupDisplay.displayDuration;
				if (pickupDisplay.pickupType == Pickup.PickUpType.BOOST_BIG_CHARGE || pickupDisplay.pickupType == Pickup.PickUpType.BOOST_CHARGE)
				{
					pickupDisplay.pickupUIElement.transform.position = Vector2.Lerp(pickupDisplay.startPosition, Vector2.Lerp(ui.chargeBar.rectTransform.position, ui.maxChargePosition.position, ui.chargeBar.fillAmount), ui.pickupTransition.Evaluate(time));
					pickupDisplay.pickupUIElement.transform.position += Vector3.up * ui.pickupBounce.Evaluate(time);
					pickupDisplay.pickupUIElement.transform.localScale = Vector3.one * ui.pickupScale.Evaluate(time);
				}
				else if (pickupDisplay.pickupType == Pickup.PickUpType.MUSIC_UNLOCKABLE || pickupDisplay.pickupType == Pickup.PickUpType.GRAFFITI_UNLOCKABLE)
				{
					pickupDisplay.pickupUIElement.transform.position = Vector2.Lerp(pickupDisplay.startPosition, ui.musicCollectablePickupLocation.position, ui.collectableTransition.Evaluate(time));
					pickupDisplay.pickupUIElement.transform.position += Vector3.up * ui.collectableBounce.Evaluate(time);
					pickupDisplay.pickupUIElement.transform.localScale = Vector3.one * ui.collectableScale.Evaluate(time);
				}
				else if (pickupDisplay.pickupType == Pickup.PickUpType.FORTUNE_BOOST)
				{
					pickupDisplay.pickupUIElement.transform.position = Vector2.Lerp(pickupDisplay.startPosition, Vector2.Lerp(ui.chargeBar.rectTransform.position, ui.maxChargePosition.position, ui.chargeBar.fillAmount), ui.fortuneTransition.Evaluate(time));
					pickupDisplay.pickupUIElement.transform.position += Vector3.up * ui.fortuneBounce.Evaluate(time);
					pickupDisplay.pickupUIElement.transform.localScale = Vector3.one * ui.fortuneScale.Evaluate(time);
				}
			}
		}

		public PickupDisplay PickupVisualization(Vector3 pickupPosition, Pickup.PickUpType pickupType, bool worldPos = true)
		{
			if (pickupType == Pickup.PickUpType.BOOST_CHARGE || pickupType == Pickup.PickUpType.BOOST_BIG_CHARGE || pickupType == Pickup.PickUpType.MUSIC_UNLOCKABLE || pickupType == Pickup.PickUpType.GRAFFITI_UNLOCKABLE || pickupType == Pickup.PickUpType.FORTUNE_BOOST)
			{
				PickupDisplay pickupDisplay = new PickupDisplay();
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<CanvasRenderer>();
				gameObject.AddComponent<RectTransform>();
				gameObject.AddComponent<Image>();
				gameObject.transform.SetParent(ui.gameplayScreen, worldPositionStays: false);
				gameObject.transform.position = (worldPos ? cam.cam.WorldToScreenPoint(pickupPosition) : pickupPosition);
				gameObject.transform.localScale = Vector2.one;
				pickupDisplay.pickupUIElement = gameObject;
				pickupDisplay.startPosition = gameObject.transform.position;
				pickupDisplay.displayTimeLeft = (pickupDisplay.displayDuration = 0.525f);
				pickupDisplay.pickupType = pickupType;
				pickupDisplays.Add(pickupDisplay);
				switch (pickupType)
				{
				case Pickup.PickUpType.BOOST_CHARGE:
					gameObject.GetComponent<Image>().sprite = ui.chargeBoost;
					break;
				case Pickup.PickUpType.BOOST_BIG_CHARGE:
					gameObject.GetComponent<Image>().sprite = ui.chargeBoostBig;
					break;
				case Pickup.PickUpType.MUSIC_UNLOCKABLE:
					gameObject.GetComponent<Image>().sprite = ui.musicCollectable;
					break;
				case Pickup.PickUpType.GRAFFITI_UNLOCKABLE:
					gameObject.GetComponent<Image>().sprite = ui.graffitiCollectable;
					break;
				case Pickup.PickUpType.FORTUNE_BOOST:
					gameObject.GetComponent<Image>().sprite = ui.chargeBoost;
					gameObject.transform.localScale = Vector2.zero;
					break;
				}
				return pickupDisplay;
			}
			return null;
		}

		public IEnumerator GiveBoostStack(Transform fromTf, int boostPickups)
		{
			WaitForSeconds waitSeconds = new WaitForSeconds(0.015f);
			int i = 0;
			while (i < boostPickups)
			{
				AddBoostCharge(Pickup.boostChargeValue);
				PickupVisualization(fromTf.position + UnityEngine.Random.insideUnitSphere * 0.5f, Pickup.PickUpType.BOOST_CHARGE);
				yield return waitSeconds;
				int num = i + 1;
				i = num;
			}
			yield return null;
		}

		private static void SetHealthFillAmount(RectTransform fillTransform, float newFillAmount01)
		{
			Vector2 sizeDelta = fillTransform.sizeDelta;
			sizeDelta.x = newFillAmount01 * 248f;
			fillTransform.sizeDelta = sizeDelta;
		}

		public void HealthBarUpdate()
		{
			float normalizedHP = GetNormalizedHP();
			if (safe && normalizedHP < 1f)
			{
				safeTimer = 15f;
			}
			safe = normalizedHP >= 1f && safeTimer <= 0f;
			if (!safe && normalizedHP >= 1f)
			{
				safeTimer -= Core.dt;
			}
			if (!safe && healthBarAnimTimer < 2f)
			{
				healthBarAnimTimer += Core.dt * 2f;
				prefFillAmount = ui.healthBarFill.rectTransform.sizeDelta.x * 0.004032258f;
				if (!ui.healthBarGroup.gameObject.activeInHierarchy)
				{
					ui.healthBarGroup.gameObject.SetActive(value: true);
				}
			}
			else if (safe && healthBarAnimTimer > 0f)
			{
				healthBarAnimTimer -= Core.dt * 2f;
				prefFillAmount = 1f;
				if (ui.healthBarGroup.gameObject.activeInHierarchy)
				{
					ui.healthBarGroup.gameObject.SetActive(value: false);
				}
			}
			if (!safe)
			{
				Vector3 vector = base.transform.position + (cam.topLocalLookAt + cam.postureOffsetTarget) * base.transform.up;
				float num = 1f - (cam.transform.position - vector).magnitude / cam.dragDistanceDefault;
				Vector3 vector2 = -cam.transform.right * 0.6f;
				Vector3 vector3 = base.transform.up * 0.1f;
				Vector3 vector4 = vector2 - vector2 * num + (vector3 - vector3 * num);
				Vector3 b = cam.cam.WorldToScreenPoint(vector4 + vector);
				ui.healthBarGroup.transform.position = Vector3.Lerp(ui.healthBarGroup.transform.position, b, 0.8f);
			}
			RectTransform rectTransform = ui.healthBarFill.rectTransform;
			RectTransform rectTransform2 = ui.healthBarFillChunk.rectTransform;
			float num2 = normalizedHP;
			float num3 = rectTransform2.sizeDelta.x * 0.004032258f;
			num3 -= Core.dt * 0.5f;
			num3 = Mathf.Max(Mathf.Min((!Mathf.Approximately(prefFillAmount, num2)) ? prefFillAmount : num3, num3), num2);
			SetHealthFillAmount(rectTransform, num2);
			SetHealthFillAmount(rectTransform2, num3);
			if (num2 < 0.35f)
			{
				ui.healthBarFill.color = ((Mathf.Sin(Time.realtimeSinceStartup * 20f) > 0f) ? Color.red : ui.healthBarDefaultColor);
			}
			else
			{
				ui.healthBarFill.color = ui.healthBarDefaultColor;
			}
		}

		public void ShowWantedUI(bool shouldShow)
		{
			if ((bool)ui)
			{
				if (!showAPB & shouldShow)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.policeAlarm);
				}
				showAPB = shouldShow;
				ui.apb.gameObject.SetActive(showAPB);
			}
		}

		public void APBTimeOut(float percentageRemaining, int currentStar)
		{
			RectTransform rectTransform = null;
			switch (currentStar)
			{
			case 6:
				rectTransform = ui.wanted6;
				break;
			case 5:
				rectTransform = ui.wanted5;
				break;
			case 4:
				rectTransform = ui.wanted4;
				break;
			case 3:
				rectTransform = ui.wanted3;
				break;
			case 2:
				rectTransform = ui.wanted2;
				break;
			case 1:
				rectTransform = ui.wanted1;
				break;
			}
			if (percentageRemaining < 1f && rectTransform != null)
			{
				rectTransform.gameObject.SetActive(Mathf.Sin(Time.realtimeSinceStartup * 20f) > 0f);
			}
			else if (rectTransform != null && !rectTransform.gameObject.activeSelf)
			{
				rectTransform.gameObject.SetActive(value: true);
			}
		}

		public void ChangeWantedStars(int numberOfStars)
		{
			ui.wanted1.gameObject.SetActive(numberOfStars > 0);
			ui.wanted2.gameObject.SetActive(numberOfStars > 1);
			ui.wanted3.gameObject.SetActive(numberOfStars > 2);
			ui.wanted4.gameObject.SetActive(numberOfStars > 3);
			ui.wanted5.gameObject.SetActive(numberOfStars > 4);
			ui.wanted6.gameObject.SetActive(numberOfStars > 5);
			switch (numberOfStars)
			{
			case 1:
				PlayStarAnim(ui.wanted1.gameObject);
				break;
			case 2:
				PlayStarAnim(ui.wanted2.gameObject);
				break;
			case 3:
				PlayStarAnim(ui.wanted3.gameObject);
				break;
			case 4:
				PlayStarAnim(ui.wanted4.gameObject);
				break;
			case 5:
				PlayStarAnim(ui.wanted5.gameObject);
				break;
			case 6:
				PlayStarAnim(ui.wanted6.gameObject);
				break;
			}
		}

		private void PlayStarAnim(GameObject star)
		{
			star.GetComponent<Animator>().Play("wantedStar");
		}

		public void ShowInformation(string info, float duration)
		{
			ui.informationLabel.text = info;
			showInformationTimer = duration;
		}

		public void ShowMusicUnlockInformation(string artist, string title, float duration, float delay)
		{
			showMusicUnlockInformationTimer = duration + delay;
			showMusicUnlockInformationDuration = duration;
			showMusicUnlockInformationTitle = title;
			showMusicUnlockInformationArtist = artist;
		}

		public void StartGraffitiMode(GraffitiSpot graffitiSpot)
		{
			if (!IsBusyWithSequence() && !IsDead() && ability != danceAbility)
			{
				Core.Instance.PauseCore(PauseType.Graffiti);
				ui.gameplayScreen.gameObject.SetActive(value: false);
				ui.graffitiScreen.gameObject.SetActive(value: true);
				phone.dynamicGameplayScreen.gameObject.SetActive(value: false);
				StopHoldProps();
				preStopAnimState = curAnim;
				preStopAnimTime = GetAnimTime();
				WorldHandler.instance.ActivateHumanPlayerGameObject(set: false);
				cam.gameObject.SetActive(value: false);
				UnityEngine.Object.Instantiate(graffitiGamePrefab).Init(this, graffitiSpot);
				inGraffitiGame = true;
				phone.AllowPhone(isAllowed: false);
			}
		}

		public void EndGraffitiMode(GraffitiSpot graffitiSpot)
		{
			FlushInput();
			ui.gameplayScreen.gameObject.SetActive(value: true);
			ui.graffitiScreen.gameObject.SetActive(value: false);
			phone.dynamicGameplayScreen.gameObject.SetActive(value: true);
			phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
			Core.Instance.UnPauseCore(PauseType.Graffiti);
			WorldHandler.instance.ActivateHumanPlayerGameObject(set: true);
			cam.gameObject.SetActive(value: true);
			inGraffitiGame = false;
			PlayAnim(preStopAnimState, forceOverwrite: true, instant: true, preStopAnimTime);
			if (!graffitiSpot.ClaimedByPlayableCrew())
			{
				return;
			}
			if (graffitiSpot.state == GraffitiState.FINISHED)
			{
				if (graffitiSpot.OnFinish.GetPersistentEventCount() == 0 && WorldHandler.instance.currentEncounter == null && graffitiSpot.reportAsCrime)
				{
					switch (graffitiSpot.size)
					{
					case GraffitiSize.S:
						WantedManager.instance.CrimePerpetrated(WantedManager.Crime.VANDALISM_SMALL);
						break;
					case GraffitiSize.M:
						WantedManager.instance.CrimePerpetrated(WantedManager.Crime.VANDALISM_MEDIUM);
						break;
					case GraffitiSize.L:
						WantedManager.instance.CrimePerpetrated(WantedManager.Crime.VANDALISM_LARGE);
						break;
					case GraffitiSize.XL:
						WantedManager.instance.CrimePerpetrated(WantedManager.Crime.VANDALISM_XLARGE);
						break;
					}
				}
				RegainAirMobility();
				newSpraycan = true;
				if (newGraffitiBonusMulti > 1)
				{
					if (IsBusyWithSequence())
					{
						AddBoostCharge(Pickup.boostChargeValue * (float)graffitiSpot.size * 10f);
					}
					else
					{
						StartCoroutine(GiveBoostStack(graffitiSpot.transform, (int)graffitiSpot.size * 10));
					}
				}
			}
			if (Core.Instance.Platform.HasAchievementsSupport)
			{
				playerAchievements.CheckStageGraffitiAchievement();
			}
		}

		public void ResetMoveVariables()
		{
			inSpeedZone = false;
			inWalkZone = false;
			inCharacterSelectSpot = null;
			onLauncher = null;
			characterSelectAbility.spaceEnough = true;
		}

		public void InitMovement(MoveStyle moveStyleToSet)
		{
			moveStyleTestCycleNum = (int)moveStyleToSet;
			SetMoveStyle(MoveStyle.ON_FOOT, changeProp: false, changeAnim: false);
			SetCurrentMoveStyleEquipped(moveStyleToSet, changeProp: false, changeAnim: false);
			landHash = Animator.StringToHash("land");
			jumpHash = Animator.StringToHash("jump");
			getHitSmallHash = Animator.StringToHash("getHitSmall");
		}

		public void SetIgnoreCamInput(bool ignore)
		{
			ignoreCamInput = ignore;
			cameraAxisX = 0f;
			cameraAxisY = 0f;
			cameraAxisZoom = 0f;
		}

		public void SetInputs(UserInputHandler.InputBuffer inputBuffer)
		{
			preMoveInputPlain = moveInputPlain;
			ref Vector2 reference = ref moveInputPlain;
			float x = (moveAxisX = inputBuffer.moveAxisX);
			reference.x = x;
			ref Vector2 reference2 = ref moveInputPlain;
			x = (moveAxisY = inputBuffer.moveAxisY);
			reference2.y = x;
			Vector3 vector = Vector3.ClampMagnitude(new Vector3(moveAxisX, 0f, moveAxisY), 1f);
			if (isAI)
			{
				moveInput = vector;
			}
			else
			{
				Vector3 normalized = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.forward, Vector3.up).normalized;
				if (normalized.sqrMagnitude == 0f)
				{
					normalized = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.up, Vector3.up).normalized;
				}
				Quaternion quaternion = Quaternion.LookRotation(normalized, Vector3.up);
				moveInput = quaternion * vector;
			}
			moveTap = Vector3.zero;
			moveTap.x += ((preMoveInputPlain.x < 0.5f && moveInputPlain.x >= 0.5f) ? 1 : 0);
			moveTap.x -= ((preMoveInputPlain.x > -0.5f && moveInputPlain.x <= -0.5f) ? 1 : 0);
			moveTap.y += ((preMoveInputPlain.y < 0.5f && moveInputPlain.y >= 0.5f) ? 1 : 0);
			moveTap.y -= ((preMoveInputPlain.y > -0.5f && moveInputPlain.y <= -0.5f) ? 1 : 0);
			jumpButtonNew = inputBuffer.jumpButtonNew;
			if (jumpButtonNew)
			{
				timeSinceJumpRequested = 0f;
				jumpRequested = true;
			}
			jumpButtonHeld = inputBuffer.jumpButtonHeld;
			preSprayButtonHeld = inputBuffer.sprayButtonHeldPrevFrame;
			sprayButtonHeld = inputBuffer.sprayButtonHeld;
			sprayButtonNew = inputBuffer.sprayButtonNew;
			trick1ButtonHeld = inputBuffer.trick1ButtonHeld;
			trick1ButtonNew = inputBuffer.trick1ButtonNew;
			trick2ButtonHeld = inputBuffer.trick2ButtonHeld;
			trick2ButtonNew = inputBuffer.trick2ButtonNew;
			trick3ButtonHeld = inputBuffer.trick3ButtonHeld;
			trick3ButtonNew = inputBuffer.trick3ButtonNew;
			slideButtonHeld = inputBuffer.slideButtonHeld;
			slideButtonNew = inputBuffer.slideButtonNew;
			danceButtonHeld = inputBuffer.danceButtonHeld;
			danceButtonNew = inputBuffer.danceButtonNew;
			boostButtonNew = inputBuffer.boostButtonNew;
			boostButtonHeld = inputBuffer.boostButtonHeld;
			switchStyleButtonNew = inputBuffer.switchStyleButtonNew;
			switchStyleButtonHeld = inputBuffer.switchStyleButtonHeld;
			walkButtonHeld = inputBuffer.walkButtonHeld;
			walkButtonNew = inputBuffer.walkButtonNew;
		}

		public void FlushInput()
		{
			moveAxisX = 0f;
			moveAxisY = 0f;
			jumpButtonNew = false;
			jumpButtonHeld = false;
			sprayButtonHeld = false;
			sprayButtonNew = false;
			danceButtonHeld = false;
			danceButtonNew = false;
			boostButtonHeld = false;
			boostButtonNew = false;
			phoneButtonNew = false;
			walkButtonHeld = false;
			walkButtonNew = false;
			trick1ButtonHeld = false;
			trick1ButtonNew = false;
			trick2ButtonHeld = false;
			trick2ButtonNew = false;
			trick3ButtonHeld = false;
			trick3ButtonNew = false;
			slideButtonHeld = false;
			slideButtonNew = false;
			jumpRequested = false;
			moveInput = (moveInputPlain = (preMoveInputPlain = Vector3.zero));
			preSprayButtonHeld = false;
		}

		public void UpdateRotation()
		{
			if (vertShape != null)
			{
				return;
			}
			Vector3 customTargetRot = moveInput;
			float rotSpeed;
			if (targetMovement == MovementType.NONE && ability == null)
			{
				rotSpeed = 0f;
			}
			else
			{
				if ((ability != null && !ability.normalRotation) || AirBraking())
				{
					return;
				}
				rotSpeed = ((!IsGrounded()) ? stats.rotSpeedInAir : Mathf.Lerp(stats.rotSpeedAtStill, stats.rotSpeedAtMaxSpeed, GetVelocity().magnitude / maxMoveSpeed));
				if (!IsGrounded())
				{
					rotSpeed = stats.rotSpeedInAir;
				}
				if (ability != null)
				{
					if (ability.customTargetRot != Vector3.zero)
					{
						customTargetRot = ability.customTargetRot;
					}
					if (ability.rotateSpeed != -1f)
					{
						rotSpeed = ability.rotateSpeed;
					}
				}
			}
			preDir = dir;
			motor.Rotate(customTargetRot, rotSpeed);
		}

		internal bool AirBraking()
		{
			if (!IsGrounded() && forwardInput < aimBackForAirBrake && (isAI || Vector3.Dot(cam.realTf.forward, dir) > 0.7f))
			{
				return ability != vertAbility;
			}
			return false;
		}

		protected Vector3 GetDesiredVelocity()
		{
			float num = ((ability != null && ability.targetSpeed != -1f) ? ability.targetSpeed : ((moveStyle == MoveStyle.SPECIAL_SKATEBOARD) ? SetTargetMoveGiveSpeed(MovementType.RUNNING) : ((targetMovement == MovementType.RUNNING) ? ((!(moveInput.magnitude <= 0.5f) && !walkButtonHeld) ? maxMoveSpeed : SetTargetMoveGiveSpeed(MovementType.WALKING)) : ((targetMovement == MovementType.WALKING) ? ((moveInput.magnitude >= 0.52f && !walkButtonHeld) ? SetTargetMoveGiveSpeed(MovementType.RUNNING) : ((moveInput.magnitude != 0f) ? minMoveSpeed : SetTargetMoveGiveSpeed(MovementType.NONE))) : ((!(moveInput.magnitude >= 0.02f)) ? 0f : SetTargetMoveGiveSpeed(MovementType.WALKING))))));
			return motor.dir * num + GetNettoCuffPull(num);
		}

		private float SetTargetMoveGiveSpeed(MovementType type)
		{
			targetMovement = type;
			return type switch
			{
				MovementType.WALKING => minMoveSpeed, 
				MovementType.RUNNING => maxMoveSpeed, 
				_ => 0f, 
			};
		}

		protected void Move()
		{
			motor.DetectGround();
			if (!CheckVert())
			{
				if (!IsInHitpause())
				{
					if (ability == null || ability.normalMovement)
					{
						Vector3 desiredVelocity;
						float acceleration;
						float deceleration;
						if (AirBraking())
						{
							desiredVelocity = Vector3.zero;
							acceleration = (deceleration = stats.airDecc * 3f);
						}
						else
						{
							if (IsGrounded())
							{
								acceleration = stats.groundAcc;
								deceleration = stats.groundDecc;
							}
							else
							{
								acceleration = stats.airAcc;
								deceleration = ((!maintainSpeedJump || !(timeSinceLastJump < 1f)) ? ((GetForwardSpeed() > maxMoveSpeed) ? aboveMaxAirDecc : stats.airDecc) : 0f);
							}
							if (ability != null)
							{
								if (ability.acc != -1f)
								{
									acceleration = ability.acc;
								}
								if (ability.decc != -1f)
								{
									deceleration = ability.decc;
								}
							}
							desiredVelocity = GetDesiredVelocity();
						}
						motor.MoveDrive(desiredVelocity, acceleration, deceleration);
					}
					else
					{
						motor.velocity = ability.customVelocity;
					}
				}
				HandleJump();
			}
			reallyMoved = tf.position - posPrev;
			reallyMovedVelocity = reallyMoved / Core.dt;
			posPrev = tf.position;
		}

		public void HandleJump()
		{
			jumpedThisFrame = false;
			timeSinceJumpRequested += Core.dt;
			if (jumpRequested && JumpIsAllowed())
			{
				Jump();
			}
		}

		public void Jump()
		{
			audioManager.PlayVoice(ref currentVoicePriority, character, AudioClipID.VoiceJump, playerGameplayVoicesAudioSource);
			PlayAnim(jumpHash);
			float num = 1f;
			if (targetMovement == MovementType.RUNNING && IsGrounded() && motor.groundCollider.gameObject.layer != 23 && Vector3.Dot(Vector3.ProjectOnPlane(motor.groundNormal, Vector3.up).normalized, dir) < -0.5f)
			{
				num = 1f + Mathf.Min(motor.groundAngle / 90f, 0.3f);
			}
			ForceUnground();
			float num2 = 0f;
			if (timeSinceGrinding <= JumpPostGroundingGraceTime)
			{
				num2 = bonusJumpSpeedGrind;
			}
			else if (timeSinceWallrunning <= JumpPostGroundingGraceTime)
			{
				num2 = bonusJumpSpeedWallrun;
			}
			float num3 = jumpSpeed * num + num2;
			if (num2 != 0f && slideButtonHeld)
			{
				num3 *= abilityShorthopFactor;
				maintainSpeedJump = true;
			}
			else
			{
				maintainSpeedJump = false;
			}
			if ((bool)onLauncher)
			{
				if (!onLauncher.parent.gameObject.name.Contains("Super"))
				{
					motor.SetVelocityYOneTime(jumpSpeedLauncher);
				}
				else
				{
					motor.SetVelocityYOneTime(jumpSpeedLauncher * 1.4f);
				}
				if (targetMovement == MovementType.RUNNING && Vector3.Dot(dir, onLauncher.back()) > 0.7f && !onLauncher.parent.gameObject.name.Contains("flat"))
				{
					SetForwardSpeed(maxMoveSpeed + 2f);
				}
				audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.launcher_woosh, playerOneShotAudioSource);
				DoHighJumpEffects(motor.groundNormalVisual * -1f);
			}
			else
			{
				DoJumpEffects(motor.groundNormalVisual * -1f);
				motor.SetVelocityYOneTime(num3);
				isJumping = true;
			}
			jumpRequested = false;
			jumpConsumed = true;
			jumpedThisFrame = true;
			timeSinceLastJump = 0f;
			if (ability != null)
			{
				ability.OnJump();
				if (ability != null && (bool)onLauncher && ability.autoAirTrickFromLauncher)
				{
					ActivateAbility(airTrickAbility);
				}
			}
			else if ((bool)onLauncher)
			{
				ActivateAbility(airTrickAbility);
			}
		}

		public void DoJumpEffects(Vector3 dir)
		{
			Core.Instance.AudioManager.PlaySfxGameplay(moveStyle, AudioClipID.jump, playerOneShotAudioSource);
			CreateCircleDustEffect(dir);
		}

		public void DoHighJumpEffects(Vector3 dir)
		{
			audioManager.PlaySfxGameplay(moveStyle, AudioClipID.jump, playerOneShotAudioSource);
			CreateHighJumpDustEffect(dir);
		}

		public bool JumpIsAllowed()
		{
			bool flag = ability == null || ability.allowNormalJump;
			if ((!jumpConsumed & flag) && vertShape == null)
			{
				if (!IsGrounded())
				{
					return timeSinceLastAbleToJump <= JumpPostGroundingGraceTime;
				}
				return true;
			}
			return false;
		}

		public void UpdateJumpStatus()
		{
			if (jumpRequested && timeSinceJumpRequested > JumpPreGroundingGraceTime)
			{
				jumpRequested = false;
			}
			timeSinceLastJump += Core.dt;
			if (ability != null && !ability.allowNormalJump)
			{
				ability.UpdateAbilityJumpStatus();
				return;
			}
			timeSinceGrinding += Core.dt;
			timeSinceWallrunning += Core.dt;
			if (((IsGrounded() && !motor.wasGrounded) || (ability != null && ability != slideAbility)) && playerMovementLoopAudioSource.clip == audioManager.GetAudioClipFromID(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.AirTime) && playerMovementLoopAudioSource.isPlaying)
			{
				playerMovementLoopAudioSource.Stop();
			}
			if (IsGrounded())
			{
				if (!jumpedThisFrame)
				{
					jumpConsumed = false;
					isJumping = false;
					jumpLow = false;
					if (!motor.wasGrounded && !skipLanding && vertShape == null)
					{
						OnLanded();
					}
					else
					{
						skipLanding = false;
					}
				}
				timeSinceLastAbleToJump = 0f;
				lastElevationForSlideBoost = tf.position.y;
			}
			else
			{
				timeSinceLastAbleToJump += Core.dt;
				if (isJumping && !jumpLow && timeSinceLastJump > 0.06f && timeSinceLastJump < 0.08f && motor.velocity.y > jumpSpeed * 0.76f && !jumpButtonHeld)
				{
					motor.SetVelocityYOneTime(motor.velocity.y * 0.76f);
					jumpLow = true;
				}
			}
		}

		public bool IsGrounded()
		{
			if (motor.isGrounded)
			{
				return !motor.ForcedUnground();
			}
			return false;
		}

		public bool OnAnyGround()
		{
			if (motor.isOnGround)
			{
				return !motor.ForcedUnground();
			}
			return false;
		}

		public bool IsOnNonStableGround()
		{
			if (motor.isOnNonStableGround)
			{
				return !motor.ForcedUnground();
			}
			return false;
		}

		public bool TreatPlayerAsSortaGrounded()
		{
			if (!IsGrounded())
			{
				if (ability != null)
				{
					return ability.treatPlayerAsSortaGrounded;
				}
				return false;
			}
			return true;
		}

		public void SetPosAndRotHard(Vector3 pos, Quaternion rot)
		{
			if (playerAchievements != null)
			{
				playerAchievements.StopFalling();
			}
			tf.position = pos;
			tf.rotation = rot;
			SetRotation(rot);
			if ((bool)visualTf)
			{
				visualTf.localRotation = Quaternion.identity;
			}
		}

		public void SetRotHard(Quaternion rot)
		{
			tf.rotation = rot;
			SetRotation(rot);
		}

		public void SetRotHard(Vector3 rot)
		{
			tf.rotation = Quaternion.LookRotation(rot);
			SetRotation(rot);
		}

		internal void SetRotation(Quaternion rotation)
		{
			SetRotation(rotation * Vector3.forward, rotation);
		}

		internal void SetRotation(Vector3 rotation)
		{
			SetRotation(rotation, Quaternion.LookRotation(rotation));
		}

		internal void SetRotation(Vector3 rotation, Vector3 upwards)
		{
			SetRotation(rotation, Quaternion.LookRotation(rotation, upwards));
		}

		internal void SetRotation(Vector3 rotDir, Quaternion rot)
		{
			motor.dir = rotDir;
			motor.rotation = rot;
		}

		public void SetForwardSpeed(float speed)
		{
			float y = motor.velocity.y;
			Vector3 vector = dir * speed;
			vector.y = 0f;
			motor.velocity = new Vector3(vector.x, y, vector.z);
		}

		public void SetSpeedFlat(float speed)
		{
			Vector3 velocity = dir * speed;
			velocity.y = 0f;
			motor.velocity = velocity;
		}

		public float GetTotalSpeed()
		{
			return motor.velocity.magnitude;
		}

		public float GetForwardSpeed()
		{
			return motor.forwardSpeed;
		}

		public void SetVelocity(Vector3 velocity)
		{
			motor.velocity = velocity;
		}

		public void SetVelocityToMax()
		{
			SetVelocity(dir * maxMoveSpeed);
		}

		public bool HasBoostCharge(float amount = 0f)
		{
			if (!((amount > 0f) ? (boostCharge >= amount) : (boostCharge > 0f)))
			{
				return isAI;
			}
			return true;
		}

		public void AddBoostCharge(float amount)
		{
			boostCharge += amount;
			boostCharge = Mathf.Clamp(boostCharge, 0f, maxBoostCharge);
		}

		public float CalculateBoostChargeForSeconds(float sec)
		{
			return maxBoostCharge * 0.1f * sec;
		}

		public void UseBoostChargeForBoost()
		{
			AddBoostCharge(Core.dt * (0f - maxBoostCharge * 0.1f));
		}

		protected void OnLanded()
		{
			OrientVisualInstant();
			if (motor.groundRigidbody != null)
			{
				Car component = motor.groundRigidbody.GetComponent<Car>();
				if (component != null)
				{
					component.PlayerLandsOn();
				}
			}
			if (ability == null)
			{
				if (GetForwardSpeed() <= minMoveSpeed + 1f)
				{
					PlayAnim(landHash);
				}
				else
				{
					PlayAnim(landRunHash);
				}
				if (GetForwardSpeed() > maxMoveSpeed)
				{
					SetSpeedFlat(maxMoveSpeed);
				}
				if (slideButtonHeld && !slideAbility.locked)
				{
					ActivateAbility(slideAbility);
				}
				else
				{
					LandCombo();
				}
			}
			else if (ability == boostAbility)
			{
				LandCombo();
			}
			audioManager.PlaySfxGameplay(moveStyle, AudioClipID.land, playerOneShotAudioSource);
			CreateCircleDustEffect(motor.groundNormalVisual * -1f);
		}

		public Vector3 FlattenRotation()
		{
			Vector3 normalized = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
			SetRotation(normalized, Vector3.up);
			return normalized;
		}

		public Vector3 FlattenRotationHard()
		{
			Vector3 normalized = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
			SetRotHard(Quaternion.LookRotation(normalized, Vector3.up));
			return normalized;
		}

		protected void OnLeaveStableGround()
		{
		}

		public void OnDiscreteCollisionDetected(Collider hitCollider)
		{
		}

		internal Vector3 GetVelocity()
		{
			return motor.velocity;
		}

		internal Vector3 GetPracticalVelocity()
		{
			if (IsGrinding())
			{
				return grindAbility.GetGrindMovementAsVelocity();
			}
			if (ability == wallrunAbility)
			{
				return wallrunAbility.GetVelocity();
			}
			return motor.velocity;
		}

		public Vector3 GetPracticalWorldVelocity()
		{
			if (IsGrinding())
			{
				return grindAbility.GetGrindMovementAsVelocity();
			}
			if (ability == wallrunAbility)
			{
				return wallrunAbility.GetVelocity();
			}
			return motor.velocity + motor.platformVelocity;
		}

		internal Vector3 GetFlatVelocity()
		{
			return Vector3.ProjectOnPlane(motor.velocity, Vector3.up);
		}

		internal bool CanJump()
		{
			if (IsGrounded() || ability == grindAbility || ability == wallrunAbility || ability == handplantAbility)
			{
				return true;
			}
			return false;
		}

		internal void ForceUnground(bool resetVisual = true)
		{
			if (resetVisual)
			{
				OrientVisualInstantReset();
			}
			motor.ForceUnground();
		}

		internal bool VertSurfaceMovingFast()
		{
			if (moveStyle != MoveStyle.ON_FOOT && GetTotalSpeed() > maxMoveSpeed)
			{
				return motor.groundCollider.gameObject.layer == 23;
			}
			return false;
		}

		public void HideForSequence(bool set)
		{
			if (set && base.gameObject.activeSelf)
			{
				preStopAnimState = curAnim;
				preStopAnimTime = GetAnimTime();
				base.gameObject.SetActive(value: false);
			}
			else if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
				anim.Play(preStopAnimState, -1, preStopAnimTime);
			}
		}

		public void PauseForSequence(bool set)
		{
			if (set && base.enabled)
			{
				prePauseVelocity = motor.velocity;
				motor.velocity = Vector3.zero;
				base.enabled = false;
			}
			else if (!base.enabled)
			{
				motor.velocity = prePauseVelocity;
				base.enabled = true;
			}
		}

		public bool IsTooBusyForSuddenCutscenes()
		{
			if (!IsBusyWithSequence())
			{
				return inGraffitiGame;
			}
			return true;
		}

		public bool IsBusyWithSequence()
		{
			if (sequenceState != SequenceState.NONE)
			{
				return !Core.Instance.UIManager.IsShowingNotification;
			}
			return false;
		}

		public void DoTrick(TrickType type, string trickName = "", int trickNum = 0)
		{
			switch (type)
			{
			case TrickType.AIR:
				airTricks[trickNum].Use(this);
				break;
			case TrickType.AIR_VERT:
				airVertTricks[trickNum].Use(this);
				ResetComboTimeOut();
				RefreshDegrade(type);
				break;
			case TrickType.AIR_BOOST:
				audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.specialTrick, playerOneShotAudioSource);
				airBoostTricks[trickNum].Use(this);
				ResetComboTimeOut();
				RefreshDegrade(type);
				break;
			case TrickType.GRIND_START:
				grindStartTricks[trickNum].Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.GRIND:
				grindTricks[trickNum].Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.GRIND_BOOST:
				audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.specialTrick, playerOneShotAudioSource);
				grindBoostTricks[trickNum].Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.GROUND:
				groundTricks[trickNum].Use(this);
				RefreshDegrade(type);
				break;
			case TrickType.GRIND_SWITCH_MOVESTYLE:
				grindStartTricks[0].Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.GROUND_BOOST:
				audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.specialTrick, playerOneShotAudioSource);
				groundBoostTricks[trickNum].Use(this);
				ResetComboTimeOut();
				RefreshDegrade(type);
				break;
			case TrickType.SLIDE:
				slideTrick.Use(this);
				RefreshDegrade(type);
				break;
			case TrickType.SPECIAL_AIR:
				specialAirTrick.Use(this);
				RefreshDegrade(type);
				break;
			case TrickType.HIT_BOUNCE:
				hitbounceTrick.Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.HANDPLANT:
				handplantTrick.Use(this);
				break;
			case TrickType.WALLRUN:
				wallrunTrick.Use(this);
				RefreshDegrade(type);
				ResetComboTimeOut();
				break;
			case TrickType.POLE_FLIP:
				poleFlipTrick.Use(this);
				break;
			case TrickType.GRAFFITI_S:
				currentTrickPoints = 500;
				ResetComboTimeOut();
				break;
			default:
				if (IsGraffitiTypeTrick(type))
				{
					if (type == TrickType.GRAFFITI_S)
					{
						currentTrickPoints = 500;
					}
					else
					{
						newGraffitiBonusMulti = (graffitiTitlesDone.Contains(trickName) ? 1 : 2);
						graffitiTitlesDone.Add(trickName);
						if (type == TrickType.GRAFFITI_M)
						{
							currentTrickPoints = 1000 * newGraffitiBonusMulti;
						}
						if (type == TrickType.GRAFFITI_L)
						{
							currentTrickPoints = 1600 * newGraffitiBonusMulti;
						}
						if (type == TrickType.GRAFFITI_XL)
						{
							currentTrickPoints = 2000 * newGraffitiBonusMulti;
						}
					}
					ResetComboTimeOut();
				}
				else
				{
					switch (type)
					{
					case TrickType.NEW_HARD_CORNER:
						currentTrickPoints = 150;
						AddScoreMultiplier();
						break;
					case TrickType.HARD_CORNER:
						currentTrickPoints = 40;
						break;
					case TrickType.SOFT_CORNER:
						currentTrickPoints = 25;
						break;
					}
				}
				break;
			}
			moveInScoreTimer = 1f;
			if ((currentTrickType == TrickType.SLIDE && type == TrickType.SLIDE) || (currentTrickType == TrickType.SOFT_CORNER && type == TrickType.SOFT_CORNER))
			{
				moveInScoreTimer = 0f;
			}
			else
			{
				if (tricksInCombo == 0 && ui != null)
				{
					ui.SetTrickingChargeBarActive(setActive: true);
				}
				tricksInCombo++;
				if (tricksInCombo >= 5)
				{
					AddBoostCharge(showAddCharge = gainBoostChargeCurve.Evaluate(Mathf.Min(tricksInCombo, 50f) / 50f));
				}
			}
			if (type != TrickType.GRIND_START && type != TrickType.SLIDE && !IsGraffitiTypeTrick(type) && type != TrickType.SOFT_CORNER)
			{
				bool flag = type == TrickType.AIR_BOOST || type == TrickType.GRIND_BOOST || type == TrickType.GROUND_BOOST || type == TrickType.SPECIAL_AIR;
				TryToRemoveCuff((!flag) ? 1 : 5);
			}
			if (scoreMultiplier == 0f)
			{
				scoreMultiplier = 1f;
			}
			if (trickName == "")
			{
				trickName = type.ToString();
			}
			currentTrickType = type;
			currentTrickName = trickName;
			currentTrickOnFoot = !usingEquippedMovestyle;
			baseScore += (int)((float)currentTrickPoints * scoreFactor);
			didAbilityTrick = true;
		}

		private bool IsGraffitiTypeTrick(TrickType type)
		{
			if (type != TrickType.GRAFFITI_S && type != TrickType.GRAFFITI_M && type != TrickType.GRAFFITI_L)
			{
				return type == TrickType.GRAFFITI_XL;
			}
			return true;
		}

		public void RefreshDegrade(TrickType trickType = TrickType.NONE)
		{
			bool changed = false;
			if (trickType != TrickType.GRIND && trickType != TrickType.GRIND_START && trickType != TrickType.GRIND_BOOST)
			{
				for (int i = 0; i < 3; i++)
				{
					grindTricks[i].Refresh(ref changed);
					grindStartTricks[i].Refresh(ref changed);
					grindBoostTricks[i].Refresh(ref changed);
				}
			}
			else if (trickType != TrickType.GROUND && trickType != TrickType.GROUND_BOOST)
			{
				for (int j = 0; j < 3; j++)
				{
					groundTricks[j].Refresh(ref changed);
					groundBoostTricks[j].Refresh(ref changed);
				}
			}
			else if (trickType != TrickType.AIR_VERT && trickType != TrickType.AIR_BOOST)
			{
				for (int k = 0; k < 3; k++)
				{
					airVertTricks[k].Refresh(ref changed);
					airBoostTricks[k].Refresh(ref changed);
				}
			}
			else if (trickType != TrickType.WALLRUN)
			{
				wallrunTrick.Refresh(ref changed);
			}
			else if (trickType != TrickType.POLE_FLIP)
			{
				poleFlipTrick.Refresh(ref changed);
			}
			if ((trickType != TrickType.NONE) & changed)
			{
				RefreshAirTricks();
			}
		}

		public void RefreshAirTricks()
		{
			bool changed = false;
			for (int i = 0; i < 3; i++)
			{
				airTricks[i].Refresh(ref changed);
			}
		}

		public void RefreshAllDegrade()
		{
			bool changed = false;
			for (int i = 0; i < 3; i++)
			{
				grindTricks[i].Refresh(ref changed);
				grindStartTricks[i].Refresh(ref changed);
				grindBoostTricks[i].Refresh(ref changed);
				groundTricks[i].Refresh(ref changed);
				groundBoostTricks[i].Refresh(ref changed);
				airVertTricks[i].Refresh(ref changed);
				airBoostTricks[i].Refresh(ref changed);
				airTricks[i].Refresh(ref changed);
			}
			wallrunTrick.Refresh(ref changed);
			poleFlipTrick.Refresh(ref changed);
		}

		public void LandCombo()
		{
			if (IsComboing())
			{
				lastScore = baseScore * scoreMultiplier;
				HitScore();
				Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().SetHighestCombo(lastScore);
				Core.Instance.SaveManager.SaveCurrentSaveSlot();
				lastCornered = corneredNodes.Count;
				if (WorldHandler.instance.currentEncounter == null)
				{
					ClearMultipliersDone();
				}
				showLastScoreTimer = showLastScoreDuration;
				showLastScoreAnimationTimer = showLastScoreAnimationDuration;
				comboTimeOutTimer = 1f;
				currentTrickType = TrickType.NONE;
				tricksInLastCombo = tricksInCombo;
				tricksInCombo = 0;
				RefreshAllDegrade();
				ui?.SetTrickingChargeBarActive(setActive: false);
				WorldHandler.instance.AllowRedoGrafspots();
			}
		}

		public void DropCombo()
		{
			if (IsComboing())
			{
				HitScore();
				lastScore = 0f;
				lastCornered = 0;
				ClearMultipliersDone();
				comboTimeOutTimer = 1f;
				currentTrickType = TrickType.NONE;
				RefreshAirTricks();
				tricksInCombo = 0;
				RefreshAllDegrade();
				ui?.SetTrickingChargeBarActive(setActive: false);
				WorldHandler.instance.AllowRedoGrafspots();
			}
		}

		public void ClearMultipliersDone()
		{
			corneredNodes.Clear();
			comboColliders.Clear();
		}

		public void HitScore()
		{
			score += baseScore * scoreMultiplier;
			Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().SetHighestCombo(lastScore);
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
			baseScore = 0f;
			scoreMultiplier = 0f;
		}

		public void HardCornerGrindLine(GrindNode node)
		{
			if (corneredNodes.Contains(node))
			{
				DoTrick(TrickType.HARD_CORNER, "Hard Corner");
				return;
			}
			DoTrick(TrickType.NEW_HARD_CORNER, "Hard Corner (New!)");
			corneredNodes.Add(node);
		}

		public void HitHighfive()
		{
		}

		public bool UseColliderInCombo(Collider collider)
		{
			if (comboColliders.Contains(collider))
			{
				return false;
			}
			AddScoreMultiplier();
			comboColliders.Add(collider);
			return true;
		}

		public void AddScoreMultiplier()
		{
			PlayVoice(AudioClipID.VoiceCombo, VoicePriority.COMBO);
			scoreMultiplier++;
		}

		public void DoComboTimeOut(float countDown)
		{
			comboTimeOutTimer -= countDown;
		}

		public void RefillComboTimeOut()
		{
			comboTimeOutTimer = 1f;
		}

		public void ResetComboTimeOut()
		{
			if (comboTimeOutTimer < 1f)
			{
				comboTimeOutShowResetTimer = 0.35f;
			}
			comboTimeOutTimer = 1f;
		}

		public void UpdateScore()
		{
			if (comboTimeOutTimer <= 0f && IsComboing() && IsGrounded())
			{
				LandCombo();
				slideAbility.ComboTimeout();
			}
		}

		public bool IsComboing()
		{
			return baseScore > 0f;
		}

		public bool AnyTrickInput()
		{
			if (!trick1ButtonNew && !trick2ButtonNew)
			{
				return trick3ButtonNew;
			}
			return true;
		}

		public int InputToTrickNumber()
		{
			if (isAI)
			{
				return UnityEngine.Random.Range(0, 3);
			}
			if (trick1ButtonNew)
			{
				return 0;
			}
			if (trick2ButtonNew)
			{
				return 1;
			}
			if (trick3ButtonNew)
			{
				return 2;
			}
			return 0;
		}

		private bool CheckVert()
		{
			bool flag = OnAnyGround();
			bool flag2 = flag && motor.groundCollider.gameObject.layer == 23;
			if (!flag && this.vertShape != null && vertPos.x > 0f && vertPos.x < this.vertShape.size.x && vertPos.y > 0f && vertPos.y < this.vertShape.size.y)
			{
				flag2 = true;
			}
			if (flag2 && !usingEquippedMovestyle)
			{
				VertShape vertShape = ((!flag) ? null : motor.groundCollider.gameObject.GetComponent<VertShape>());
				if (vertShape != null && vertShape.GetArcY(base.transform.position.y) > vertSteepIfAbove)
				{
					SwitchToEquippedMovestyle(set: true, doAirTrick: true);
				}
				else
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				if (flag)
				{
					VertShape component = motor.groundCollider.gameObject.GetComponent<VertShape>();
					_ = component == null;
					if (this.vertShape != component)
					{
						VertEnter(component, this.vertShape);
					}
				}
				VertMove();
				return true;
			}
			if (this.vertShape != null)
			{
				VertExit();
			}
			return false;
		}

		private void VertEnter(VertShape shape, VertShape prev)
		{
			vertShape = shape;
			vertShape.Init();
			if (ability == vertAbility)
			{
				StopCurrentAbility();
				audioManager.PlaySfxGameplay(moveStyle, AudioClipID.land, playerOneShotAudioSource);
				OrientVisualInstant();
				PlayAnim(landRunHash);
				skipLanding = true;
			}
			vertGoJump = false;
			vertPos = vertShape.WorldToVertPos(tf.position);
			UpdateVertNormal();
			Vector3 vector = Vector3.ProjectOnPlane(motor.velocity, vertNormal);
			Quaternion quaternion = Quaternion.FromToRotation(vertNormal, Vector3.up);
			Vector3 vector2 = vertShape.rotLocal * (quaternion * vector);
			vertDir = new Vector2(vector2.x, vector2.z).normalized;
			if (prev == null)
			{
				vertSpeed = vector.magnitude;
			}
		}

		private void VertMove()
		{
			UpdateVertNormal();
			Vector3 vector = vertShape.rotLocal * moveInput;
			Vector2 b = new Vector2(vector.x, vector.z);
			if (vertGoJump)
			{
				b = new Vector2(0f, 1f);
			}
			bool flag = b.sqrMagnitude > 0.0001f;
			if (flag)
			{
				vertDir = Vector2.Lerp(vertDir, b, stats.rotSpeedAtMaxSpeed * 0.7f * Core.dt);
			}
			float num = vertSpeed;
			if (flag)
			{
				num = stats.runSpeed;
			}
			else
			{
				bool flag2 = false;
				if (vertDir.y < 0f)
				{
					flag2 = true;
				}
				else if (vertPosYRel > vertSteepIfAbove)
				{
					flag2 = true;
				}
				else
				{
					num = 0f;
				}
				if (flag2)
				{
					Vector2 vector2 = new Vector2(vertDir.x * vertSpeed, vertDir.y * vertSpeed - vertRollDownForce * Core.dt);
					num = (vertSpeed = vector2.magnitude);
					vertDir = vector2 / vertSpeed;
				}
			}
			if (vertSpeed > num)
			{
				vertSpeed = Mathf.Max(num, vertSpeed - stats.groundDecc * Core.dt);
			}
			else if (vertSpeed < num)
			{
				vertSpeed = Mathf.Min(num, vertSpeed + stats.groundAcc * Core.dt);
			}
			if (vertSpeed > vertMaxSpeed)
			{
				vertSpeed = vertMaxSpeed;
			}
			targetMovement = ((vertSpeed < stats.walkSpeed && !flag) ? MovementType.WALKING : MovementType.RUNNING);
			Vector3 vector3 = new Vector3(vertDir.x, 0f, vertDir.y);
			vector3 = Quaternion.FromToRotation(Vector3.up, vertNormal) * (vertShape.rotWorld * vector3);
			motor.Rotate(vector3, 100000f);
			vertPos += vertDir * vertSpeed * Core.dt;
			motor.velocity = (vertShape.VertPosToWorld(vertPos) - tf.position) / Core.dt;
			if (jumpRequested)
			{
				if (vertPosYRel < vertSteepIfAbove)
				{
					vertShape = null;
					Jump();
				}
				else
				{
					vertGoJump = true;
				}
			}
		}

		private void VertExit()
		{
			_ = vertPos.y / vertShape.size.y;
			Vector3 normalized = reallyMovedVelocity.normalized;
			float num = vertSpeed;
			if (vertPosYRel < 0.2f)
			{
				normalized.y = 0f;
				normalized.Normalize();
				if (num > vertBottomExitSpeedThreshold)
				{
					num = vertBottomExitSpeed;
				}
				skipLanding = true;
			}
			motor.velocity = normalized * num;
			if (vertDir.y > 0f && vertPosYRel > 0.9f)
			{
				vertNormal.y = 0f;
				vertNormal.Normalize();
				UseColliderInCombo(vertShape.GetComponent<Collider>());
				vertAbility.Trigger(tf.position, vertNormal);
			}
			vertShape = null;
		}

		private void UpdateVertNormal()
		{
			vertPosYRel = vertPos.y / vertShape.size.y;
			vertNormal = vertShape.rotWorld * new Vector3(0f, Mathf.Cos(vertPosYRel * MathF.PI * 0.5f), 0f - Mathf.Sin(vertPosYRel * MathF.PI * 0.5f));
		}

		public void InitVisual()
		{
			InitHitboxes();
			if (!isAI)
			{
				InitCuffs();
			}
		}

		public void SetCharacter(Characters setChar, int setOutfit = 0)
		{
			if (setChar != character)
			{
				if (!isAI)
				{
					WorldHandler.instance.SetNPCAvailabilityBasedOnCharacter(character, setChar);
				}
				character = setChar;
				charTestCycleNum = (int)setChar;
				if (visualTf != null)
				{
					UnityEngine.Object.Destroy(visualTf.gameObject);
				}
				characterVisual = characterConstructor.CreateNewCharacterVisual(character, animatorController, !isAI, motor.groundDetection.groundLimit);
				characterMesh = characterVisual.mainRenderer.sharedMesh;
				if (setOutfit > 0)
				{
					SetOutfit(setOutfit);
				}
				characterVisual.transform.SetParent(base.transform.GetChild(0), worldPositionStays: false);
				characterVisual.transform.localPosition = Vector3.zero;
				characterVisual.transform.rotation = Quaternion.LookRotation(base.transform.forward);
				characterVisual.anim.gameObject.AddComponent<AnimationEventRelay>().Init();
				visualTf = characterVisual.transform;
				headTf = visualTf.FindRecursive("head");
				phoneDirBone = visualTf.FindRecursive("phoneDirection");
				heightToHead = (headTf.position - visualTf.position).y;
				isGirl = characterVisual.isGirl;
				anim = characterVisual.anim;
				if (curAnim != 0)
				{
					int newAnim = curAnim;
					curAnim = 0;
					PlayAnim(newAnim);
				}
				characterVisual.InitVFX(VFXPrefabs);
				characterVisual.InitMoveStyleProps(MoveStylePropsPrefabs);
				characterConstructor.SetMoveStyleSkinsForCharacter(this, setChar);
				if (characterVisual.hasEffects)
				{
					boostpackTrail = characterVisual.VFX.boostpackTrail.GetComponent<TrailRenderer>();
					boostpackTrailDefaultWidth = boostpackTrail.startWidth;
					boostpackTrailDefaultTime = boostpackTrail.time;
					spraypaintParticles = characterVisual.VFX.spraypaint.GetComponent<ParticleSystem>();
					characterVisual.VFX.spraypaint.transform.localScale = Vector3.one * 0.5f;
					SetDustEmission(0);
					ringParticles = characterVisual.VFX.ring.GetComponent<ParticleSystem>();
					SetRingEmission(0);
				}
				SetMoveStyle(MoveStyle.ON_FOOT);
				SaveSelectedCharacter(setChar);
			}
		}

		private void SaveSelectedCharacter(Characters selectedCharacter)
		{
			if (!isAI)
			{
				Core.Instance.SaveManager.CurrentSaveSlot.currentCharacter = selectedCharacter;
				Core.Instance.SaveManager.SaveCurrentSaveSlot();
			}
		}

		public void SetOutfit(int setOutfit)
		{
			if (!isAI)
			{
				Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).outfit = setOutfit;
			}
			characterVisual.GetComponentInChildren<SkinnedMeshRenderer>().material = characterConstructor.CreateCharacterMaterial(character, setOutfit);
		}

		public bool HasSpecialSkateboard()
		{
			return characterVisual.moveStyleProps.specialSkateBoard;
		}

		public void CreateCircleBoostEffect(Vector3 dir, Vector3 offset = default(Vector3))
		{
			_ = UnityEngine.Object.Instantiate(VFXPrefabs.circleBoostEffect, capsuleQuarter + offset + dir * 1.2f, Quaternion.LookRotation(dir)).transform;
		}

		public Transform CreateCircleDustEffect(Vector3 dir)
		{
			return UnityEngine.Object.Instantiate(VFXPrefabs.jumpEffect, tf.position, Quaternion.LookRotation(dir)).transform;
		}

		public void CreateHighJumpDustEffect(Vector3 dir)
		{
			_ = UnityEngine.Object.Instantiate(VFXPrefabs.highJumpEffect, tf.position, Quaternion.LookRotation(dir)).transform;
		}

		public void CreateCapPopEffect()
		{
			if (newSpraycan)
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.sprayCanPop, playerAccessoryAudioSource);
				Transform transform = characterVisual.VFX.spraycan.transform;
				Rigidbody rigidbody = WorldHandler.instance.SpawnJunk(DynamicJunk.SprayCap, transform.position, Quaternion.identity);
				rigidbody.transform.localScale = transform.lossyScale;
				rigidbody.velocity = GetPracticalVelocity();
				float num = (rigidbody.maxAngularVelocity = 40f);
				rigidbody.AddTorque(UnityEngine.Random.rotation * Vector3.forward * num, ForceMode.Impulse);
				rigidbody.AddForce(Vector3.up * 4f + tf.right * 1f, ForceMode.Impulse);
				newSpraycan = false;
			}
		}

		public void CreateSpraycanJunkEffect()
		{
			Transform transform = characterVisual.VFX.spraycan.transform;
			Rigidbody rigidbody = WorldHandler.instance.SpawnJunk(DynamicJunk.SprayingCan, transform.position, Quaternion.identity);
			rigidbody.transform.localScale = transform.lossyScale;
			rigidbody.velocity = GetPracticalVelocity();
			float num = (rigidbody.maxAngularVelocity = 20f);
			rigidbody.AddTorque(UnityEngine.Random.rotation * Vector3.forward * num, ForceMode.Impulse);
			rigidbody.AddForce(tf.forward * -1f + tf.right * 1.5f + Vector3.up * -1f, ForceMode.Impulse);
		}

		public void RemoveGraffitiSlash()
		{
			if ((bool)characterVisual.VFX.graffitiSlash)
			{
				UnityEngine.Object.Destroy(characterVisual.VFX.graffitiSlash);
			}
			if ((bool)characterVisual.VFX.graffitiFinishEffect)
			{
				UnityEngine.Object.Destroy(characterVisual.VFX.graffitiFinishEffect);
			}
		}

		public GameObject CreateGraffitiSlashEffect(Transform parent, Vector3 direction)
		{
			characterVisual.VFX.graffitiSlash = UnityEngine.Object.Instantiate(VFXPrefabs.graffitiSlash, parent.position + parent.forward + parent.up, Quaternion.LookRotation(parent.forward * -1f, direction));
			characterVisual.VFX.graffitiSlash.transform.parent = parent;
			return characterVisual.VFX.graffitiSlash;
		}

		public GameObject CreateGraffitiFinishEffect(Transform parent, GraffitiSize size)
		{
			if (size == GraffitiSize.S || size == GraffitiSize.M)
			{
				characterVisual.VFX.graffitiFinishEffect = UnityEngine.Object.Instantiate(VFXPrefabs.graffitiFinishEffect, parent.position, Quaternion.LookRotation(parent.forward));
			}
			else
			{
				characterVisual.VFX.graffitiFinishEffect = UnityEngine.Object.Instantiate(VFXPrefabs.graffitiFinishLargeEffect, parent.position, Quaternion.LookRotation(parent.forward));
			}
			characterVisual.VFX.graffitiFinishEffect.transform.parent = parent;
			return characterVisual.VFX.graffitiFinishEffect;
		}

		public void UpdateBoostpack()
		{
			if (!characterVisual.hasEffects)
			{
				return;
			}
			if ((ability == null && !boosting) || (ability != null && ability.keepJumping && !boosting))
			{
				if (isJumping)
				{
					if (timeSinceLastJump < 0.15f)
					{
						SetDustEmission(100);
						SetBoostpackAndFrictionEffects(BoostpackEffectMode.ON);
					}
					else
					{
						SetDustEmission(0);
						SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF);
					}
				}
				else
				{
					SetDustEmission(0);
					SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF);
				}
			}
			if (ability == specialAirAbility)
			{
				if (isJumping)
				{
					if (timeSinceLastJump < 0.3f)
					{
						SetBoostpackAndFrictionEffects(BoostpackEffectMode.ON);
					}
					else
					{
						SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF);
					}
				}
				else
				{
					SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF);
				}
			}
			UpdateTrails();
		}

		public void UpdateTrails()
		{
			if (characterVisual.VFX.boostpackEffect.activeSelf)
			{
				if (!preBoostpackActive)
				{
					TrailRenderer trailRenderer = boostpackTrail;
					float time = (boostpackTrail.startWidth = 0f);
					trailRenderer.time = time;
				}
				preBoostpackActive = true;
				characterVisual.VFX.boostpackTrail.SetActive(value: true);
				boostTrailTimer += Core.dt * 1.2f;
				if (boostTrailTimer > boostpackTrailDefaultTime)
				{
					boostTrailTimer = boostpackTrailDefaultTime;
				}
				trailWidth += Core.dt;
				if (trailWidth > boostpackTrailDefaultWidth)
				{
					trailWidth = boostpackTrailDefaultWidth;
				}
			}
			else
			{
				preBoostpackActive = false;
				boostTrailTimer -= Core.dt * 6f;
				if (boostTrailTimer < 0f)
				{
					boostTrailTimer = 0f;
					characterVisual.VFX.boostpackTrail.SetActive(value: false);
				}
				trailWidth -= Core.dt;
				if (trailWidth < 0f)
				{
					trailWidth = 0f;
				}
			}
			boostpackTrail.time = boostTrailTimer;
			boostpackTrail.startWidth = trailWidth;
		}

		public void SetBoostpackAndFrictionEffects(BoostpackEffectMode boostpack, FrictionEffectMode friction = FrictionEffectMode.OFF)
		{
			characterVisual.SetBoostpackEffect(boostpack);
			characterVisual.SetFrictionEffect(friction);
		}

		public void SetDustEmission(int rate)
		{
			characterVisual.SetDustEmission(rate);
		}

		public void SetSpraypaintEmission(int rate)
		{
			ParticleSystem.EmissionModule emission = spraypaintParticles.emission;
			emission.rateOverTime = rate;
		}

		public void SetRingEmission(int rate)
		{
			ParticleSystem.EmissionModule emission = ringParticles.emission;
			emission.rateOverTime = rate;
		}

		public void SetDustSize(float size)
		{
			characterVisual.VFX.dust.transform.localScale = Vector3.one * size;
		}

		public void StartScreenShake(ScreenShakeType size, float duration, bool continuous = false)
		{
			if (!isAI)
			{
				GameplayCamera.StartScreenShake(size, duration, continuous);
			}
		}

		public void DoHitEffect(Transform target)
		{
			float num = 1.2f;
			if ((ability == slideAbility && moveStyle != MoveStyle.BMX) || moveStyle == MoveStyle.SKATEBOARD)
			{
				num = 0.75f;
			}
			GameObject obj = UnityEngine.Object.Instantiate(VFXPrefabs.hitEffect, base.transform.position + (target.position - base.transform.position).normalized * 1f + Vector3.up * num, Quaternion.identity);
			obj.transform.rotation = Quaternion.LookRotation(-WorldHandler.instance.CurrentCameraTransform.forward, -WorldHandler.instance.CurrentCameraTransform.up);
			obj.transform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360)), Space.Self);
		}

		public void DoHitEffectPosition(Vector3 position)
		{
			GameObject obj = UnityEngine.Object.Instantiate(VFXPrefabs.hitEffect, position, Quaternion.identity);
			obj.transform.rotation = Quaternion.LookRotation(-WorldHandler.instance.CurrentCameraTransform.forward, -WorldHandler.instance.CurrentCameraTransform.up);
			obj.transform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360)), Space.Self);
		}

		public void DoGetHitEffect(Transform target)
		{
			float num = 1f;
			GameObject obj = UnityEngine.Object.Instantiate(VFXPrefabs.getHitEffect, base.transform.position + (target.position - base.transform.position).normalized * 0.5f + Vector3.up * num, Quaternion.identity);
			obj.transform.rotation = Quaternion.LookRotation(-WorldHandler.instance.CurrentCameraTransform.forward, -WorldHandler.instance.CurrentCameraTransform.up);
			obj.transform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360)), Space.Self);
		}
	}
}
