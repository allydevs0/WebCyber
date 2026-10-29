using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class AOptionsMenuTab : MonoBehaviour, IUIElement
	{
		[SerializeField]
		private GameObject settingsContainer;

		[SerializeField]
		private UIAnimationController optionsTabAnimation;

		[SerializeField]
		private GameObject firstSelectable;

		[SerializeField]
		private GameObject lastSelectable;

		[SerializeField]
		private Button openTabButton;

		[SerializeField]
		private GameObject tabButtonContainer;

		[SerializeField]
		private bool allowMouseInput;

		protected SaveManager saveManager;

		protected bool isActive;

		private ITextSelectable[] textSelectables;

		public bool IsEnabled
		{
			get
			{
				return base.gameObject.activeSelf;
			}
			set
			{
			}
		}

		public bool AllowMouseInput => allowMouseInput;

		public bool IsShowingAnimation
		{
			get
			{
				if (optionsTabAnimation != null)
				{
					return optionsTabAnimation.IsPlayingAnimation;
				}
				return false;
			}
		}

		public bool HasAnimation => optionsTabAnimation != null;

		public Button OpenTabButton => openTabButton;

		protected virtual void Awake()
		{
			saveManager = Core.Instance.SaveManager;
			textSelectables = base.gameObject.GetComponentsInChildren<ITextSelectable>();
		}

		public virtual void Activate()
		{
			isActive = true;
			base.gameObject.SetActive(value: true);
		}

		public virtual void Deactivate()
		{
			if (saveManager != null && isActive)
			{
				saveManager.SaveSettings();
			}
			isActive = false;
			HideSettingsContainer();
			ResetButtonsSelectableState();
			base.gameObject.SetActive(value: false);
		}

		public void ShowOpeningAnimation()
		{
			if (HasAnimation)
			{
				PlayOpenAnimation();
			}
		}

		public void ShowClosingAnimation()
		{
			if (HasAnimation)
			{
				PlayClosingAnimation();
				HideSettingsContainer();
			}
		}

		public GameObject GetFirstFocusGameObject()
		{
			return firstSelectable.gameObject;
		}

		public GameObject GetLastSelectable()
		{
			return lastSelectable.gameObject;
		}

		public GameObject GetRoot()
		{
			return base.gameObject;
		}

		public void ShowSettingsContainer()
		{
			settingsContainer.SetActive(value: true);
		}

		public void HideSettingsContainer()
		{
			settingsContainer.SetActive(value: false);
		}

		public void HideTabButtonContainer()
		{
			tabButtonContainer.SetActive(value: false);
		}

		private void PlayOpenAnimation()
		{
			optionsTabAnimation.PlayAnimation();
			optionsTabAnimation.OnAnimationEnded += OnOpeningAnimationEnded;
		}

		private void OnOpeningAnimationEnded()
		{
			optionsTabAnimation.OnAnimationEnded -= OnOpeningAnimationEnded;
			ShowSettingsContainer();
		}

		private void PlayClosingAnimation()
		{
			optionsTabAnimation.PlayAnimationBackward();
			optionsTabAnimation.OnAnimationEnded += OnClosingAnimationEnded;
		}

		private void OnClosingAnimationEnded()
		{
			optionsTabAnimation.OnAnimationEnded -= OnClosingAnimationEnded;
			Deactivate();
		}

		private void ResetButtonsSelectableState()
		{
			for (int i = 0; i < textSelectables.Length; i++)
			{
				textSelectables[i].ResetSelectableState();
			}
		}
	}
}
