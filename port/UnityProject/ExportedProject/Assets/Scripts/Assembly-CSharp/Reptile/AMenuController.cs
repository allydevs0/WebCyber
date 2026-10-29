using System.Collections;
using UnityEngine;

namespace Reptile
{
	public abstract class AMenuController : MonoBehaviour, IMenuAnimationTransitionController, IUIMenuController, IUIElement
	{
		[SerializeField]
		protected GameObject firstMenuButton;

		[SerializeField]
		protected Menu menuType;

		protected GameObject lastSelectedButton;

		protected UIManager uIManager;

		protected AudioManager audioManager;

		protected TextMeshProMenuButton[] textMeshProMenuButtons;

		[SerializeField]
		protected bool allowMouseInput = true;

		[SerializeField]
		private bool alwaysShowOnTop;

		[SerializeField]
		private bool allowPauseMenu;

		[SerializeField]
		private bool allowPhone;

		[SerializeField]
		private bool rememberLastSelectedButton = true;

		[SerializeField]
		private UIAnimationController[] menuAnimations;

		[SerializeField]
		private UIAnimationController defaultAnimation;

		private UIAnimationController currentAnimation;

		private ITextSelectable[] textSelectables;

		private bool recreateTweens;

		public bool IsEnabled { get; set; }

		public bool AlwaysShowOnTop => alwaysShowOnTop;

		public Menu MenuType => menuType;

		public bool IsTransitioning
		{
			get
			{
				if (currentAnimation != null)
				{
					return currentAnimation.IsPlayingAnimation;
				}
				return false;
			}
		}

		public bool AllowPauseMenu => allowPauseMenu;

		public bool AllowPhone => allowPhone;

		public bool AllowMouseInput => allowMouseInput;

		public virtual void Activate()
		{
			base.gameObject.SetActive(value: true);
		}

		public virtual void Deactivate()
		{
			if (base.gameObject.activeInHierarchy)
			{
				ResetSelectableState();
				base.gameObject.SetActive(value: false);
			}
		}

		public virtual GameObject GetRoot()
		{
			return base.gameObject;
		}

		public virtual GameObject GetFirstFocusGameObject()
		{
			return firstMenuButton;
		}

		public virtual GameObject GetLastSelectedObject()
		{
			return lastSelectedButton;
		}

		public virtual IMenuAnimationTransitionController ShowMenu()
		{
			return this;
		}

		public virtual IMenuAnimationTransitionController HideMenu()
		{
			return this;
		}

		public void SetLastSelectedObject(GameObject lastSelectedObject)
		{
			if (rememberLastSelectedButton)
			{
				lastSelectedButton = lastSelectedObject;
			}
		}

		public void OnPressedBackButton()
		{
			if (!IsTransitioning)
			{
				HandlePressedBackButton();
			}
		}

		public void ShowOpeningMenuTransitionAnimation(Menu fromMenu)
		{
			currentAnimation = GetMenuAnimationController(fromMenu);
			if (currentAnimation == null)
			{
				EnableSelectablesAsRaycastTarget();
			}
			else if (recreateTweens)
			{
				recreateTweens = false;
				PlayAnimationAfterTweenRecreation(currentAnimation);
			}
			else
			{
				SetupAnimationListeners(currentAnimation);
				DisableSelectablesAsRaycastTarget();
				PlayAnimation(currentAnimation);
			}
		}

		public void ShowClosingMenuTransitionAnimation(Menu toMenu)
		{
			currentAnimation = GetMenuAnimationController(toMenu);
			if (!(currentAnimation == null))
			{
				SetupAnimationListeners(currentAnimation);
				DisableSelectablesAsRaycastTarget();
				PlayAnimationBackward(currentAnimation);
			}
		}

		public void PlayCustomMenuAnimation(int index)
		{
			currentAnimation = GetMenuAnimationController(index);
			if (!(currentAnimation == null))
			{
				if (recreateTweens)
				{
					recreateTweens = false;
					PlayAnimationAfterTweenRecreation(currentAnimation);
				}
				else
				{
					SetupAnimationListeners(currentAnimation);
					DisableSelectablesAsRaycastTarget();
					PlayAnimation(currentAnimation);
				}
			}
		}

		public void PlayCustomMenuAnimationBackwards(int index)
		{
			currentAnimation = GetMenuAnimationController(index);
			if (!(currentAnimation == null))
			{
				SetupAnimationListeners(currentAnimation);
				DisableSelectablesAsRaycastTarget();
				PlayAnimationBackward(currentAnimation);
			}
		}

		protected virtual void Awake()
		{
			textSelectables = base.gameObject.GetComponentsInChildren<ITextSelectable>();
			textMeshProMenuButtons = base.gameObject.GetComponentsInChildren<TextMeshProMenuButton>();
			Core.OnScreenSizeChanged += OnScreenSizeChanged;
			ListenForButtonPress();
		}

		protected virtual void Start()
		{
			uIManager = Core.Instance.UIManager;
			audioManager = Core.Instance.AudioManager;
		}

		protected virtual void OnDestroy()
		{
			Core.OnScreenSizeChanged -= OnScreenSizeChanged;
			StopListeningForButtonPress();
		}

		protected virtual void OnMenuAnimationStarted()
		{
		}

		protected virtual void HandlePressedBackButton()
		{
		}

		protected virtual void OnMenuAnimationEnded()
		{
			DestroyAnimationListeners(currentAnimation);
			EnableSelectablesAsRaycastTarget();
			currentAnimation = null;
		}

		private void ListenForButtonPress()
		{
			for (int i = 0; i < textMeshProMenuButtons.Length; i++)
			{
				if (textMeshProMenuButtons[i].UseDelayedButtonClick && textMeshProMenuButtons[i].DisableInputDuringDelay)
				{
					textMeshProMenuButtons[i].onClick.AddListener(OnMenuButtonPressed);
					textMeshProMenuButtons[i].onDelayedButtonClick.AddListener(OnMenuButtonPressedFinished);
				}
			}
		}

		private void StopListeningForButtonPress()
		{
			for (int i = 0; i < textMeshProMenuButtons.Length; i++)
			{
				if (textMeshProMenuButtons[i].UseDelayedButtonClick && textMeshProMenuButtons[i].DisableInputDuringDelay)
				{
					textMeshProMenuButtons[i].onClick.RemoveListener(OnMenuButtonPressed);
					textMeshProMenuButtons[i].onDelayedButtonClick.RemoveListener(OnMenuButtonPressedFinished);
				}
			}
		}

		private void SetupAnimationListeners(UIAnimationController menuAnimationController)
		{
			menuAnimationController.OnAnimationStarted += OnMenuAnimationStarted;
			menuAnimationController.OnAnimationEnded += OnMenuAnimationEnded;
		}

		private void DestroyAnimationListeners(UIAnimationController menuAnimationController)
		{
			menuAnimationController.OnAnimationStarted -= OnMenuAnimationStarted;
			menuAnimationController.OnAnimationEnded -= OnMenuAnimationEnded;
		}

		private UIAnimationController GetMenuAnimationController(int index)
		{
			if (index >= menuAnimations.Length)
			{
				return defaultAnimation;
			}
			return menuAnimations[index];
		}

		private UIAnimationController GetMenuAnimationController(Menu menu)
		{
			for (int i = 0; i < menuAnimations.Length; i++)
			{
				if (menuAnimations[i].useFromMenu == menu)
				{
					return menuAnimations[i];
				}
			}
			return defaultAnimation;
		}

		private void ResetSelectableState()
		{
			for (int i = 0; i < textSelectables.Length; i++)
			{
				textSelectables[i].ResetSelectableState();
			}
		}

		private void EnableSelectablesAsRaycastTarget()
		{
			for (int i = 0; i < textSelectables.Length; i++)
			{
				textSelectables[i].EnableSelectableAsRaycastTarget();
			}
		}

		private void DisableSelectablesAsRaycastTarget()
		{
			for (int i = 0; i < textSelectables.Length; i++)
			{
				textSelectables[i].DisableSelectableAsRaycastTarget();
			}
		}

		private void OnScreenSizeChanged(int newWidth, int newHeight)
		{
			if (!base.gameObject.activeSelf)
			{
				recreateTweens = true;
			}
			else if (menuAnimations.Length != 0)
			{
				StartCoroutine(RecreateAllTweensInActiveMenu());
			}
		}

		private IEnumerator RecreateAllTweensInActiveMenu()
		{
			yield return null;
			CompleteAllAnimations();
			RecreateAllTweens();
			CompleteDefaultAnimation();
		}

		private void RecreateAllTweens()
		{
			for (int i = 0; i < menuAnimations.Length; i++)
			{
				menuAnimations[i].RecreateAllTweens();
			}
		}

		private void CompleteAllAnimations()
		{
			for (int i = 0; i < menuAnimations.Length; i++)
			{
				menuAnimations[i].CompleteAllTweens();
			}
		}

		private void CompleteDefaultAnimation()
		{
			defaultAnimation.CompleteAllTweens();
		}

		private void PlayAnimationAfterTweenRecreation(UIAnimationController menuAnimationController)
		{
			StartCoroutine(WaitForTweenReinitialization(menuAnimationController));
		}

		private IEnumerator WaitForTweenReinitialization(UIAnimationController menuAnimationController)
		{
			yield return RecreateAllTweenNextFrame();
			SetupAnimationListeners(menuAnimationController);
			DisableSelectablesAsRaycastTarget();
			PlayAnimation(menuAnimationController);
		}

		private IEnumerator RecreateAllTweenNextFrame()
		{
			yield return null;
			CompleteAllAnimations();
			RecreateAllTweens();
		}

		private void PlayAnimation(UIAnimationController menuAnimationController)
		{
			menuAnimationController.PlayAnimation();
		}

		private void PlayAnimationBackward(UIAnimationController menuAnimationController)
		{
			menuAnimationController.PlayAnimationBackward();
		}

		private void OnMenuButtonPressed()
		{
			DisableSelectablesAsRaycastTarget();
		}

		private void OnMenuButtonPressedFinished()
		{
			if (currentAnimation == null)
			{
				EnableSelectablesAsRaycastTarget();
			}
		}
	}
}
