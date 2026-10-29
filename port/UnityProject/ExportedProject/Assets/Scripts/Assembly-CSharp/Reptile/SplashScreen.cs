using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class SplashScreen : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI introTextField;

		[SerializeField]
		private TMProLocalizationAddOn introTextFieldLocalization;

		[SerializeField]
		private GameObject introTextContainer;

		[SerializeField]
		private GameObject logoContainer;

		[SerializeField]
		private GameObject splashScreenContainer;

		[SerializeField]
		private Image splashScreenFadeInImage;

		[SerializeField]
		private Image splashScreenFadeOutImage;

		[SerializeField]
		private UIAnimationController logoFadeInAnimation;

		[SerializeField]
		private UIAnimationController logoFadeOutAnimation;

		[SerializeField]
		private UIAnimationController splashScreenFadeInAnimation;

		[SerializeField]
		private UIAnimationController splashScreenFadeOutAnimation;

		public float blinkInterval = 1f;

		public float blinkIntervalOnPress = 0.075f;

		public float blinkCount = 6f;

		private float blinkTimer;

		private BaseModule baseModule;

		private GameInput gameInput;

		private UIManager uIManager;

		private AGameDebug gameDebug;

		private Coroutine blinkCoroutine;

		private bool canLoginUser = true;

		private bool isCoreInitialized;

		private readonly List<int> anyButtonDownBlacklist = new List<int>
		{
			13, 14, 0, 1, 42, 43, 44, 60, 61, 5,
			6, 40
		};

		private void Awake()
		{
			if (Core.Instance == null)
			{
				Core.OnCoreInitialized += OnCoreInitialized;
			}
			else
			{
				OnCoreInitialized();
			}
			logoContainer.SetActive(value: true);
			splashScreenContainer.SetActive(value: false);
			StartAnimation(logoFadeInAnimation, OnLogoFadeInEnded);
		}

		private void OnCoreInitialized()
		{
			isCoreInitialized = true;
			Core.OnCoreInitialized -= OnCoreInitialized;
			gameInput = Core.Instance.GameInput;
			baseModule = Core.Instance.BaseModule;
			uIManager = Core.Instance.UIManager;
			gameDebug = Core.Instance.gameDebug;
			uIManager.SetLocalizationValueOnUIText(introTextField, Core.Instance.Localizer);
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(1);
		}

		private void OnDestroy()
		{
			if (isCoreInitialized)
			{
				baseModule.OnUserLoginFinished -= OnUserLoginFinished;
			}
			RemoveRewiredInputListener();
			logoFadeInAnimation.OnAnimationEnded -= OnLogoFadeInEnded;
			logoFadeOutAnimation.OnAnimationEnded -= OnLogoFadeOutEnded;
			splashScreenFadeInAnimation.OnAnimationEnded -= OnSplashScreenFadeInEnded;
			splashScreenFadeOutAnimation.OnAnimationEnded -= OnSplashScreenFadeOutEnded;
			Core.OnCoreInitialized -= OnCoreInitialized;
		}

		private void StartAnimation(UIAnimationController newAnimation, UIAnimationController.AnimationStartedDelegate newListener)
		{
			newAnimation.OnAnimationEnded += newListener;
			newAnimation.PlayAnimation();
		}

		private void RemoveRewiredInputListener()
		{
			if (isCoreInitialized && gameInput.IsRewiredInitialized && gameInput.FirstRewiredPlayer != null)
			{
				gameInput.FirstRewiredPlayer.RemoveInputEventDelegate(OnAnyKeyPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
			}
		}

		private void OnLogoFadeInEnded()
		{
			logoFadeInAnimation.OnAnimationEnded -= OnLogoFadeInEnded;
			StartCoroutine(WaitForCoreToLoad());
		}

		private IEnumerator WaitForCoreToLoad()
		{
			while (!isCoreInitialized)
			{
				yield return null;
			}
			StartAnimation(logoFadeOutAnimation, OnLogoFadeOutEnded);
		}

		private void OnLogoFadeOutEnded()
		{
			logoFadeOutAnimation.OnAnimationEnded -= OnLogoFadeOutEnded;
			StartAnimation(splashScreenFadeInAnimation, OnSplashScreenFadeInEnded);
			logoContainer.SetActive(value: false);
			splashScreenContainer.SetActive(value: true);
			introTextContainer.SetActive(value: true);
			AddRewiredInputListener();
		}

		private void OnSplashScreenFadeInEnded()
		{
			splashScreenFadeInAnimation.OnAnimationEnded -= OnSplashScreenFadeInEnded;
		}

		private void AddRewiredInputListener()
		{
			if (gameInput.IsRewiredInitialized)
			{
				gameInput.FirstRewiredPlayer.AddInputEventDelegate(OnAnyKeyPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
			}
		}

		private void OnAnyKeyPressed(InputActionEventData data)
		{
			if (!gameDebug.IsShowingDebugMenu && !Core.Instance.Platform.ErrorHandler.IsHandlingError && !anyButtonDownBlacklist.Contains(data.actionId))
			{
				DelayedContinueToGame();
			}
		}

		private void DelayedContinueToGame()
		{
			if (blinkCoroutine == null)
			{
				blinkCoroutine = StartCoroutine(BlinkButtonAndContinue());
			}
		}

		private void ContinueToGame()
		{
			RemoveRewiredInputListener();
			StartAnimation(splashScreenFadeOutAnimation, OnSplashScreenFadeOutEnded);
		}

		private void Update()
		{
			if (blinkCoroutine == null)
			{
				blinkTimer += Core.dt;
				if (blinkTimer > blinkInterval)
				{
					blinkTimer -= blinkInterval;
					introTextContainer.SetActive(!introTextContainer.activeSelf);
				}
			}
			if (!isCoreInitialized)
			{
				UpdateDotTween();
			}
		}

		private void UpdateDotTween()
		{
			DOTween.ManualUpdate(Core.dt, 1f / 60f);
		}

		private IEnumerator BlinkButtonAndContinue()
		{
			bool containerActive = introTextContainer.activeSelf;
			float targetTime = blinkIntervalOnPress * blinkCount;
			for (float timer = 0f; timer < targetTime; timer += blinkIntervalOnPress + Core.dt)
			{
				containerActive = !containerActive;
				introTextContainer.SetActive(containerActive);
				yield return new WaitForSeconds(blinkIntervalOnPress);
			}
			ContinueToGame();
			blinkCoroutine = null;
		}

		private void OnSplashScreenFadeOutEnded()
		{
			LoginUser();
		}

		private void LoginUser()
		{
			if (canLoginUser)
			{
				baseModule.OnUserLoginFinished += OnUserLoginFinished;
				baseModule.StartLoginAfterIntro();
				canLoginUser = false;
			}
		}

		private void OnUserLoginFinished(bool success, AUser user)
		{
			baseModule.OnUserLoginFinished -= OnUserLoginFinished;
			canLoginUser = true;
			if (!success)
			{
				ResetSplashScreenFade();
			}
		}

		private void ResetSplashScreenFade()
		{
			splashScreenFadeInImage.color = new Color(splashScreenFadeInImage.color.r, splashScreenFadeInImage.color.g, splashScreenFadeInImage.color.b, 1f);
			splashScreenFadeOutImage.color = new Color(splashScreenFadeOutImage.color.r, splashScreenFadeOutImage.color.g, splashScreenFadeOutImage.color.b, 0f);
			StartAnimation(splashScreenFadeInAnimation, OnSplashScreenFadeInEnded);
			AddRewiredInputListener();
		}
	}
}
