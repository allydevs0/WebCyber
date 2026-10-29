using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class OptionsMenu : AMenuController
	{
		private const int FIRST_TAB = 0;

		[SerializeField]
		private AOptionsMenuTab firstOptionsMenuTab;

		[SerializeField]
		private AOptionsMenuTab videoSettingsOptionsMenuTab;

		[SerializeField]
		private TextMeshProMenuButton backButton;

		[SerializeField]
		private GameObject[] tabActivationButtons;

		[SerializeField]
		private AOptionsMenuTab[] allTabs;

		[SerializeField]
		private AOptionsMenuTab[] consoleTabs;

		private AOptionsMenuTab[] availableTabs;

		private AOptionsMenuTab currentActiveOptionsMenuTab;

		private GameInput gameInput;

		private int currentSelectedOptionsMenuTabIndex;

		private int currentActiveOptionsMenuTabIndex;

		public override GameObject GetFirstFocusGameObject()
		{
			if (availableTabs[0] == null)
			{
				return base.GetFirstFocusGameObject();
			}
			currentSelectedOptionsMenuTabIndex = currentActiveOptionsMenuTabIndex;
			return availableTabs[currentActiveOptionsMenuTabIndex].OpenTabButton.gameObject;
		}

		public override void Activate()
		{
			base.Activate();
			DeactivateAllAvailableTabs();
			currentSelectedOptionsMenuTabIndex = 0;
			currentActiveOptionsMenuTabIndex = 0;
		}

		public override void Deactivate()
		{
			if (currentActiveOptionsMenuTab != null)
			{
				currentActiveOptionsMenuTab.Deactivate();
			}
			base.Deactivate();
		}

		public override IMenuAnimationTransitionController ShowMenu()
		{
			ShowNewOptionsTab(firstOptionsMenuTab);
			SetMenuButtonsNavigation(currentActiveOptionsMenuTab);
			return base.ShowMenu();
		}

		public override IMenuAnimationTransitionController HideMenu()
		{
			HideCurrentOptionsTab();
			return base.HideMenu();
		}

		public void ShowTab(AOptionsMenuTab optionsMenuTab)
		{
			currentSelectedOptionsMenuTabIndex = GetOptionsMenuTabIndex(optionsMenuTab);
			SwitchOptionsTab(optionsMenuTab);
		}

		public void Close()
		{
			uIManager.ShowPreviousMenu();
		}

		protected override void Awake()
		{
			base.Awake();
			gameInput = Core.Instance.GameInput;
			availableTabs = allTabs;
			currentActiveOptionsMenuTab = availableTabs[0];
			currentActiveOptionsMenuTabIndex = 0;
			currentSelectedOptionsMenuTabIndex = 0;
			SetupOpenTabButtonNavigation();
			SetAllAvailableTabsActive();
		}

		protected void Update()
		{
			if (!base.IsTransitioning)
			{
				UpdateInput();
			}
		}

		protected override void HandlePressedBackButton()
		{
			base.HandlePressedBackButton();
			if (backButton != null)
			{
				if (EventSystem.current.sendNavigationEvents)
				{
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
				}
				backButton.ShowButtonPress(playSound: false);
			}
			else
			{
				BackButtonDelayedClick(playSound: true);
			}
		}

		public void BackButtonDelayedClick(bool playSound = false)
		{
			if (playSound)
			{
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
			}
			Close();
		}

		private void DeactivateAllAvailableTabs()
		{
			for (int i = 0; i < availableTabs.Length; i++)
			{
				availableTabs[i].Deactivate();
			}
		}

		private void HideCurrentOptionsTab()
		{
			if (!(currentActiveOptionsMenuTab == null))
			{
				HideOptionsTab(currentActiveOptionsMenuTab);
			}
		}

		private void HideOptionsTab(AOptionsMenuTab optionsMenuTab)
		{
			if (optionsMenuTab.HasAnimation)
			{
				optionsMenuTab.ShowClosingAnimation();
			}
			else
			{
				optionsMenuTab.Deactivate();
			}
		}

		private int GetOptionsMenuTabIndex(AOptionsMenuTab optionsMenuTab)
		{
			for (int i = 0; i < availableTabs.Length; i++)
			{
				if (optionsMenuTab.Equals(availableTabs[i]))
				{
					return i;
				}
			}
			return 0;
		}

		private void SwitchOptionsTab(AOptionsMenuTab newOptionsTab)
		{
			if (!currentActiveOptionsMenuTab.Equals(newOptionsTab))
			{
				HideCurrentOptionsTab();
				ShowNewOptionsTab(newOptionsTab);
				SetMenuButtonsNavigation(currentActiveOptionsMenuTab);
			}
		}

		private void ShowNewOptionsTab(AOptionsMenuTab optionsMenuTab)
		{
			optionsMenuTab.Activate();
			if (optionsMenuTab.HasAnimation)
			{
				optionsMenuTab.ShowOpeningAnimation();
			}
			else
			{
				optionsMenuTab.ShowSettingsContainer();
			}
			currentActiveOptionsMenuTab = optionsMenuTab;
			currentActiveOptionsMenuTabIndex = currentSelectedOptionsMenuTabIndex;
		}

		private void SetMenuButtonsNavigation(AOptionsMenuTab optionsMenuTab)
		{
			SetTabActivationButtonsNavigation(optionsMenuTab);
			SetBackButtonNavigation(optionsMenuTab);
		}

		private void SetTabActivationButtonsNavigation(AOptionsMenuTab optionsMenuTab)
		{
			GameObject firstFocusGameObject = optionsMenuTab.GetFirstFocusGameObject();
			if (!(firstFocusGameObject == null))
			{
				Selectable component = firstFocusGameObject.GetComponent<Selectable>();
				for (int i = 0; i < tabActivationButtons.Length; i++)
				{
					Selectable component2 = tabActivationButtons[i].GetComponent<Selectable>();
					SetButtonNavigation(tabActivationButtons[i], component2.navigation.selectOnUp, component, component2.navigation.selectOnLeft, component2.navigation.selectOnRight);
				}
			}
		}

		private void SetBackButtonNavigation(AOptionsMenuTab optionsMenuTab)
		{
			GameObject lastSelectable = optionsMenuTab.GetLastSelectable();
			if (!(lastSelectable == null))
			{
				Selectable component = lastSelectable.GetComponent<Selectable>();
				SetButtonNavigation(backButton.gameObject, component, null, null, null);
			}
		}

		private void SetButtonNavigation(GameObject buttonToSetNavigation, Selectable onUp, Selectable onDown, Selectable onLeft, Selectable onRight)
		{
			Selectable component = buttonToSetNavigation.GetComponent<Selectable>();
			if (!(component == null))
			{
				Navigation navigation = component.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnDown = onDown;
				navigation.selectOnUp = onUp;
				navigation.selectOnLeft = onLeft;
				navigation.selectOnRight = onRight;
				component.navigation = navigation;
			}
		}

		private void SetupOpenTabButtonNavigation()
		{
			for (int i = 0; i < availableTabs.Length; i++)
			{
				Selectable openTabButton = availableTabs[i].OpenTabButton;
				Selectable onRight = null;
				Selectable onLeft = null;
				if (i + 1 < availableTabs.Length)
				{
					onRight = availableTabs[i + 1].OpenTabButton;
				}
				if (i - 1 >= 0)
				{
					onLeft = availableTabs[i - 1].OpenTabButton;
				}
				SetButtonNavigation(openTabButton.gameObject, openTabButton.navigation.selectOnUp, openTabButton.navigation.selectOnDown, onLeft, onRight);
			}
		}

		private void SetAllAvailableTabsActive()
		{
			for (int i = 0; i < availableTabs.Length; i++)
			{
				availableTabs[i].gameObject.SetActive(value: true);
			}
		}

		private void SelectOpenButtonTabButton(AOptionsMenuTab currentTab)
		{
			uIManager.SetSelectedGameObject(currentTab.OpenTabButton.gameObject);
		}

		private void UpdateInput()
		{
			if (!currentActiveOptionsMenuTab.IsShowingAnimation)
			{
				if (gameInput.GetButtonNew(45))
				{
					SelectPreviousOptionsTab();
				}
				else if (gameInput.GetButtonNew(47))
				{
					SelectNextOptionsTab();
				}
			}
		}

		private void SelectPreviousOptionsTab()
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (!(currentSelectedGameObject == null))
			{
				SetIndexToSelectedTab(currentSelectedGameObject);
				if (currentSelectedOptionsMenuTabIndex > 0)
				{
					currentSelectedOptionsMenuTabIndex--;
					SelectOpenButtonTabButton(availableTabs[currentSelectedOptionsMenuTabIndex]);
				}
			}
		}

		private void SelectNextOptionsTab()
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (!(currentSelectedGameObject == null))
			{
				SetIndexToSelectedTab(currentSelectedGameObject);
				if (currentSelectedOptionsMenuTabIndex < availableTabs.Length - 1)
				{
					currentSelectedOptionsMenuTabIndex++;
					SelectOpenButtonTabButton(availableTabs[currentSelectedOptionsMenuTabIndex]);
				}
			}
		}

		private void SetIndexToSelectedTab(GameObject currentSelectedGameObject)
		{
			int num = currentActiveOptionsMenuTabIndex;
			for (int i = 0; i < availableTabs.Length; i++)
			{
				if (currentSelectedGameObject.Equals(availableTabs[i].OpenTabButton.gameObject))
				{
					num = i;
					break;
				}
			}
			currentSelectedOptionsMenuTabIndex = num;
		}
	}
}
