using System.Collections.Generic;
using DG.Tweening;
using Kaj.SimpleEasing;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Reptile
{
	public class GraffitiGame : MonoBehaviour
	{
		public enum GraffitiGameState
		{
			MAIN_STATE = 0,
			COMPLETE_TARGETS = 1,
			SHOW_PIECE = 2,
			FINISHED = 3,
			SHOW_FAIL = 4,
			CLOSED = 5
		}

		public enum HitSpeed
		{
			NORMAL = 0,
			QUICK = 1
		}

		private class Target
		{
			public enum TargetState
			{
				OPEN = 0,
				ACTIVE = 1,
				CLEARED = 2,
				LOCKED = 3
			}

			private GraffitiGame grafGame;

			private int index;

			public TargetState state;

			public Transform Transform { get; private set; }

			public SpriteRenderer TargetSprite { get; private set; }

			public SpriteRenderer backgroundSprite { get; private set; }

			public Transform TargetTransform { get; private set; }

			public Transform Ring { get; private set; }

			public Target(Transform transform, SpriteRenderer setTargetSprite, SpriteRenderer setBackgroundSprite, Transform ring, GraffitiGame setGame, int index)
			{
				grafGame = setGame;
				Transform = transform;
				TargetSprite = setTargetSprite;
				TargetTransform = setTargetSprite.transform;
				backgroundSprite = setBackgroundSprite;
				Ring = ring;
				this.index = index;
				SetState(TargetState.OPEN);
			}

			public void SetState(TargetState setState)
			{
				state = setState;
				TargetTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
				Ring.localEulerAngles = new Vector3(0f, 0f, 0f);
				switch (setState)
				{
				case TargetState.OPEN:
					Ring.gameObject.SetActive(value: false);
					TargetSprite.gameObject.SetActive(value: true);
					TargetSprite.sprite = grafGame.targetOpenSprite;
					if (index == 0)
					{
						TargetSprite.sprite = grafGame.targetOpenCenterSprite;
					}
					break;
				case TargetState.ACTIVE:
					Ring.gameObject.SetActive(value: true);
					TargetSprite.gameObject.SetActive(value: true);
					TargetSprite.sprite = grafGame.targetActiveSprite;
					backgroundSprite.sprite = grafGame.targetBackgroundCleared;
					TargetTransform.localScale = Vector3.one;
					break;
				case TargetState.CLEARED:
					Ring.gameObject.SetActive(value: false);
					TargetSprite.gameObject.SetActive(value: false);
					break;
				case TargetState.LOCKED:
					TargetSprite.gameObject.SetActive(value: false);
					backgroundSprite.sprite = grafGame.targetLockedSprite;
					break;
				}
			}
		}

		public enum GrafCamState
		{
			STARTZOOM = 0,
			FAST = 1,
			HITLAG = 2,
			SLOW = 3
		}

		private class VisualProperties
		{
			public float shakePosMagnitude = float.MaxValue;

			public float shakeRotMagnitude = float.MaxValue;

			public float shakePosSpeed = float.MaxValue;

			public float shakeRotSpeed = float.MaxValue;

			public SimpleEasing.EaseType shakeDecayEase = SimpleEasing.EaseType.EaseLinear;
		}

		private List<GraffitiArt> unlockedGraffiti = new List<GraffitiArt>();

		private int availableTargets;

		private EffectsUI effectsUI;

		private Material m_LineMaterial;

		private Vector3 lastAim;

		private GraffitiGameState state;

		private float stateTimer;

		private Transform UICameraTf;

		private GraffitiSpot gSpot;

		private Vector3 aim = Vector3.zero;

		private float shootTargetTimer;

		private bool reloadedSpray;

		private float sprayWait;

		private float sprayWaitDuration = 0.14f;

		private Vector3 slashCardinal;

		private Player player;

		private GraffitiArtInfo graffitiArtInfo;

		private Target lastActiveTarget;

		private Target activeTarget;

		private Target[] targets;

		public GameObject LinePrefab;

		private LineRenderer line;

		public Sprite targetOpenSprite;

		public Sprite targetActiveSprite;

		public Sprite targetLockedSprite;

		public Sprite targetOpenCenterSprite;

		public Sprite targetBackgroundCleared;

		public GameObject backgroundCircle;

		private List<int> targetsHitSequence = new List<int>();

		private float distanceToGrafSpot = 2.65f;

		private float timeRemaining;

		private float showPieceCamSpeed;

		private Vector3 originalTargetScale = Vector3.one;

		private AudioManager audioManager;

		private GameInput gameInput;

		private BaseModule baseModule;

		private Vector2 mouseInput;

		private bool mouseWasUsed;

		public float TimePerTarget = 1.5f;

		private const float MouseInputSqrDeadzone = 0.05f;

		private const float QuickThreshold = 0.07f;

		private float previousHitTime;

		private Target previousHitTarget;

		private CharacterVisual characterPuppet;

		private float targetScaleAnimTime;

		private int posShakeFramer;

		private Vector3 lastPosShake;

		private readonly float startZoomDistance = 0.2f;

		private readonly float fastCamDuration = 0.1f;

		private readonly float hitlagCamDuration = 0.18f;

		private readonly float flashDuration = 0.03f;

		private readonly float flashFade = 0.03f;

		private readonly float showPieceDuration_S_M = 1.25f;

		private readonly float showPieceDuration_L_XL = 1.45f;

		private GraffitiArt grafArt;

		private int graffitiCamDefaultCulling;

		private Vector3 lastCamPos;

		private Vector3 camTargetPos;

		private Transform graffitiCameraTf;

		private Transform closeUpCameraTf;

		private Transform closeUpCameraRotatorTf;

		private GraffitiEffect graffitiSlash;

		private float effectSpeed;

		private float effectStun;

		private float camTimer;

		private Side slashSide;

		private HitSpeed currentHitSpeed;

		private Dictionary<HitSpeed, VisualProperties> visualPropertiesDict;

		private Vector3 lastSlashDir;

		private Vector3 screenShakeOriginPos;

		private Vector3 screenShakeLocalOriginRot;

		[SerializeField]
		protected AnimationCurve targetScale;

		private string graffitiNewText = string.Empty;

		private IGameTextLocalizer localizer;

		private const string GRAFFITI_NEW_LOCALIZATION_KEY = "GAMEPLAY_NEW";

		private const string GRAFFITI_DISPLAY_FORMAT = "'{0}'";

		private GrafCamState grafCamState;

		public void Init(Player p, GraffitiSpot g)
		{
			gameInput = Core.Instance.GameInput;
			baseModule = Core.Instance.BaseModule;
			effectsUI = Core.Instance.UIManager.effects;
			player = p;
			gSpot = g;
			graffitiArtInfo = g.GraffitiArtInfo;
			audioManager = p.AudioManager;
			gSpot.StartGraffitiMode(player);
			UICameraTf = base.transform.Find("UICamera");
			sprayWait = sprayWaitDuration;
			reloadedSpray = true;
			backgroundCircle.SetActive(value: true);
			Transform child = base.transform.Find("Targets").GetChild((int)gSpot.size);
			child.gameObject.SetActive(value: true);
			targets = new Target[child.childCount];
			for (int i = 0; i < child.childCount; i++)
			{
				Transform child2 = child.GetChild(i);
				targets[i] = new Target(child2, child2.FindRecursive("TargetSprite").GetComponent<SpriteRenderer>(), child2.FindRecursive("BackgroundSprite").GetComponent<SpriteRenderer>(), child2.FindRecursive("Ring"), this, i);
				child2.GetChild(0).position = UICameraTf.position;
			}
			originalTargetScale = targets[0].TargetTransform.localScale;
			if (gSpot.size != GraffitiSize.S)
			{
				List<GraffitiArt> list = graffitiArtInfo.FindBySize(gSpot.size);
				AUser user = Core.Instance.Platform.User;
				for (int j = 0; j < list.Count; j++)
				{
					GraffitiArt graffitiArt = list[j];
					if (user.GetUnlockableSaveDataFor(graffitiArt.unlockable).IsUnlocked)
					{
						unlockedGraffiti.Add(graffitiArt);
					}
				}
			}
			availableTargets = graffitiArtInfo.GetAvailableTargets(unlockedGraffiti, targetsHitSequence, gSpot.size);
			SetActiveTargetToCenter();
			previousHitTarget = activeTarget;
			timeRemaining = (float)targets.Length * TimePerTarget;
			previousHitTime = timeRemaining;
			InitVisual();
			SetState(GraffitiGameState.MAIN_STATE);
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(4);
			audioManager.PauseAllGameplayLoopingSfx();
		}

		private void SetState(GraffitiGameState setState)
		{
			state = setState;
			stateTimer = 0f;
			switch (state)
			{
			case GraffitiGameState.MAIN_STATE:
				mouseWasUsed = false;
				mouseInput = Vector3.zero;
				lastAim = Vector3.zero;
				break;
			case GraffitiGameState.COMPLETE_TARGETS:
				UICameraTf.gameObject.SetActive(value: false);
				player.ui.graffitiTimerBar.fillAmount = 0f;
				player.ui.graffitiTimerBackdrop.gameObject.SetActive(value: false);
				backgroundCircle.SetActive(value: false);
				break;
			case GraffitiGameState.SHOW_PIECE:
			{
				if (gSpot.size == GraffitiSize.S)
				{
					grafArt = graffitiArtInfo.FindByCharacter(player.character);
					line.gameObject.SetActive(value: false);
					Target[] array = targets;
					foreach (Target target in array)
					{
						if (target != null)
						{
							Object.Destroy(target.Transform.gameObject);
						}
					}
				}
				else
				{
					targetsHitSequence.RemoveAt(targetsHitSequence.Count - 1);
					grafArt = graffitiArtInfo.FindBySequence(targetsHitSequence);
				}
				Player.TrickType type = Player.TrickType.GRAFFITI_S;
				if (gSpot.size == GraffitiSize.M)
				{
					type = Player.TrickType.GRAFFITI_M;
				}
				if (gSpot.size == GraffitiSize.L)
				{
					type = Player.TrickType.GRAFFITI_L;
				}
				if (gSpot.size == GraffitiSize.XL)
				{
					type = Player.TrickType.GRAFFITI_XL;
				}
				player.DoTrick(type, grafArt.title);
				bool oldBottomWasClaimedByPlayableCrew = gSpot.bottomCrew == Crew.PLAYERS || gSpot.bottomCrew == Crew.ROGUE;
				gSpot.Paint(Crew.PLAYERS, grafArt);
				gSpot.GiveRep(noRepPickup: false, oldBottomWasClaimedByPlayableCrew);
				break;
			}
			case GraffitiGameState.FINISHED:
				gSpot.SetLastPlayer(player);
				gSpot.SetState(GraffitiState.FINISHED);
				gSpot.SpawnRep();
				End();
				break;
			case GraffitiGameState.CLOSED:
				End();
				break;
			}
			SetStateVisual(state);
		}

		private void Update()
		{
			if (baseModule.IsInGamePaused)
			{
				return;
			}
			stateTimer += Core.dt;
			switch (state)
			{
			case GraffitiGameState.MAIN_STATE:
			{
				gSpot.ManualAnimateExtraSlotAIPlayers();
				Vector3 vector2 = new Vector3(gameInput.GetAxis(5), gameInput.GetAxis(6), 0f);
				Vector3 vector3 = new Vector3(gameInput.GetAxis(13), gameInput.GetAxis(14), 0f);
				if (gameInput.IsInputSourceForAction(ControllerType.Mouse, 13) || gameInput.IsInputSourceForAction(ControllerType.Mouse, 14))
				{
					mouseWasUsed = true;
				}
				else if (vector2 != Vector3.zero && vector3 != Vector3.zero)
				{
					mouseWasUsed = false;
				}
				if (mouseWasUsed)
				{
					mouseInput += (Vector2)vector3 * 0.017f;
					if (mouseInput.magnitude > 1f)
					{
						mouseInput.Normalize();
					}
				}
				aim = vector2;
				if (mouseWasUsed)
				{
					aim = mouseInput;
				}
				else
				{
					aim += vector3;
				}
				if (aim.magnitude > 1f)
				{
					aim.Normalize();
				}
				sprayWait -= Core.dt;
				TargetingUpdate();
				UpdateMainStateVisual();
				if (AllTargetsDone() && ((grafCamState == GrafCamState.SLOW && camTimer > 0.2f) || gSpot.size == GraffitiSize.S))
				{
					SetState(GraffitiGameState.COMPLETE_TARGETS);
				}
				else if (gameInput.GetButtonNew(54))
				{
					CancelGame();
				}
				break;
			}
			case GraffitiGameState.COMPLETE_TARGETS:
			{
				Target[] array = targets;
				foreach (Target target in array)
				{
					if (target != null)
					{
						target.Transform.localScale -= Vector3.one * 5f * Core.dt;
						if (target.Transform.localScale.x < 0f)
						{
							target.Transform.localScale = Vector3.zero;
						}
					}
				}
				line.startWidth -= Core.dt;
				line.endWidth -= Core.dt;
				Color color = m_LineMaterial.color;
				color.a -= Core.dt * 3f;
				m_LineMaterial.color = color;
				Camera component = UICameraTf.GetComponent<Camera>();
				component.orthographicSize = Mathf.Lerp(component.orthographicSize, 3f, 0.15f);
				gSpot.ReleaseExtraSlotPlayers();
				if (stateTimer > 0.6f)
				{
					SetState(GraffitiGameState.SHOW_PIECE);
				}
				break;
			}
			case GraffitiGameState.SHOW_PIECE:
			{
				Vector3 vector = gSpot.transform.forward * Core.dt;
				if (stateTimer >= 0.2f)
				{
					vector *= showPieceCamSpeed;
					float num = 75f;
					if (gSpot.size == GraffitiSize.S || gSpot.size == GraffitiSize.M)
					{
						num = 150f;
					}
					showPieceCamSpeed = Mathf.Max(showPieceCamSpeed - Core.dt * num, 0.35f);
				}
				graffitiCameraTf.position += vector;
				screenShakeOriginPos += vector;
				graffitiSlash.SetSpeed(Mathf.Max(graffitiSlash.GetSpeed() - Core.dt * 60f, 0f));
				currentHitSpeed = HitSpeed.NORMAL;
				ScreenShakeGraffitiCam(stateTimer, 0.4f, new Vector3(0.5f, -0.5f, 0f));
				characterPuppet.gameObject.SetActive(value: false);
				float num2 = showPieceDuration_S_M;
				if (gSpot.size == GraffitiSize.L || gSpot.size == GraffitiSize.XL)
				{
					num2 = showPieceDuration_L_XL;
				}
				if (stateTimer > num2 - 0.2f && !effectsUI.IsShowingFade)
				{
					effectsUI.FadeInAndOutBlack(0.2f, 0.1f, 0.2f, UpdateType.Normal);
				}
				if (stateTimer > num2)
				{
					SetState(GraffitiGameState.FINISHED);
				}
				break;
			}
			}
		}

		private void TargetingUpdate()
		{
			if (lastAim == Vector3.zero || Vector3.Dot(aim.normalized, lastAim) < 0.97f)
			{
				reloadedSpray = true;
			}
			if (sprayWait > 0f || !reloadedSpray || AllTargetsDone())
			{
				aim = Vector3.zero;
			}
			Vector3 position = activeTarget.Transform.position + aim * 4f;
			if (aim.magnitude > 0.6f)
			{
				shootTargetTimer += Core.dt;
			}
			if (shootTargetTimer > 1.5f)
			{
				reloadedSpray = true;
			}
			if (reloadedSpray && shootTargetTimer > 0.04f)
			{
				int num = CheckTargetHit(activeTarget.Transform.position, aim * 1.5f);
				if (num != -1)
				{
					Vector3 normalized = (targets[num].Transform.position - activeTarget.Transform.position).normalized;
					HitTarget(targets[num], normalized);
					reloadedSpray = false;
					previousHitTarget = targets[num];
					position = previousHitTarget.Transform.position;
				}
			}
			lastAim = aim.normalized;
			line.SetPosition(1, position);
			if (!AllTargetsDone())
			{
				timeRemaining -= Core.dt * 0.2f;
				if (timeRemaining < 0f)
				{
					CancelGame();
				}
			}
		}

		private int CheckTargetHit(Vector3 from, Vector3 aimedSprayMove)
		{
			int num = -1;
			float num2 = -2f;
			for (int i = ((!OneTargetLeft()) ? 1 : 0); i < targets.Length; i++)
			{
				float num3 = Vector3.Dot(aimedSprayMove, (targets[i].Transform.position - from).normalized);
				if (num3 > num2 && num3 > 0.4f)
				{
					num2 = num3;
					num = i;
				}
			}
			if (num != -1 && (targetsHitSequence.Contains(num) || activeTarget == targets[num] || GraffitiArtInfo.TargetIsNotAllowed(availableTargets, num)))
			{
				num = -1;
			}
			return num;
		}

		private void HitTarget(Target target, Vector3 slashDirectionUI)
		{
			lastActiveTarget = activeTarget;
			activeTarget = target;
			targetsHitSequence.Add(activeTarget.Transform.GetSiblingIndex());
			line.SetPosition(1, activeTarget.Transform.position);
			line = Object.Instantiate(line.gameObject, base.transform).GetComponent<LineRenderer>();
			line.SetPosition(0, activeTarget.Transform.position);
			sprayWait = sprayWaitDuration;
			shootTargetTimer = 0f;
			mouseWasUsed = false;
			mouseInput = Vector3.zero;
			HitSpeed hitSpeed = ((previousHitTime - timeRemaining <= 0.07f) ? HitSpeed.QUICK : HitSpeed.NORMAL);
			previousHitTime = timeRemaining;
			HitTargetVisual(target, slashDirectionUI, hitSpeed);
		}

		public static Side GetClosestCardinalDirection(Vector3 direction, GraffitiSpot grafSpot, ref Vector3 sCardinal)
		{
			float num = -2f;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = grafSpot.transform.right * -1f;
			Vector3 up = grafSpot.transform.up;
			int result = -1;
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector3 = up;
				if (i == 1)
				{
					vector3 = (vector2 + vector2 + up).normalized;
				}
				if (i == 2)
				{
					vector3 = vector2;
				}
				if (i == 3)
				{
					vector3 = (vector2 + vector2 + up * -1f).normalized;
				}
				if (i == 4)
				{
					vector3 = up * -1f;
				}
				if (i == 5)
				{
					vector3 = (vector2 * -1f + vector2 * -1f + up * -1f).normalized;
				}
				if (i == 6)
				{
					vector3 = vector2 * -1f;
				}
				if (i == 7)
				{
					vector3 = (vector2 * -1f + vector2 * -1f + up).normalized;
				}
				float num2 = Vector3.Dot(direction, vector3);
				if (num2 > num)
				{
					result = i;
					num = num2;
					vector = vector3;
				}
			}
			sCardinal = vector;
			return (Side)result;
		}

		private void CancelGame()
		{
			gSpot.SetState(GraffitiState.CANCEL);
			SetState(GraffitiGameState.CLOSED);
			gSpot.ManualLetExtraSlotAIPlayersFinish();
		}

		private bool OneTargetLeft()
		{
			return targetsHitSequence.Count == targets.Length - 1;
		}

		private bool AllTargetsDone()
		{
			if (targetsHitSequence.Count != targets.Length)
			{
				return targets.Length <= 1;
			}
			return true;
		}

		private void SetActiveTargetToCenter()
		{
			Target target = targets[0];
			float num = float.MaxValue;
			Target[] array = targets;
			foreach (Target target2 in array)
			{
				if (target2.Transform.localPosition.magnitude < num)
				{
					num = target2.Transform.localPosition.magnitude;
					target = target2;
				}
			}
			activeTarget = target;
			activeTarget.SetState(Target.TargetState.ACTIVE);
			mouseWasUsed = false;
			mouseInput = Vector3.zero;
		}

		public void End()
		{
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMaps(BaseModule.IN_GAME_INPUT_MAPS);
			player.RemoveGraffitiSlash();
			player.EndGraffitiMode(gSpot);
			Object.Destroy(base.gameObject);
			if ((bool)characterPuppet)
			{
				Object.Destroy(characterPuppet.gameObject);
			}
			Object.Destroy(graffitiCameraTf.gameObject);
			audioManager.ResumeAllGameplayLoopingSfx();
		}

		private void Awake()
		{
			SetupGraffitiTextLocalization();
		}

		private void OnDestroy()
		{
			Object.Destroy(m_LineMaterial);
			unlockedGraffiti.Clear();
			ClearGraffitiTextLocalization();
		}

		public void InitVisual()
		{
			CreateProperties();
			line = Object.Instantiate(LinePrefab, base.transform).GetComponent<LineRenderer>();
			line.positionCount = 2;
			line.SetPosition(0, activeTarget.Transform.position);
			m_LineMaterial = line.material;
			Transform transform = gSpot.transform;
			graffitiCameraTf = base.transform.Find("GraffitiCamera");
			graffitiCameraTf.position = (camTargetPos = (lastCamPos = transform.position + transform.forward * (distanceToGrafSpot + startZoomDistance)));
			graffitiCameraTf.rotation = Quaternion.LookRotation(transform.forward * -1f);
			graffitiCameraTf.parent = transform;
			SetCameraTarget(graffitiCameraTf.position + graffitiCameraTf.forward * startZoomDistance);
			graffitiCamDefaultCulling = graffitiCameraTf.GetComponent<Camera>().cullingMask;
			closeUpCameraTf = base.transform.Find("CloseUpCamera");
			characterPuppet = Object.Instantiate(player.visualTf.gameObject).GetComponent<CharacterVisual>();
			characterPuppet.transform.position = gSpot.GetGraffitiSpotPlayerPos();
			characterPuppet.transform.rotation = Quaternion.LookRotation(transform.forward * -1f);
			characterPuppet.SetBoostpackEffect(BoostpackEffectMode.GRAFFITI);
			characterPuppet.SetFrictionEffect(FrictionEffectMode.OFF);
			characterPuppet.SetSpraycan(set: true);
			characterPuppet.SetMoveStyleVisualAnim(null, MoveStyle.ON_FOOT);
			characterPuppet.SetMoveStyleVisualProps(null, MoveStyle.ON_FOOT);
			characterPuppet.anim.SetLayerWeight(1, 0f);
			characterPuppet.anim.SetLayerWeight(2, 0f);
			characterPuppet.SetDustEmission(0);
			characterPuppet.transform.parent = transform;
			characterPuppet.name = "puppet";
			closeUpCameraRotatorTf = Object.Instantiate(new GameObject(), gSpot.transform.position + gSpot.transform.forward, Quaternion.LookRotation(gSpot.transform.forward), base.transform).transform;
			closeUpCameraTf.SetParent(closeUpCameraRotatorTf);
			RefreshLockedTargets();
			if (gSpot.size == GraffitiSize.S)
			{
				Play("grafSlashFinisher");
				return;
			}
			if (player.GetPracticalVelocity().magnitude <= player.minMoveSpeed)
			{
				Play("grafSlideF");
				return;
			}
			float num = Vector3.Dot(player.dir, gSpot.transform.right);
			if (num > 0.5f)
			{
				Play("grafSlideR");
			}
			else if (num < -0.5f)
			{
				Play("grafSlideL");
			}
			else
			{
				Play("grafSlideF");
			}
		}

		private void SetupGraffitiTextLocalization()
		{
			localizer = Core.Instance.Localizer;
			localizer.OnLanguageChanged += OnLanguageChanged;
			SetGraffitiTexts();
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			SetGraffitiTexts();
		}

		private void SetGraffitiTexts()
		{
			graffitiNewText = localizer.GetUserInterfaceText("GAMEPLAY_NEW");
		}

		private void ClearGraffitiTextLocalization()
		{
			localizer.OnLanguageChanged -= OnLanguageChanged;
		}

		private void SetStateVisual(GraffitiGameState setState)
		{
			closeUpCameraTf.gameObject.SetActive(value: false);
			switch (setState)
			{
			case GraffitiGameState.MAIN_STATE:
			{
				TextMeshProUGUI graffitiTitle = player.ui.graffitiTitle;
				string text = (player.ui.graffitiNewLabel.text = "");
				graffitiTitle.text = text;
				player.ui.graffitiTimerBar.fillAmount = 0f;
				player.ui.graffitiTimerBackdrop.gameObject.SetActive(value: false);
				break;
			}
			case GraffitiGameState.COMPLETE_TARGETS:
			{
				characterPuppet.anim.speed = 1f;
				characterPuppet.SetBoostpackEffect(BoostpackEffectMode.OFF);
				Play("grafSlashFinisher");
				player.RemoveGraffitiSlash();
				player.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK, fromPlayer: false);
				if (gSpot.attachedTo == GraffitiSpot.AttachType.COPS)
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.graffitiComplete);
				}
				else
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.graffitiComplete);
				}
				activeTarget.Ring.localEulerAngles = new Vector3(0f, 0f, 0f);
				player.ui.closeUp.enabled = true;
				RawImage closeUp = player.ui.closeUp;
				RenderTexture texture = (closeUpCameraTf.GetComponent<Camera>().targetTexture = new RenderTexture(closeUpCameraTf.GetComponent<Camera>().pixelWidth, closeUpCameraTf.GetComponent<Camera>().pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt));
				closeUp.texture = texture;
				closeUpCameraTf.position = gSpot.transform.position + gSpot.transform.forward * 2.3f;
				closeUpCameraTf.rotation = Quaternion.LookRotation(gSpot.transform.forward * -1f);
				closeUpCameraTf.gameObject.SetActive(value: true);
				int cullingMask = graffitiCamDefaultCulling & -32769;
				graffitiCameraTf.GetComponent<Camera>().cullingMask = cullingMask;
				Core.Instance.UIManager.effects.ShowBarsInAndOut(0.02f, 0.56f, 0.02f, UpdateType.Normal);
				Core.Instance.UIManager.effects.FadeInAndOut(0.02f, 0.56f, 0.02f, new Color(0.05f, 0.05f, 0.05f, 0.75f), UpdateType.Normal);
				break;
			}
			case GraffitiGameState.SHOW_PIECE:
				closeUpCameraTf.GetComponent<Camera>().targetTexture.Release();
				closeUpCameraTf.gameObject.SetActive(value: false);
				player.ui.closeUp.enabled = false;
				player.ui.graffitiTitle.text = $"'{grafArt.title}'";
				player.ui.graffitiNewLabel.text = ((player.newGraffitiBonusMulti != 1) ? graffitiNewText : "");
				showPieceCamSpeed = 25f;
				DoGraffitiFinishEffect(gSpot.size);
				graffitiCameraTf.GetComponent<Camera>().cullingMask = graffitiCamDefaultCulling;
				graffitiCameraTf.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
				graffitiCameraTf.position = (lastCamPos = (camTargetPos = gSpot.transform.position + gSpot.transform.forward * 3f));
				screenShakeOriginPos = graffitiCameraTf.position;
				graffitiCameraTf.rotation = Quaternion.LookRotation(gSpot.transform.forward * -1f);
				screenShakeLocalOriginRot = graffitiCameraTf.localEulerAngles;
				break;
			case GraffitiGameState.FINISHED:
			case GraffitiGameState.SHOW_FAIL:
			case GraffitiGameState.CLOSED:
				break;
			}
		}

		private void UpdateMainStateVisual()
		{
			for (int i = 0; i < targets.Length; i++)
			{
				targets[i].Transform.GetChild(0).localPosition = Vector3.Lerp(targets[i].Transform.GetChild(0).localPosition, Vector3.zero, 0.3f);
			}
			targetScaleAnimTime += Time.deltaTime * 1.64f;
			for (int j = 0; j < targets.Length; j++)
			{
				if (targets[j].state == Target.TargetState.OPEN)
				{
					targets[j].TargetTransform.localScale = originalTargetScale * targetScale.Evaluate(targetScaleAnimTime % 1f);
				}
				else
				{
					targets[j].Transform.GetChild(0).localScale = Vector3.one;
				}
			}
			player.ui.graffitiTimerBar.fillAmount = timeRemaining / ((float)targets.Length * TimePerTarget);
			player.ui.graffitiTimerBackdrop.gameObject.SetActive(value: true);
			if (effectSpeed >= 0.5f)
			{
				effectSpeed = Mathf.Lerp(effectSpeed, 0f, Core.dt * 10f);
			}
			else
			{
				effectSpeed = Mathf.Lerp(effectSpeed, 0f, Core.dt * 1f);
			}
			if (grafCamState == GrafCamState.HITLAG)
			{
				effectStun = 0f;
			}
			else
			{
				effectStun = 1f;
			}
			if ((bool)graffitiSlash)
			{
				graffitiSlash.SetSpeed(effectSpeed);
			}
			if ((bool)graffitiSlash)
			{
				graffitiSlash.SetStun(effectStun);
			}
			camTimer += Core.dt;
			switch (grafCamState)
			{
			case GrafCamState.STARTZOOM:
				graffitiCameraTf.position = Vector3.Lerp(graffitiCameraTf.position, camTargetPos, 0.05f);
				graffitiCameraTf.rotation = Quaternion.LookRotation((gSpot.transform.position - graffitiCameraTf.position).normalized);
				break;
			case GrafCamState.FAST:
				characterPuppet.anim.speed = 1f;
				graffitiCameraTf.position = Vector3.Lerp(lastCamPos, camTargetPos, camTimer / fastCamDuration);
				graffitiCameraTf.rotation = Quaternion.LookRotation((gSpot.transform.position - graffitiCameraTf.position).normalized);
				if (camTimer > fastCamDuration)
				{
					camTimer = 0f;
					grafCamState = GrafCamState.HITLAG;
					Core.Instance.UIManager.effects.FadeInAndOut(flashFade, flashDuration, flashFade, new Color(1f, 1f, 1f, 0.7f), UpdateType.Normal);
					screenShakeOriginPos = graffitiCameraTf.position;
					screenShakeLocalOriginRot = graffitiCameraTf.localEulerAngles;
				}
				break;
			case GrafCamState.HITLAG:
				characterPuppet.anim.speed = 1f;
				ScreenShakeGraffitiCam(camTimer, hitlagCamDuration, lastSlashDir);
				if (camTimer > hitlagCamDuration)
				{
					camTimer = 0f;
					grafCamState = GrafCamState.SLOW;
					PlayPuppetSlashAnim(slashSide, outAnim: true);
				}
				break;
			case GrafCamState.SLOW:
			{
				Vector3 b = camTargetPos + (camTargetPos - lastCamPos).normalized * 0.5f;
				graffitiCameraTf.position = Vector3.Lerp(graffitiCameraTf.position, b, 0.1f);
				graffitiCameraTf.rotation = Quaternion.LookRotation((gSpot.transform.position - graffitiCameraTf.position).normalized);
				if (effectSpeed > 0.5f)
				{
					characterPuppet.anim.speed = 1f;
				}
				else
				{
					characterPuppet.anim.speed = effectSpeed;
				}
				if (characterPuppet.anim.speed > 1f)
				{
					characterPuppet.anim.speed = 1f;
				}
				break;
			}
			}
			if (activeTarget.state == Target.TargetState.ACTIVE)
			{
				float num = (0f - Time.time * 500f) % 360f;
				activeTarget.Ring.localEulerAngles = new Vector3(0f, 0f, 0f - num);
				activeTarget.TargetTransform.localEulerAngles = new Vector3(0f, 0f, num);
			}
		}

		private void ScreenShakeGraffitiCam(float shakeTime, float shakeduration, Vector3 dir)
		{
			VisualProperties visualProperties = visualPropertiesDict[currentHitSpeed];
			float num = SimpleEasing.SimpleEase(visualProperties.shakeDecayEase, shakeTime, 1f, -1f, shakeduration);
			float z = 15f * Mathf.Sin(Time.time * (visualProperties.shakePosSpeed * 0.45f));
			Vector3 vector = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0f, 0f, z)), Vector3.one).MultiplyPoint(Vector3.Reflect(dir, Vector3.up)) * visualProperties.shakePosMagnitude * (Mathf.Cos(Time.time * visualProperties.shakePosSpeed) * num);
			if (posShakeFramer == 0)
			{
				vector = lastPosShake;
			}
			else
			{
				lastPosShake = vector;
			}
			posShakeFramer++;
			if (posShakeFramer >= 3)
			{
				posShakeFramer = 0;
			}
			Vector3 vector2 = new Vector3
			{
				x = (visualProperties.shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * visualProperties.shakeRotSpeed, 6f) * visualProperties.shakeRotMagnitude) * num,
				y = (visualProperties.shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * visualProperties.shakeRotSpeed, 7f) * visualProperties.shakeRotMagnitude) * num,
				z = 0f
			};
			graffitiCameraTf.position = screenShakeOriginPos + graffitiCameraTf.TransformVector(vector);
			graffitiCameraTf.localRotation = Quaternion.Euler(screenShakeLocalOriginRot + vector2);
		}

		private void HitTargetVisual(Target target, Vector3 slashDirectionUI, HitSpeed hitSpeed)
		{
			currentHitSpeed = hitSpeed;
			float num = Random.Range(distanceToGrafSpot - 0.25f, distanceToGrafSpot + 0.25f);
			Vector3 vector = slashDirectionUI;
			vector.x += Random.Range(-0.2f, 0.2f);
			vector.y += Random.Range(-0.2f, 0.2f);
			Vector3 position = new Vector3
			{
				x = vector.normalized.x * num,
				y = (0f - vector.normalized.y) * num * 0.6f,
				z = num
			};
			SetCameraTarget(gSpot.transform.TransformPoint(position));
			grafCamState = GrafCamState.FAST;
			slashSide = GetClosestCardinalDirection(Quaternion.FromToRotation(Vector3.right, gSpot.transform.right * -1f) * slashDirectionUI, gSpot, ref slashCardinal);
			lastSlashDir = slashDirectionUI;
			PlayPuppetSlashAnim(slashSide);
			player.RemoveGraffitiSlash();
			DoGraffitiEffect();
			audioManager.PlaySfxGameplay(SfxCollectionID.GraffitiSfx, AudioClipID.graffitiSlash);
			_ = activeTarget.Transform.position;
			lastActiveTarget.SetState(Target.TargetState.CLEARED);
			if (AllTargetsDone())
			{
				activeTarget.SetState(Target.TargetState.CLEARED);
			}
			else
			{
				if (OneTargetLeft())
				{
					targets[0].SetState(Target.TargetState.OPEN);
				}
				activeTarget.SetState(Target.TargetState.ACTIVE);
				RefreshLockedTargets();
			}
			activeTarget.TargetSprite.transform.localScale *= 1.5f;
			activeTarget.TargetSprite.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InBack);
			activeTarget.Ring.transform.localScale *= 1.75f;
			activeTarget.Ring.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.InBack);
		}

		private void RefreshLockedTargets()
		{
			availableTargets = gSpot.GraffitiArtInfo.GetAvailableTargets(unlockedGraffiti, targetsHitSequence, gSpot.size);
			for (int i = 1; i < targets.Length; i++)
			{
				if (targets[i].state != Target.TargetState.ACTIVE && targets[i].state != Target.TargetState.CLEARED)
				{
					if (GraffitiArtInfo.TargetIsNotAllowed(availableTargets, i))
					{
						targets[i].SetState(Target.TargetState.LOCKED);
					}
					else if (targets[i].state == Target.TargetState.LOCKED)
					{
						targets[i].SetState(Target.TargetState.OPEN);
					}
				}
			}
		}

		private void SetCameraTarget(Vector3 pos)
		{
			lastCamPos = graffitiCameraTf.position;
			camTargetPos = pos;
			camTimer = 0f;
		}

		private void DoGraffitiEffect()
		{
			effectSpeed = 100f;
			graffitiSlash = player.CreateGraffitiSlashEffect(characterPuppet.transform, slashCardinal).GetComponent<GraffitiEffect>();
			graffitiSlash.SetSpeed(effectSpeed);
		}

		private void DoGraffitiFinishEffect(GraffitiSize size)
		{
			effectSpeed = 100f;
			graffitiSlash = player.CreateGraffitiFinishEffect(gSpot.transform, size).GetComponent<GraffitiEffect>();
			graffitiSlash.SetSpeed(effectSpeed);
		}

		public void PlayPuppetSlashAnim(Side side, bool outAnim = false)
		{
			characterPuppet.anim.Play("grafSlash" + side.ToString() + (outAnim ? "_Out" : ""));
		}

		public void Play(string anim)
		{
			characterPuppet.anim.Play(anim);
		}

		public void CreateProperties()
		{
			visualPropertiesDict = new Dictionary<HitSpeed, VisualProperties>
			{
				{
					HitSpeed.NORMAL,
					new VisualProperties
					{
						shakePosMagnitude = 1.5f,
						shakeRotMagnitude = 14f,
						shakePosSpeed = 150f,
						shakeRotSpeed = 27.5f,
						shakeDecayEase = SimpleEasing.EaseType.EaseLinear
					}
				},
				{
					HitSpeed.QUICK,
					new VisualProperties
					{
						shakePosMagnitude = 0.08f,
						shakeRotMagnitude = 2f,
						shakePosSpeed = 150f,
						shakeRotSpeed = 27.5f,
						shakeDecayEase = SimpleEasing.EaseType.EaseLinear
					}
				}
			};
		}
	}
}
