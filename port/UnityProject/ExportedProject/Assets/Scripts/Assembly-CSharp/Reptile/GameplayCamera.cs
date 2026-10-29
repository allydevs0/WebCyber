using System;
using System.Collections.Generic;
using Kaj.SimpleEasing;
using Rewired;
using UnityEngine;

namespace Reptile
{
	public class GameplayCamera : MonoBehaviour
	{
		[Serializable]
		public class ScreenShake
		{
			public float shakePosMagnitude = 0.08f;

			public float shakeRotMagnitude = 2f;

			public float shakePosSpeed = 150f;

			public float shakeRotSpeed = 27.5f;

			public SimpleEasing.EaseType shakeDecayEase = SimpleEasing.EaseType.EaseLinear;
		}

		private static GameplayCamera instance;

		internal Player player;

		private CameraMode cameraMode;

		private CameraMode cameraModePrev;

		private float cameraModeFade;

		internal CameraZone cameraZone;

		internal Transform realTf;

		internal Transform sunCamTf;

		internal Camera cam;

		private CameraModeLock camLock;

		public float lookAngleLowest = -45f;

		public float lookAngleHighest = 50f;

		public float lookAtSharpness = 50f;

		public float lookAtVertSharpness = 25f;

		public float postureOffsetSharpness = 10f;

		public float topLocalLookAt = 1.4f;

		public float bottomLocalLookAt = -0.18f;

		public float obstructionCheckRadius = 0.15f;

		public List<Collider> IgnoredColliders = new List<Collider>();

		[NonSerialized]
		public float postureOffsetTarget;

		private float hitpause;

		internal int fixedFrameCounter;

		public float joystickFactor = 1.5f;

		public float mouseFactor = 0.5f;

		[NonSerialized]
		public bool ignoreInput;

		[NonSerialized]
		public bool mouseForOrbit;

		[NonSerialized]
		public Vector2 orbitInput;

		private bool inverted = true;

		private float inputDeadZone = 0.15f;

		private GameInput gameInput;

		private float lastCamMouseOrbitInputTimer;

		[Tooltip("Delay after mouse stopped moving before it starts fading back to automatic camera control")]
		public float mouseOrbitDoneDelay = 2f;

		[Tooltip("Duration of fade to automatic camera control when no longer orbiting manually")]
		public float orbitFadeDuration = 5f;

		[Tooltip("Default speed of transition between camera modes")]
		public float cameraModeTransitionSpeed = 2f;

		[Header("Drag")]
		public float defaultCamHeight = 2f;

		public float maxBelowPlayer;

		public float maxAbovePlayer;

		public float camHeightSharpness = 60f;

		public float dragDistanceDefault;

		public float dragDistanceMax;

		public float orbitDistanceSharpness = 10f;

		[NonSerialized]
		public CameraModeDrag cameraModeDrag;

		[Header("Grind")]
		[Tooltip("Cam y offset from lookat when grinding horizontally")]
		public float grindHeightOffsetDefault = 0.7f;

		[Tooltip("Cam y offset goes 1 up/down for each these degrees (up to min/max below)")]
		public float grindHeightOffsetAngleQuotient = 18f;

		public float grindHeightOffsetMin = -1f;

		public float grindHeightOffsetMax = 3.5f;

		[Tooltip("How quick camera is pushed behind the player")]
		public float grindPushBehind = 3f;

		public float grindTiltMax = 13f;

		public float grindTiltSpeed = 6f;

		[Header("Wallrun")]
		[Tooltip("How much player is from screen center horizontally")]
		public float wallrunPlayerCenterOffset = 1f;

		[Tooltip("Y pos player on screen")]
		public float wallrunPlayerYOffset = 1f;

		[Tooltip("How much camera drags behind player (fade value)")]
		public float wallrunDragBehind = 5.5f;

		[Tooltip("How much camera looks away from/to wallrun (also influenced by dragBehind)")]
		public float wallrunAwayFromWall = 1.3f;

		public float wallrunTiltAngle = 33f;

		[Header("Vert")]
		[Tooltip("When speed is getting lower than this, start fading to descend cam (with fade done when speed is the same but going downwards)")]
		public float vertSpeedThreshold = 8f;

		[Tooltip("Max angle looking down during descend")]
		public float vertFallAngle = 80f;

		[Tooltip("Max cam dist during descend")]
		public float vertFallCamDist = 5f;

		[Header("Handplant")]
		public float handplantCamDist = 3.5f;

		public float handplantHeightOffsetDefault = 1f;

		[Header("Screenshake")]
		public ScreenShake screenShakeProfileUltraLight;

		public ScreenShake screenShakeProfileExtraLight;

		public ScreenShake screenShakeProfileJustABitLighter;

		public ScreenShake screenShakeProfileLight;

		public ScreenShake screenShakeProfileMedium;

		public ScreenShake screenShakeProfileHeavy;

		private bool screenShaking;

		private Transform curShakeTf;

		private float screenShakeTime;

		private float screenShakeDuration;

		private int screenShakeFrameCounter;

		private Vector3 screenShakeDir;

		private Vector3 lastScreenShakeLocalPos;

		private Vector3 screenShakeOriginPos;

		private Vector3 screenShakeLocalOriginRot;

		private bool screenShakeContinuous;

		private ScreenShakeType curShakeType;

		public float dt => Time.deltaTime;

		private void Awake()
		{
			instance = this;
			realTf = base.transform;
			sunCamTf = UnityEngine.Object.FindObjectOfType<SunFlareGPU>().GetComponentInChildren<Camera>().transform;
			cam = GetComponent<Camera>();
			gameInput = Core.Instance.GameInput;
			cameraModeDrag = new CameraModeDrag();
		}

		public void SetPlayerToFollow(Player p)
		{
			player = p;
			ResetCameraPositionRotation();
			IgnoredColliders.Clear();
			IgnoredColliders.AddRange(p.GetComponentsInChildren<Collider>());
		}

		public void SetSunCamTransform(Transform sunCameraTransform)
		{
			sunCamTf = sunCameraTransform;
		}

		public void ResetCameraPositionRotation()
		{
			cameraZone = null;
			SetCameraMode(cameraModeDrag, reset: true);
		}

		public void SetCustomStartPos(Vector3 pos)
		{
		}

		public void UpdateCamera()
		{
			if (!player || !base.isActiveAndEnabled)
			{
				return;
			}
			if (hitpause > 0f && player.GetVelocity() == Vector3.zero)
			{
				hitpause -= dt;
				return;
			}
			Vector3 positionFinal = cameraMode.positionFinal;
			UpdateCameraInput();
			if (camLock != null)
			{
				if (cameraMode is CameraModeLock cameraModeLock)
				{
					if (cameraModeLock != camLock)
					{
						SetCameraMode(camLock);
					}
				}
				else
				{
					SetCameraMode(camLock);
				}
			}
			else if (CameraZoneActive())
			{
				if (cameraMode is CameraModeZone cameraModeZone)
				{
					if (cameraModeZone.cameraZone != cameraZone)
					{
						SetCameraMode(cameraZone);
					}
				}
				else
				{
					SetCameraMode(cameraZone);
				}
			}
			else if (player.ability != null && player.ability.customCameraMode != null)
			{
				if (cameraMode != player.ability.customCameraMode)
				{
					SetCameraMode(player.ability.customCameraMode);
				}
			}
			else if (cameraMode != cameraModeDrag)
			{
				SetCameraMode(cameraModeDrag);
			}
			cameraMode.Update(fixedFrameCounter);
			Vector3 vector = cameraMode.positionFinal;
			Vector3 vector2 = cameraMode.lookAtPos;
			float num = cameraMode.tilt;
			if (cameraModePrev != null)
			{
				cameraModePrev.Update(fixedFrameCounter);
				cameraModeFade += dt * cameraModeTransitionSpeed;
				if (cameraModeFade >= 1f)
				{
					cameraModePrev = null;
				}
				else
				{
					float t = FadeOut3(cameraModeFade);
					vector = Vector3.Lerp(cameraModePrev.positionFinal, vector, t);
					vector2 = Vector3.Lerp(cameraModePrev.lookAtPos, vector2, t);
					num = Mathf.Lerp(cameraModePrev.tilt, num, t);
				}
			}
			Vector3 eulerAngles = Quaternion.LookRotation(vector2 - vector, Vector3.up).eulerAngles;
			Quaternion rotation = Quaternion.Euler(new Vector3(eulerAngles.x, eulerAngles.y, num));
			if (float.IsNaN(vector.x))
			{
				cameraMode.positionFinal = positionFinal;
				vector = positionFinal;
			}
			realTf.SetPositionAndRotation(vector, rotation);
			if (sunCamTf != null)
			{
				sunCamTf.position = vector;
			}
			UpdateScreenShakes();
			fixedFrameCounter = 2;
		}

		private bool CameraZoneActive()
		{
			if (cameraZone != null)
			{
				if (cameraZone.onlyOnGrinds)
				{
					return player.IsGrinding();
				}
				return true;
			}
			return false;
		}

		private void SetCameraMode(CameraMode mode, bool reset = false)
		{
			cameraModePrev = (reset ? null : cameraMode);
			cameraMode = mode;
			mode.Activate(player, cameraModePrev);
			cameraModeFade = 0f;
		}

		private void SetCameraMode(CameraZone camZone)
		{
			SetCameraMode(new CameraModeZone(camZone));
		}

		private float FadeOut3(float f)
		{
			f = 1f - f;
			return 1f - f * f * f;
		}

		private void UpdateCameraInput()
		{
			float num = gameInput.GetAxis(13);
			float num2 = gameInput.GetAxis(14);
			if (ignoreInput)
			{
				num = (num2 = 0f);
			}
			if (inverted)
			{
				num2 = 0f - num2;
			}
			if (num != 0f || num2 != 0f)
			{
				if (gameInput.IsInputSourceForAction(ControllerType.Mouse, 13) || gameInput.IsInputSourceForAction(ControllerType.Mouse, 14))
				{
					mouseForOrbit = true;
				}
				else if (lastCamMouseOrbitInputTimer > 0.2f)
				{
					mouseForOrbit = false;
				}
			}
			if (mouseForOrbit)
			{
				if (num == 0f && num2 == 0f)
				{
					lastCamMouseOrbitInputTimer += dt;
				}
				else
				{
					lastCamMouseOrbitInputTimer = 0f;
				}
				float num3 = 1f / 60f / Core.dt;
				num *= num3;
				num2 *= num3;
				mouseForOrbit = lastCamMouseOrbitInputTimer < mouseOrbitDoneDelay;
			}
			if (!mouseForOrbit)
			{
				if (num < inputDeadZone && num > 0f - inputDeadZone)
				{
					num = 0f;
				}
				else if (num > 1f - inputDeadZone || num < -1f + inputDeadZone)
				{
					num = ((!(num < 0f)) ? 1 : (-1));
				}
				if (num2 < inputDeadZone && num2 > 0f - inputDeadZone)
				{
					num2 = 0f;
				}
				else if (num2 > 1f - inputDeadZone || num2 < -1f + inputDeadZone)
				{
					num2 = ((!(num2 < 0f)) ? 1 : (-1));
				}
			}
			if (cameraMode.inputEnabled)
			{
				CalculateOrbitInput(num, num2);
			}
			else
			{
				orbitInput = Vector2.zero;
			}
		}

		public void SetCamLock(Transform tf)
		{
			camLock = new CameraModeLock(tf);
		}

		public void SetCamLock(BasicEnemy enemy)
		{
			camLock = new CameraModeLock(enemy.transform, () => !enemy.IsDead(), canSwitchToOtherEnemies: true);
		}

		public void UnsetCamLock()
		{
			camLock = null;
		}

		private void CalculateOrbitInput(float camXInput, float camYInput)
		{
			orbitInput = CalculateNormalOrbitInput(camXInput, camYInput);
		}

		private Vector2 CalculateNormalOrbitInput(float camXInput, float camYInput)
		{
			float aimSensitivity = Core.Instance.SaveManager.Settings.gameplaySettings.aimSensitivity;
			float num = Mathf.Lerp(0.75f, 1.8f, aimSensitivity);
			bool invertY = Core.Instance.SaveManager.Settings.gameplaySettings.invertY;
			return new Vector2(camXInput * num, camYInput * num * (float)((!invertY) ? 1 : (-1)));
		}

		public void StartHitpause(float duration)
		{
			hitpause = duration;
		}

		private void OnDestroy()
		{
			instance = null;
		}

		private void UpdateScreenShakes()
		{
			if (screenShaking)
			{
				screenShakeTime += Core.dt;
				DoScreenShake(curShakeTf, screenShakeTime, screenShakeDuration, screenShakeContinuous);
				if (screenShakeTime > screenShakeDuration)
				{
					screenShaking = false;
				}
			}
		}

		public static void StartScreenShake(ScreenShakeType size, float duration, bool continuous = false)
		{
			StartScreenShake(instance.realTf, size, duration, UnityEngine.Random.insideUnitCircle.normalized, continuous);
		}

		public static void StartScreenShake(Transform onTransform, ScreenShakeType profile, float duration, Vector3 dir, bool continous)
		{
			instance.curShakeType = profile;
			instance.screenShaking = true;
			instance.curShakeTf = onTransform;
			instance.screenShakeTime = 0f;
			instance.screenShakeDuration = duration;
			instance.screenShakeOriginPos = onTransform.position;
			instance.screenShakeLocalOriginRot = onTransform.localEulerAngles;
			instance.screenShakeContinuous = continous;
			instance.lastScreenShakeLocalPos = Vector3.zero;
			instance.screenShakeFrameCounter = 1;
			instance.screenShakeDir = dir;
		}

		private void DoScreenShake(Transform shakeTf, float shakeTime, float shakeduration, bool continuous)
		{
			ScreenShake screenShake = instance.screenShakeProfileLight;
			if (curShakeType == ScreenShakeType.ULTRALIGHT)
			{
				screenShake = instance.screenShakeProfileUltraLight;
			}
			else if (curShakeType == ScreenShakeType.EXTRALIGHT)
			{
				screenShake = instance.screenShakeProfileExtraLight;
			}
			else if (curShakeType == ScreenShakeType.JUST_A_BIT_LIGHTER)
			{
				screenShake = instance.screenShakeProfileJustABitLighter;
			}
			else if (curShakeType == ScreenShakeType.MEDIUM)
			{
				screenShake = instance.screenShakeProfileMedium;
			}
			else if (curShakeType == ScreenShakeType.HEAVY)
			{
				screenShake = instance.screenShakeProfileHeavy;
			}
			float num = SimpleEasing.SimpleEase(screenShake.shakeDecayEase, shakeTime, 1f, -1f, shakeduration);
			float z = 15f * Mathf.Sin(Time.time * (screenShake.shakePosSpeed * 0.45f));
			Vector3 vector = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0f, 0f, z)), Vector3.one).MultiplyPoint(Vector3.Reflect(instance.screenShakeDir, Vector3.up));
			float num2 = Time.time * screenShake.shakePosSpeed;
			num2 = ((!continuous) ? Mathf.Cos(num2) : (num2 - (float)(int)num2));
			Vector3 vector2 = vector * screenShake.shakePosMagnitude * (num2 * num);
			if (instance.screenShakeFrameCounter == 0)
			{
				vector2 = instance.lastScreenShakeLocalPos;
			}
			else
			{
				instance.lastScreenShakeLocalPos = vector2;
			}
			instance.screenShakeFrameCounter++;
			if (instance.screenShakeFrameCounter >= 3)
			{
				instance.screenShakeFrameCounter = 0;
			}
			Vector3 vector3 = new Vector3
			{
				x = (screenShake.shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * screenShake.shakeRotSpeed, 6f) * screenShake.shakeRotMagnitude) * num,
				y = (screenShake.shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * screenShake.shakeRotSpeed, 7f) * screenShake.shakeRotMagnitude) * num,
				z = 0f
			};
			shakeTf.position += shakeTf.TransformVector(vector2);
			shakeTf.localRotation = Quaternion.Euler(shakeTf.localEulerAngles + vector3);
		}
	}
}
