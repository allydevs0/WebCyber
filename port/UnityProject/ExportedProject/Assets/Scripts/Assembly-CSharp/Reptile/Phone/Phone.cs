using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class Phone : MonoBehaviour
	{
		private struct NotificationInfo
		{
			public Notification notification;

			public string topBarText;

			public AUnlockable unlockable;
		}

		private enum PhoneState
		{
			OFF = 0,
			BOOTINGUP = 1,
			ON = 2,
			SHUTTINGDOWN = 3,
			NOTIFICATION_OUTER = 4,
			NOTIFICATION_INNER = 5,
			NOTIFICATION_BEGIN = 6,
			NOTIFICATION_END = 7,
			BEING_CALLED = 8,
			IN_CALL = 9
		}

		public const float CloseHoldDuration = 0.25f;

		public const float ScreenOnDelay = 0.2f;

		private App m_CurrentApp;

		private Sequence m_PhoneAnimation;

		private Sequence m_PhoneTurnOffAnimation;

		private Sequence m_PhoneRingAnimation;

		private Sequence m_OSSequence;

		private Stack<App> m_PreviousApps;

		private Queue<NotificationInfo> activeNotifications;

		[SerializeField]
		private RectTransform m_PhoneOpenTransform;

		[SerializeField]
		private RectTransform m_PhoneClosedTransform;

		[SerializeField]
		private GameObject openPhoneCanvas;

		[SerializeField]
		private GameObject closedPhoneCanvas;

		[SerializeField]
		private RectTransform m_Screen;

		[SerializeField]
		private RectTransform m_NotificationMainScreen;

		[SerializeField]
		private RectTransform m_NotificationSwiperMain;

		[SerializeField]
		private RectTransform m_NotificationSwiperOuter;

		[SerializeField]
		private Statusbar m_Statusbar;

		[SerializeField]
		private PhoneTransformState m_ClosedState;

		[SerializeField]
		private PhoneTransformState m_OpenState;

		[SerializeField]
		private PhoneTransformState m_PopState;

		[SerializeField]
		private PhoneTransformState m_RingState;

		[SerializeField]
		private PhoneTransformState m_CutsceneOpenState;

		[SerializeField]
		private PhoneTransformState m_NotifyMainPopupState;

		private Player player;

		private AudioManager audioManager;

		private GameInput gameInput;

		private float m_CloseTimer;

		private bool m_PhoneAllowed = true;

		private bool phoneOnBeforeNonallowed;

		private bool externalForcedPickup;

		private bool phoneOpenBeforeCall;

		private float phoneScale = 0.335f;

		private int enabledMask;

		private float phoneRingIntensity = 5f;

		private float phoneRingDuration = 1f;

		private float phoneRingInterval = 1f;

		private float phoneNotificationIntensity = 2.5f;

		private float phoneNotificationDuration = 0.5f;

		private float phoneNotificationEndWait = 0.5f;

		private Vector2 originalSwiperPosition;

		private Vector2 originalMainSwiperPosition;

		private Coroutine mainNotificationCoroutine;

		private Coroutine outerNotificationCoroutine;

		public RectTransform dynamicGameplayScreen;

		public GameObject grafSpotUI;

		public GameObject storySpotUI;

		private bool showOnMainScreen;

		private PhoneState state;

		public Vector2 ScreenSize => m_Screen.sizeDelta;

		public bool IsOn => state == PhoneState.ON;

		public bool IsBeingCalled => state == PhoneState.BEING_CALLED;

		public float FoldAmount { get; private set; }

		public Image ScreenOverlay { get; private set; }

		public Statusbar Statusbar => m_Statusbar;

		public bool IsUsingMinimap
		{
			get
			{
				if (IsOn)
				{
					return m_CurrentApp is AppHomeScreen;
				}
				return false;
			}
		}

		public Dictionary<string, App> AppInstances { get; private set; }

		public void PhoneInit(Player setPlayer)
		{
			state = PhoneState.ON;
			player = setPlayer;
			audioManager = Core.Instance.AudioManager;
			gameInput = Core.Instance.GameInput;
			AppInstances = new Dictionary<string, App>();
			App[] componentsInChildren = GetComponentsInChildren<App>();
			foreach (App app in componentsInChildren)
			{
				AppInstances.Add(app.name, app);
				if (app.GetType() != typeof(AppHomeScreen))
				{
					app.Init(this, audioManager);
				}
				app.gameObject.SetActive(value: false);
			}
			AppInstances["AppHomeScreen"].Init(this, audioManager);
			m_PreviousApps = new Stack<App>();
			activeNotifications = new Queue<NotificationInfo>();
			SetPhoneTransformState(m_PhoneOpenTransform, m_ClosedState);
			SetPhoneTransformState(m_PhoneClosedTransform, m_ClosedState);
			originalSwiperPosition = m_NotificationSwiperOuter.anchoredPosition;
			originalMainSwiperPosition = m_NotificationSwiperMain.anchoredPosition;
			openPhoneCanvas.SetActive(value: false);
			closedPhoneCanvas.SetActive(value: false);
			ScreenOverlay = m_Screen.Find("ScreenOverlay").GetComponent<Image>();
			phoneScale = m_Screen.localScale.x;
			InitEnableOSSequence();
			InitTurnOnAnimation();
			InitTurnOffAnimation();
			InitRingAnimation();
			OpenApp(typeof(AppHomeScreen));
			TurnOff();
		}

		public void Terminate()
		{
			m_CurrentApp.OnAppDisable();
			foreach (App value in AppInstances.Values)
			{
				value.Terminate();
			}
			m_PhoneAnimation.Pause();
			m_PhoneAnimation.Kill();
			m_PhoneTurnOffAnimation.Pause();
			m_PhoneTurnOffAnimation.Kill();
			m_OSSequence.Pause();
			m_OSSequence.Kill();
			m_PhoneRingAnimation.Pause();
			m_PhoneRingAnimation.Kill();
			m_PreviousApps = null;
			activeNotifications = null;
		}

		private void GetInput(App app)
		{
			if (app.HandleInput && player.userInputEnabled && !player.IsBusyWithSequence() && (state < PhoneState.NOTIFICATION_OUTER || state > PhoneState.NOTIFICATION_END) && player.ability != player.danceAbility)
			{
				if (gameInput.GetButtonNew(57))
				{
					app.OnPressLeft();
				}
				if (gameInput.GetButtonNew(29))
				{
					app.OnPressRight();
				}
				if (gameInput.GetButtonNew(21))
				{
					app.OnPressUp();
				}
				if (gameInput.GetButtonNew(56))
				{
					app.OnPressDown();
				}
				if (gameInput.GetButtonHeld(57))
				{
					app.OnHoldLeft();
				}
				if (gameInput.GetButtonHeld(29))
				{
					app.OnHoldRight();
				}
				if (gameInput.GetButtonHeld(21))
				{
					app.OnHoldUp();
				}
				if (gameInput.GetButtonHeld(56))
				{
					app.OnHoldDown();
				}
				if (gameInput.GetButtonUp(57))
				{
					app.OnReleaseLeft();
				}
				if (gameInput.GetButtonUp(29))
				{
					app.OnReleaseRight();
				}
				if (gameInput.GetButtonUp(21))
				{
					app.OnReleaseUp();
				}
				if (gameInput.GetButtonUp(56))
				{
					app.OnReleaseDown();
				}
			}
		}

		public void PhoneUpdate()
		{
			if (state == PhoneState.ON)
			{
				player.showRepLingerTimer = 0.5f;
				if (m_CurrentApp != null)
				{
					OSUpdate();
				}
			}
			OpenCloseHandeling();
		}

		private void OSUpdate()
		{
			GetInput(m_CurrentApp);
			m_CurrentApp.OnAppUpdate();
			m_CurrentApp.OnAppLateUpdate();
		}

		private void OpenCloseHandeling()
		{
			if (state == PhoneState.ON)
			{
				if (gameInput.GetButtonHeld(57) && player.userInputEnabled)
				{
					m_CloseTimer += Time.deltaTime / 0.25f;
					if (m_CloseTimer >= 1f && !player.IsBusyWithSequence())
					{
						TurnOff(playSound: true);
					}
				}
				else
				{
					m_CloseTimer = 0f;
				}
			}
			else if ((state == PhoneState.OFF || state == PhoneState.BOOTINGUP || state == PhoneState.SHUTTINGDOWN) && !player.phoneLocked && !player.IsBusyWithSequence() && (gameInput.GetButtonNew(21) || gameInput.GetButtonNew(29) || gameInput.GetButtonNew(57)) && m_PhoneAllowed && (WorldHandler.instance.currentEncounter == null || WorldHandler.instance.currentEncounter.allowPhone) && TurnOn())
			{
				audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.ui_phoneFlipOpen);
			}
		}

		public void AllowPhone(bool isAllowed, bool completelyTurnOff = false, bool turnOnWhenAllowed = false)
		{
			if (m_PhoneAllowed == isAllowed)
			{
				return;
			}
			m_PhoneAllowed = isAllowed;
			bool flag = (phoneOnBeforeNonallowed = state == PhoneState.ON || state == PhoneState.BOOTINGUP);
			if ((!isAllowed | (WorldHandler.instance.currentEncounter != null && !WorldHandler.instance.currentEncounter.allowPhone) | player.phoneLocked) & (state != PhoneState.IN_CALL) & (state != PhoneState.ON || m_CurrentApp.GetType() != typeof(AppGraffiti) || !player.inGraffitiGame))
			{
				if (flag)
				{
					if (completelyTurnOff)
					{
						completelyTurnOff = TurnOff();
					}
					if (!completelyTurnOff)
					{
						SetPhoneUIEnabled(enableUI: false, openPhone: false);
					}
				}
				else if (state == PhoneState.BEING_CALLED || (state >= PhoneState.NOTIFICATION_OUTER && state <= PhoneState.NOTIFICATION_END))
				{
					SetPhoneUIEnabled(enableUI: false, openPhone: false);
				}
			}
			else if (state == PhoneState.BEING_CALLED || state == PhoneState.NOTIFICATION_OUTER)
			{
				SetPhoneUIEnabled(enableUI: true, openPhone: false);
			}
			else
			{
				if (turnOnWhenAllowed && (phoneOnBeforeNonallowed || state == PhoneState.NOTIFICATION_INNER) && (!flag || state != PhoneState.NOTIFICATION_INNER))
				{
					TurnOn();
				}
				if (flag || state == PhoneState.NOTIFICATION_INNER)
				{
					SetPhoneUIEnabled(enableUI: true, openPhone: true);
				}
				if (state != PhoneState.NOTIFICATION_INNER)
				{
					PeekNotification();
				}
			}
		}

		public bool TurnOn()
		{
			bool result = false;
			if (state != PhoneState.ON && state != PhoneState.BOOTINGUP && state != PhoneState.SHUTTINGDOWN)
			{
				state = PhoneState.BOOTINGUP;
				result = true;
				SetPhoneUIEnabled(enableUI: true, openPhone: true);
				m_PhoneAnimation.Restart();
				m_CloseTimer = 0f;
			}
			return result;
		}

		public bool TurnOff(bool playSound = false)
		{
			bool result = false;
			if (state != PhoneState.OFF && state != PhoneState.SHUTTINGDOWN && state != PhoneState.BOOTINGUP)
			{
				state = PhoneState.SHUTTINGDOWN;
				result = true;
				if (playSound)
				{
					audioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.ui_phoneFlipClose);
				}
				m_PhoneTurnOffAnimation.Restart();
				if (state >= PhoneState.NOTIFICATION_OUTER && state <= PhoneState.NOTIFICATION_END)
				{
					PeekNotification();
				}
				m_CurrentApp.OnAppDisable();
			}
			return result;
		}

		private void SetPhoneUIEnabled(bool enableUI, bool openPhone)
		{
			if (enableUI)
			{
				dynamicGameplayScreen.gameObject.SetActive(openPhone);
				openPhoneCanvas.SetActive(openPhone);
				closedPhoneCanvas.SetActive(!openPhone);
			}
			else
			{
				openPhoneCanvas.SetActive(enableUI);
				closedPhoneCanvas.SetActive(enableUI);
			}
		}

		public void PopUp(bool mainScreen, bool skipBeginAndEnd = false)
		{
			if (!mainScreen)
			{
				outerNotificationCoroutine = StartCoroutine(NotificationOuter(skipBeginAndEnd));
			}
			else
			{
				mainNotificationCoroutine = StartCoroutine(NotificationMain(skipBeginAndEnd));
			}
			m_CloseTimer = 0f;
		}

		public IEnumerator RingPhone(Player player, int characterNameIDOfCaller, int timesToRing = 5)
		{
			yield return new WaitUntil(() => player.sequenceState == SequenceState.NONE);
			m_CloseTimer = 0f;
			float timer = 0f;
			float rumbleUpto = timer + phoneRingDuration;
			float forcePickupAfterSeconds = (float)timesToRing * (phoneRingDuration + phoneRingInterval);
			externalForcedPickup = false;
			if (m_PhoneTurnOffAnimation.IsPlaying())
			{
				yield return m_PhoneTurnOffAnimation.WaitForCompletion();
			}
			if (m_PhoneAnimation.IsPlaying())
			{
				yield return m_PhoneAnimation.WaitForCompletion();
			}
			phoneOpenBeforeCall = state == PhoneState.ON || state == PhoneState.BOOTINGUP;
			state = PhoneState.BEING_CALLED;
			AppCall callingApp = (AppCall)OpenApp(typeof(AppCall));
			string callerName = callingApp.SetNPCCalling(characterNameIDOfCaller);
			callingApp.ShowPickupScreen(characterNameIDOfCaller);
			Core coreInstance = Core.Instance;
			yield return new WaitWhile(() => !m_PhoneAllowed && coreInstance.IsCorePaused);
			phoneOpenBeforeCall = phoneOpenBeforeCall || state == PhoneState.ON || state == PhoneState.BOOTINGUP;
			state = PhoneState.BEING_CALLED;
			if (phoneOpenBeforeCall)
			{
				yield return RingOpenPhone(coreInstance, callingApp, timesToRing, timer, rumbleUpto, forcePickupAfterSeconds);
			}
			else
			{
				yield return RingClosedPhone(coreInstance, callingApp, callerName, timesToRing, timer, rumbleUpto, forcePickupAfterSeconds);
			}
		}

		private IEnumerator RingOpenPhone(Core coreInstance, AppCall callingApp, float timesToRing, float timer, float rumbleUpto, float forcePickupAfterSeconds)
		{
			yield return m_PhoneAnimation.WaitForCompletion();
			if (mainNotificationCoroutine != null)
			{
				StopCoroutine(mainNotificationCoroutine);
			}
			SetPhoneUIEnabled(enableUI: true, phoneOpenBeforeCall);
			Vector3 originalOpenPosition = m_PhoneOpenTransform.localPosition;
			bool flag = false;
			audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
			while (!flag && timesToRing != 0f)
			{
				if (timer < rumbleUpto)
				{
					m_PhoneOpenTransform.localPosition = originalOpenPosition + new Vector3(UnityEngine.Random.Range(0f - phoneRingIntensity, phoneRingIntensity), UnityEngine.Random.Range(0f - phoneRingIntensity, phoneRingIntensity), 0f);
				}
				else if (timer >= rumbleUpto + phoneRingInterval)
				{
					rumbleUpto = timer + phoneRingDuration;
					if (m_PhoneAllowed)
					{
						audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
					}
				}
				timer += Core.dt;
				m_CloseTimer = 0f;
				yield return new WaitWhile(() => coreInstance.IsCorePaused);
				flag = m_PhoneAllowed && (timer > forcePickupAfterSeconds || gameInput.GetButtonNew(29) || externalForcedPickup);
			}
			SequenceHandler.instance.SetPreEnteringSequence();
			yield return callingApp.HidePickupScreen();
			state = PhoneState.IN_CALL;
			m_CloseTimer = 0f;
		}

		private IEnumerator RingClosedPhone(Core coreInstance, AppCall callingApp, string callerName, float timesToRing, float timer, float rumbleUpto, float forcePickupAfterSeconds)
		{
			bool openPhone = false;
			if (outerNotificationCoroutine != null)
			{
				StopCoroutine(outerNotificationCoroutine);
			}
			if (timesToRing != 0f)
			{
				m_NotificationSwiperOuter.anchoredPosition = originalSwiperPosition;
				SetPhoneUIEnabled(enableUI: true, phoneOpenBeforeCall);
				if (activeNotifications.Count != 0)
				{
					activeNotifications.Peek().notification.HideNotification(hideOnMainscreen: false);
				}
				callingApp.Notification.ShowNotification(this, callerName, showOnMainScreen: false, notificationScreenIsAlreadyActive: false, dontPopupOrNotify: true);
				m_PhoneRingAnimation.Restart();
				yield return m_PhoneRingAnimation.WaitForCompletion();
				Vector3 originalPosition = m_PhoneClosedTransform.localPosition;
				_ = Time.realtimeSinceStartup;
				audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
				while (!openPhone)
				{
					yield return new WaitWhile(() => !m_PhoneAllowed && coreInstance.IsCorePaused);
					if (timer < rumbleUpto)
					{
						m_PhoneClosedTransform.localPosition = originalPosition + new Vector3(UnityEngine.Random.Range(0f - phoneRingIntensity, phoneRingIntensity), UnityEngine.Random.Range(0f - phoneRingIntensity, phoneRingIntensity), 0f);
					}
					else if (timer >= rumbleUpto + phoneRingInterval)
					{
						rumbleUpto = timer + phoneRingDuration;
						if (m_PhoneAllowed)
						{
							audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
						}
					}
					timer += Core.dt;
					m_CloseTimer = 0f;
					openPhone = m_PhoneAllowed && (timer > forcePickupAfterSeconds || externalForcedPickup || gameInput.GetButtonNew(21) || gameInput.GetButtonNew(57) || gameInput.GetButtonNew(29));
				}
			}
			SequenceHandler.instance.SetPreEnteringSequence();
			if ((state != PhoneState.ON && state != PhoneState.BOOTINGUP && timesToRing != 0f) || state == PhoneState.BEING_CALLED)
			{
				float x = m_NotificationSwiperOuter.anchoredPosition.x;
				Tween t = m_NotificationSwiperOuter.DOAnchorPosX(0f - x, 0.5f).SetEase(Ease.OutExpo);
				yield return t.WaitForCompletion();
				TurnOn();
				yield return callingApp.HidePickupScreen();
			}
			if (state != PhoneState.ON && state != PhoneState.BOOTINGUP)
			{
				TurnOn();
			}
			yield return m_PhoneAnimation.WaitForCompletion();
			state = PhoneState.IN_CALL;
			callingApp.Notification.HideNotification(hideOnMainscreen: false);
			m_CloseTimer = 0f;
		}

		public void HangupCall(bool leavePhoneOpen)
		{
			state = PhoneState.ON;
			SetPhoneUIEnabled(enableUI: true, openPhone: true);
			CloseCurrentApp();
			if (!leavePhoneOpen && !phoneOpenBeforeCall)
			{
				TurnOff();
			}
			else
			{
				PeekNotification();
			}
		}

		public bool HandlePhoneCalling()
		{
			if (state == PhoneState.BEING_CALLED)
			{
				externalForcedPickup = true;
				return true;
			}
			return state == PhoneState.IN_CALL;
		}

		private void EnableOS()
		{
			m_OSSequence.Restart();
		}

		private void DisableOS()
		{
			m_Screen.gameObject.SetActive(value: false);
			m_NotificationMainScreen.gameObject.SetActive(value: false);
			state = PhoneState.OFF;
		}

		public App OpenApp(Type appToOpenType)
		{
			App result = null;
			if (appToOpenType.IsSubclassOf(typeof(App)))
			{
				if (m_CurrentApp == null)
				{
					EnableApp(appToOpenType.Name);
				}
				else if (m_CurrentApp.GetType() != appToOpenType)
				{
					m_PreviousApps.Push(m_CurrentApp);
					DisableApp(m_CurrentApp.name);
					EnableApp(appToOpenType.Name);
				}
				result = m_CurrentApp;
			}
			return result;
		}

		public T GetAppInstance<T>() where T : App
		{
			return (T)AppInstances[typeof(T).Name];
		}

		public void CloseCurrentApp()
		{
			if (m_PreviousApps.Count != 0)
			{
				DisableApp(m_CurrentApp.name);
				EnableApp(m_PreviousApps.Pop().name);
				audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_Back);
			}
		}

		public void ReturnToHome()
		{
			OpenApp(typeof(AppHomeScreen));
			m_PreviousApps.Clear();
		}

		private void EnableApp(string app)
		{
			m_CurrentApp = AppInstances[app];
			m_CurrentApp.gameObject.SetActive(value: true);
			m_CurrentApp.OnAppEnable();
		}

		private void DisableApp(string app)
		{
			AppInstances[app].OnAppDisable();
			AppInstances[app].gameObject.SetActive(value: false);
		}

		public void SetAllCamerasForSequenceEnter(bool usePhone)
		{
			if (usePhone)
			{
				SetPhoneTransformState(m_PhoneOpenTransform, m_CutsceneOpenState);
			}
			SetPhoneUIEnabled(usePhone, usePhone);
			player.phone.dynamicGameplayScreen.gameObject.SetActive(value: false);
		}

		public void SetAllCamerasForSequenceExit()
		{
			bool isOn = player.phone.IsOn;
			if (isOn)
			{
				SetPhoneTransformState(m_PhoneOpenTransform, m_OpenState);
			}
			player.phone.dynamicGameplayScreen.gameObject.SetActive(isOn);
		}

		private void InitTurnOnAnimation()
		{
			float num = 0.75f;
			m_PhoneAnimation = DOTween.Sequence();
			m_PhoneAnimation.SetUpdate(UpdateType.Manual);
			m_PhoneAnimation.SetAutoKill(autoKillOnCompletion: false);
			m_PhoneAnimation.Pause();
			m_PhoneAnimation.Append(m_PhoneOpenTransform.DOAnchorPos(m_OpenState.Position, 0.3f * num).SetEase(Ease.OutExpo));
			m_PhoneAnimation.InsertCallback(0.2f * num, () =>
			{
				EnableOS();
			});
		}

		private void InitTurnOffAnimation()
		{
			m_PhoneTurnOffAnimation = DOTween.Sequence();
			m_PhoneTurnOffAnimation.SetUpdate(UpdateType.Manual);
			m_PhoneTurnOffAnimation.SetAutoKill(autoKillOnCompletion: false);
			m_PhoneTurnOffAnimation.Pause();
			m_PhoneTurnOffAnimation.Join(m_PhoneOpenTransform.DOAnchorPos(m_ClosedState.Position, 0.5f).SetEase(Ease.OutExpo));
			m_PhoneTurnOffAnimation.AppendCallback(() =>
			{
				DisableOS();
			});
			m_PhoneTurnOffAnimation.AppendCallback(() =>
			{
				SetPhoneUIEnabled(enableUI: false, openPhone: false);
			});
		}

		private IEnumerator NotificationOuter(bool skipBeginAndEnd = false)
		{
			if (m_PhoneTurnOffAnimation.IsPlaying())
			{
				yield return m_PhoneTurnOffAnimation.WaitForCompletion();
			}
			m_NotificationSwiperOuter.anchoredPosition = originalSwiperPosition;
			SetPhoneUIEnabled(enableUI: true, openPhone: false);
			if (!skipBeginAndEnd)
			{
				Tween t = m_PhoneClosedTransform.DOAnchorPos(m_PopState.Position, 0.35f).SetEase(Ease.OutExpo).SetDelay(0.35f);
				yield return t.WaitForCompletion();
			}
			audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
			float timer = 0f;
			Vector3 originalPosition = m_PhoneClosedTransform.localPosition;
			while (timer < phoneNotificationDuration && !gameInput.GetButtonNew(29))
			{
				m_PhoneClosedTransform.localPosition = originalPosition + new Vector3(UnityEngine.Random.Range(0f - phoneNotificationIntensity, phoneNotificationIntensity), UnityEngine.Random.Range(0f - phoneNotificationIntensity, phoneNotificationIntensity), 0f);
				timer += Core.dt;
				yield return null;
			}
			for (timer = 0f; timer < phoneNotificationEndWait; timer += Core.dt)
			{
				if (gameInput.GetButtonNew(29))
				{
					yield return OpenNotification();
					yield break;
				}
				yield return null;
			}
			if (PeekNextNotification())
			{
				yield break;
			}
			for (timer = 0f; timer < 2f; timer += Core.dt)
			{
				if (gameInput.GetButtonNew(29))
				{
					yield return OpenNotification();
					yield break;
				}
				yield return null;
			}
			Tween t2 = m_PhoneClosedTransform.DOAnchorPos(m_ClosedState.Position, 0.5f).SetEase(Ease.OutExpo);
			yield return t2.WaitForCompletion();
			SetPhoneUIEnabled(enableUI: false, openPhone: false);
			PopNotification();
			PeekNotification();
		}

		private IEnumerator OpenNotification()
		{
			float x = m_NotificationSwiperOuter.anchoredPosition.x;
			Tween t = m_NotificationSwiperOuter.DOAnchorPosX(0f - x, 0.5f).SetEase(Ease.OutExpo);
			yield return t.WaitForCompletion();
			NotificationInfo notificationInfo = activeNotifications.Peek();
			bool appAlreadyOpen = m_CurrentApp.GetType() == notificationInfo.notification.GetAppType();
			OpenApp(notificationInfo.notification.GetAppType());
			PopNotification();
			TurnOn();
			yield return new WaitUntil(() => state == PhoneState.ON);
			m_CurrentApp.OpenContent(notificationInfo.unlockable, appAlreadyOpen);
			PeekNotification();
		}

		private IEnumerator NotificationMain(bool skipBeginAndEnd = false)
		{
			if (m_PhoneAnimation.IsPlaying())
			{
				yield return m_PhoneAnimation.WaitForCompletion();
			}
			m_NotificationSwiperMain.anchoredPosition = new Vector2(originalMainSwiperPosition.x, m_NotificationSwiperMain.anchoredPosition.y);
			m_NotificationMainScreen.gameObject.SetActive(value: true);
			SetPhoneUIEnabled(enableUI: true, openPhone: true);
			if (!skipBeginAndEnd)
			{
				m_NotificationMainScreen.anchoredPosition = m_NotifyMainPopupState.PhoneAngle;
				Tween t = m_NotificationMainScreen.DOAnchorPosY(m_NotifyMainPopupState.Position.y, 0.35f).SetEase(Ease.OutExpo);
				yield return t.WaitForCompletion();
			}
			audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_RingTone);
			if (activeNotifications.Count > 0)
			{
				NotificationInfo notificationInfo = activeNotifications.Peek();
				if (m_CurrentApp != null && m_CurrentApp.GetType() == notificationInfo.notification.GetAppType())
				{
					m_CurrentApp.OnAppRefresh();
				}
				bool canceledNotification = false;
				float timer = 0f;
				Vector3 originalPosition = m_PhoneOpenTransform.anchoredPosition;
				while (timer < phoneNotificationDuration && !gameInput.GetButtonNew(29) && !canceledNotification)
				{
					m_PhoneOpenTransform.anchoredPosition = originalPosition + new Vector3(UnityEngine.Random.Range(0f - phoneNotificationIntensity, phoneNotificationIntensity), UnityEngine.Random.Range(0f - phoneNotificationIntensity, phoneNotificationIntensity), 0f);
					timer += Core.dt;
					yield return null;
					canceledNotification |= gameInput.GetButtonNew(57);
				}
				timer = 0f;
				while (timer < phoneNotificationEndWait && !canceledNotification)
				{
					if (gameInput.GetButtonNew(29))
					{
						float x = m_NotificationSwiperMain.anchoredPosition.x;
						Tween t2 = m_NotificationSwiperMain.DOAnchorPosX(0f - x, 0.5f).SetEase(Ease.OutExpo);
						yield return t2.WaitForCompletion();
						App currentApp = m_CurrentApp;
						OpenApp(notificationInfo.notification.GetAppType());
						bool appAlreadyOpen = m_CurrentApp == currentApp;
						m_CurrentApp.OpenContent(notificationInfo.unlockable, appAlreadyOpen);
						break;
					}
					yield return null;
					timer += Core.dt;
					canceledNotification = gameInput.GetButtonNew(57);
				}
			}
			if (!PeekNextNotification())
			{
				Tween t3 = m_NotificationMainScreen.transform.DOLocalMoveY(m_NotifyMainPopupState.PhoneAngle.y, 0.35f).SetEase(Ease.OutExpo).SetDelay(0.5f);
				yield return t3.WaitForCompletion();
				m_NotificationMainScreen.gameObject.SetActive(value: false);
				PopNotification();
				PeekNotification();
			}
		}

		private void InitRingAnimation()
		{
			m_PhoneRingAnimation = DOTween.Sequence();
			m_PhoneRingAnimation.SetUpdate(UpdateType.Manual);
			m_PhoneRingAnimation.SetAutoKill(autoKillOnCompletion: false);
			m_PhoneRingAnimation.Pause();
			m_PhoneRingAnimation.Append(m_PhoneClosedTransform.DOAnchorPos(m_RingState.Position, 0.35f).SetEase(Ease.OutExpo).SetDelay(0.35f));
		}

		private void InitEnableOSSequence()
		{
			m_OSSequence = DOTween.Sequence();
			m_OSSequence.SetUpdate(UpdateType.Manual);
			m_OSSequence.SetAutoKill(autoKillOnCompletion: false);
			m_OSSequence.Pause();
			m_OSSequence.AppendCallback(() =>
			{
				state = PhoneState.BOOTINGUP;
			});
			m_OSSequence.AppendCallback(() =>
			{
				m_Screen.gameObject.SetActive(value: true);
			});
			m_OSSequence.AppendCallback(() =>
			{
				m_CurrentApp.OnAppEnable();
			});
			m_OSSequence.AppendCallback(() =>
			{
				state = PhoneState.ON;
			});
			m_OSSequence.AppendCallback(() =>
			{
				ScreenOverlay.color = Color.white;
			});
			m_OSSequence.AppendCallback(() =>
			{
				m_Screen.localScale = new Vector3(0f, 0.025f, 1f);
			});
			m_OSSequence.Append(m_Screen.DOScaleX(phoneScale, 0.5f)).SetEase(Ease.InExpo);
			m_OSSequence.Join(m_Screen.DOScaleY(phoneScale, 0.5f).SetDelay(0.35f)).SetEase(Ease.OutExpo);
			m_OSSequence.Join(ScreenOverlay.DOFade(0f, 0.35f).SetEase(Ease.Linear));
		}

		public void PushNotification(App app, string topBarText, AUnlockable unlockable)
		{
			Notification notification = app.Notification;
			if (!(notification != null))
			{
				return;
			}
			NotificationInfo item = new NotificationInfo
			{
				notification = notification,
				topBarText = topBarText,
				unlockable = unlockable
			};
			if (activeNotifications.Count == 0)
			{
				activeNotifications.Enqueue(item);
				PeekNotification();
				return;
			}
			bool flag = false;
			for (int i = 0; i < activeNotifications.Count; i++)
			{
				NotificationInfo notificationInfo = activeNotifications.Peek();
				flag |= notificationInfo.notification == notification && notificationInfo.topBarText.Equals(topBarText);
			}
			if (!flag)
			{
				activeNotifications.Enqueue(item);
			}
		}

		private void PopNotification()
		{
			if (activeNotifications.Count != 0)
			{
				activeNotifications.Dequeue().notification.HideNotification(showOnMainScreen);
				if (state == PhoneState.NOTIFICATION_INNER)
				{
					state = PhoneState.ON;
				}
				else if (state == PhoneState.NOTIFICATION_OUTER)
				{
					state = PhoneState.OFF;
				}
			}
		}

		public bool PeekNotification()
		{
			bool flag = false;
			if (CheckNotificationAllowed())
			{
				flag = true;
				showOnMainScreen = (state == PhoneState.ON) | (state == PhoneState.BOOTINGUP);
				state = (PhoneState)(4 + Convert.ToInt32(showOnMainScreen));
				NotificationInfo notificationInfo = activeNotifications.Peek();
				Notification notification = notificationInfo.notification;
				bool flag2 = false;
				if (showOnMainScreen)
				{
					flag2 = m_NotificationMainScreen.gameObject.activeInHierarchy;
					notification.ShowNotification(this, notificationInfo.topBarText, showOnMainScreen, flag2);
					m_NotificationMainScreen.gameObject.SetActive(value: true);
				}
				else
				{
					flag2 = closedPhoneCanvas.activeInHierarchy;
					SetPhoneUIEnabled(enableUI: true, openPhone: false);
					notification.ShowNotification(this, notificationInfo.topBarText, showOnMainScreen, flag2);
				}
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		public bool PeekNextNotification()
		{
			bool result = false;
			if (CheckNotificationAllowed() && activeNotifications.Count >= 2)
			{
				PopNotification();
				PeekNotification();
				result = true;
			}
			return result;
		}

		public bool CheckNotificationAllowed()
		{
			bool result = true;
			if (player.IsBusyWithSequence() || activeNotifications.Count == 0 || state == PhoneState.BEING_CALLED || state == PhoneState.IN_CALL || !m_PhoneAllowed)
			{
				result = false;
			}
			return result;
		}

		internal void RefreshCurrentApp()
		{
			if (m_CurrentApp != null)
			{
				m_CurrentApp.OnAppRefresh();
			}
		}

		internal bool IsCurrentAppAndOpen(Type appType)
		{
			bool num = m_CurrentApp.GetType() == appType;
			bool flag = state == PhoneState.ON || state == PhoneState.BOOTINGUP;
			return num & flag;
		}

		private void SetPhoneTransformState(RectTransform transformToSet, PhoneTransformState stateToSet)
		{
			transformToSet.rotation = Quaternion.Euler(stateToSet.PhoneAngle);
			transformToSet.anchoredPosition = stateToSet.Position;
		}
	}
}
