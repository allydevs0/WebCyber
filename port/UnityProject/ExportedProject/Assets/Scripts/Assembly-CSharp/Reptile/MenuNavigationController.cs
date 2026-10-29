using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class MenuNavigationController : MonoBehaviour
	{
		private enum TransitionType
		{
			NONE = 0,
			ADDING = 1,
			CLOSING = 2,
			ENABLING = 3,
			DISABLING = 4
		}

		private struct MenuTransitionState
		{
			public IUIMenuController menuController;

			public TransitionType transitionType;

			public bool IsValid => menuController == null;

			public MenuTransitionState(IUIMenuController menuController, TransitionType transitionType)
			{
				this.menuController = menuController;
				this.transitionType = transitionType;
			}
		}

		private const int activeUIMax = 5;

		private readonly MenuTransitionState DEFAULT_MENU_TRANSITION_STATE;

		private int activeUIIndex = 5;

		private int phoneAllowControlIndex;

		private IUIMenuController[] activeUIMenu = new IUIMenuController[5];

		private Queue<MenuTransitionState> menuTransitionQueue;

		private MenuTransitionState currentMenuTransitionState;

		private EventSystem eventSystem;

		private GameInput gameInput;

		public bool IsShowingAnyMenu => activeUIIndex < 5;

		public bool IsTransitioning => currentMenuTransitionState.transitionType != TransitionType.NONE;

		public bool CurrentMenuAllowsPauseMenu
		{
			get
			{
				if (!IsShowingAnyMenu)
				{
					return true;
				}
				return activeUIMenu[activeUIIndex].AllowPauseMenu;
			}
		}

		public bool CurrentMenuAllowsMouseInput
		{
			get
			{
				if (!IsShowingAnyMenu)
				{
					return false;
				}
				return activeUIMenu[activeUIIndex].AllowMouseInput;
			}
		}

		public void Init(EventSystem eventSystem, GameInput gameInput)
		{
			this.eventSystem = eventSystem;
			this.gameInput = gameInput;
			menuTransitionQueue = new Queue<MenuTransitionState>();
		}

		public bool IsMenuTopMenu(IUIMenuController menu)
		{
			if (!IsShowingAnyMenu)
			{
				return false;
			}
			return activeUIMenu[activeUIIndex].Equals(menu);
		}

		public bool IsTransitioningMenu(IUIMenuController menu)
		{
			if (!IsTransitioning)
			{
				return false;
			}
			if (currentMenuTransitionState.menuController.Equals(menu))
			{
				return true;
			}
			MenuTransitionState[] array = menuTransitionQueue.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].menuController.Equals(menu))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsMenuPartOfMenuStack(IUIMenuController menu)
		{
			if (!IsShowingAnyMenu)
			{
				return false;
			}
			for (int i = activeUIIndex; i < 5; i++)
			{
				if (activeUIMenu[i].Equals(menu))
				{
					return true;
				}
			}
			return false;
		}

		public void PushNewMenu(IUIMenuController newMenu)
		{
			DisableUINavigation();
			MenuTransitionState menuTransitionState = AddToQueue(newMenu, TransitionType.ADDING);
			StartCoroutine(ShowNewMenuASync(menuTransitionState));
		}

		public void PushNewMenuInstant(IUIMenuController newMenu)
		{
			if (activeUIIndex < 5)
			{
				activeUIMenu[activeUIIndex].SetLastSelectedObject(eventSystem.currentSelectedGameObject);
			}
			ClearButtonSelection();
			AddNewMenu(newMenu);
			EnableMenu(newMenu);
			ForceFocusOnTopMenu();
			EnableUINavigation();
		}

		public void PopCurrentMenu()
		{
			IUIMenuController iUIMenuController = activeUIMenu[activeUIIndex];
			iUIMenuController.SetLastSelectedObject(null);
			DisableUINavigation();
			MenuTransitionState menuTransitionState = AddToQueue(iUIMenuController, TransitionType.CLOSING);
			StartCoroutine(CloseMenuASync(menuTransitionState));
		}

		public void PopMenu(IUIMenuController menu)
		{
			if (currentMenuTransitionState.menuController.Equals(menu))
			{
				menu.SetLastSelectedObject(null);
				DisableUINavigation();
				MenuTransitionState menuTransitionState = AddToQueue(menu, TransitionType.CLOSING);
				StartCoroutine(CloseMenuASync(menuTransitionState));
			}
		}

		public void DisableCurrentMenu()
		{
			IUIMenuController iUIMenuController = activeUIMenu[activeUIIndex];
			iUIMenuController.SetLastSelectedObject(eventSystem.currentSelectedGameObject);
			DisableUINavigation();
			MenuTransitionState menuTransitionState = AddToQueue(iUIMenuController, TransitionType.DISABLING);
			StartCoroutine(DisableMenuASync(menuTransitionState));
		}

		public void PopCurrentMenuInstant()
		{
			IUIMenuController iUIMenuController = activeUIMenu[activeUIIndex];
			iUIMenuController.SetLastSelectedObject(null);
			ClearButtonSelection();
			RemoveMenu(iUIMenuController);
			DisableMenu(iUIMenuController);
			SetupUINavigation();
		}

		public void PopAllMenusInstant()
		{
			for (int i = activeUIIndex; i < 5; i++)
			{
				IUIMenuController iUIMenuController = activeUIMenu[i];
				RemoveMenu(iUIMenuController);
				DisableMenu(iUIMenuController);
			}
		}

		public void ShowPreviousMenu()
		{
			if (activeUIIndex >= 4)
			{
				PopCurrentMenu();
				return;
			}
			IUIMenuController iUIMenuController = activeUIMenu[activeUIIndex];
			IUIMenuController previousMenu = activeUIMenu[activeUIIndex + 1];
			iUIMenuController.SetLastSelectedObject(null);
			DisableUINavigation();
			MenuTransitionState menuTransitionState = AddToQueue(iUIMenuController, TransitionType.CLOSING);
			StartCoroutine(ShowPreviousMenuASync(menuTransitionState, previousMenu));
		}

		public void ShowPreviousMenuInstant()
		{
			if (activeUIMenu != null)
			{
				if (!IsShowingAnyMenu)
				{
					gameInput.SetGameCursorMode();
					return;
				}
				if (activeUIIndex >= 4)
				{
					PopCurrentMenuInstant();
					return;
				}
				IUIMenuController iUIMenuController = activeUIMenu[activeUIIndex];
				IUIMenuController menuToActivate = activeUIMenu[activeUIIndex + 1];
				iUIMenuController.SetLastSelectedObject(null);
				RemoveMenu(iUIMenuController);
				DisableMenu(iUIMenuController);
				EnableMenu(menuToActivate);
				SetupUINavigation();
				currentMenuTransitionState = DEFAULT_MENU_TRANSITION_STATE;
			}
		}

		public void AbsolutePopIfPresent(IUIMenuController menuRootObject)
		{
			if (activeUIMenu != null && IsShowingAnyMenu)
			{
				menuRootObject.SetLastSelectedObject(null);
				RemoveMenu(menuRootObject);
				DisableMenu(menuRootObject);
			}
		}

		public void PushNewMenuEndOfFrame(IUIMenuController newMenu)
		{
			StartCoroutine(PushNewMenuDelayed(newMenu));
		}

		public void PopCurrentMenuEndOfFrame()
		{
			StartCoroutine(PopCurrentMenuDelayed(activeUIIndex));
		}

		public void SetSelectedGameObject(GameObject button)
		{
			if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.Equals(button))
			{
				ForceHighlightOnGameObjectWithButton(button);
			}
			else
			{
				eventSystem.SetSelectedGameObject(button);
			}
		}

		public void ForceFocusOnTopMenu()
		{
			if (IsShowingAnyMenu)
			{
				ForceFocusOnMenu(activeUIMenu[activeUIIndex]);
			}
		}

		public void RemoveMenuIfJustPopped(IUIMenuController menuToCheck)
		{
			if (IsShowingAnyMenu && activeUIMenu[activeUIIndex] == menuToCheck && !menuToCheck.IsEnabled)
			{
				activeUIMenu[activeUIIndex] = null;
				activeUIIndex++;
			}
		}

		public void OnPressedBackButton()
		{
			if (IsShowingAnyMenu && !IsTransitioning)
			{
				activeUIMenu[activeUIIndex].OnPressedBackButton();
			}
		}

		public void Clear()
		{
			gameInput.SetGameCursorMode();
			if (IsShowingAnyMenu)
			{
				for (int i = activeUIIndex; i < 5; i++)
				{
					DisableMenu(activeUIMenu[activeUIIndex]);
					RemoveMenu(activeUIMenu[activeUIIndex]);
				}
			}
		}

		private void Update()
		{
			UpdateMouseState();
		}

		private void UpdateMouseState()
		{
			if (IsShowingAnyMenu)
			{
				if (gameInput.IsCursorVisible)
				{
					UpdateVisibleMouseState();
				}
				else
				{
					UpdateInvisibleMouseState();
				}
			}
		}

		private void UpdateVisibleMouseState()
		{
			if (!(Vector2.SqrMagnitude(gameInput.GetMouseDelta()) > 0f))
			{
				IUIMenuController menuController = activeUIMenu[activeUIIndex];
				DetermineMenuMouseState(menuController);
			}
		}

		private void UpdateInvisibleMouseState()
		{
			if (!gameInput.GetMouseDelta().Equals(Vector2.zero))
			{
				IUIMenuController menuController = activeUIMenu[activeUIIndex];
				DetermineMenuMouseState(menuController);
			}
		}

		private void OnDestroy()
		{
			activeUIMenu = null;
			activeUIIndex = 5;
			menuTransitionQueue.Clear();
			menuTransitionQueue = null;
			StopAllCoroutines();
		}

		private void DisableUINavigation()
		{
			eventSystem.sendNavigationEvents = false;
		}

		private void EnableUINavigation()
		{
			eventSystem.sendNavigationEvents = true;
		}

		private void EnableMenu(IUIMenuController menuToActivate)
		{
			menuToActivate.Activate();
			if (IsMenuTopMenu(menuToActivate))
			{
				menuToActivate.IsEnabled = true;
			}
		}

		private void DisableMenu(IUIMenuController menuToDeactivate)
		{
			menuToDeactivate.IsEnabled = false;
			menuToDeactivate.Deactivate();
		}

		private void SetupUINavigation()
		{
			if (IsShowingAnyMenu)
			{
				EnableUINavigation();
				ForceFocusOnTopMenu();
			}
			else
			{
				gameInput.SetGameCursorMode();
			}
		}

		private void RemoveMenu(IUIMenuController menuToCheck)
		{
			int num = 5;
			int num2 = 0;
			for (int num3 = 4; num3 >= activeUIIndex; num3--)
			{
				if (activeUIMenu[num3].Equals(menuToCheck))
				{
					activeUIMenu[num3] = null;
					num = num3;
					num2++;
				}
				else if (num != 5)
				{
					activeUIMenu[num3 + 1] = activeUIMenu[num3];
					activeUIMenu[num3] = null;
				}
			}
			if (num == 5)
			{
				return;
			}
			int num4 = activeUIIndex + num2;
			if (num == activeUIIndex && num4 != 5)
			{
				activeUIMenu[num4].IsEnabled = true;
			}
			activeUIIndex = num4;
			if (num != phoneAllowControlIndex)
			{
				return;
			}
			phoneAllowControlIndex = 0;
			WorldHandler instance = WorldHandler.instance;
			if (instance != null && (instance.currentEncounter == null || instance.currentEncounter.allowPhone))
			{
				Player currentPlayer = instance.GetCurrentPlayer();
				if (currentPlayer != null && !currentPlayer.inGraffitiGame)
				{
					currentPlayer.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
				}
			}
		}

		private MenuTransitionState AddToQueue(IUIMenuController nextMenuToHandle, TransitionType transitionType)
		{
			MenuTransitionState menuTransitionState = new MenuTransitionState(nextMenuToHandle, transitionType);
			if (!IsTransitioning && menuTransitionQueue.Count == 0)
			{
				currentMenuTransitionState = menuTransitionState;
			}
			else
			{
				menuTransitionQueue.Enqueue(menuTransitionState);
			}
			return menuTransitionState;
		}

		private IEnumerator ShowNewMenuASync(MenuTransitionState menuTransitionState)
		{
			yield return UpdateQueue(menuTransitionState);
			AddNewMenu(menuTransitionState.menuController);
			EnableMenu(menuTransitionState.menuController);
			yield return TransitionToNextMenu(menuTransitionState.menuController);
		}

		private IEnumerator UpdateQueue(MenuTransitionState menuTransitionState)
		{
			while (!currentMenuTransitionState.Equals(menuTransitionState))
			{
				yield return null;
			}
		}

		private void EvaluateNextInQueue()
		{
			if (menuTransitionQueue.Count > 0)
			{
				currentMenuTransitionState = menuTransitionQueue.Dequeue();
			}
			else
			{
				currentMenuTransitionState = DEFAULT_MENU_TRANSITION_STATE;
			}
		}

		private IEnumerator TransitionToNextMenu(IUIMenuController nextMenu)
		{
			IMenuAnimationTransitionController menuAnimationTransitionController = nextMenu.ShowMenu();
			if (menuAnimationTransitionController == null)
			{
				FinishTransition();
				yield break;
			}
			menuAnimationTransitionController.ShowOpeningMenuTransitionAnimation(nextMenu.MenuType);
			yield return WaitForMenuTransition(menuAnimationTransitionController);
			FinishTransition();
		}

		private void FinishTransition()
		{
			EvaluateNextInQueue();
			if (menuTransitionQueue.Count == 0)
			{
				SetupUINavigation();
			}
		}

		private IEnumerator WaitForMenuTransition(IMenuAnimationTransitionController menuAnimationTransitionController)
		{
			while (menuAnimationTransitionController.IsTransitioning)
			{
				yield return null;
			}
		}

		private void AddNewMenu(IUIMenuController newMenu)
		{
			IUIMenuController iUIMenuController = null;
			if (activeUIIndex < 5)
			{
				iUIMenuController = activeUIMenu[activeUIIndex];
			}
			activeUIIndex--;
			activeUIMenu[activeUIIndex] = newMenu;
			if (!newMenu.AllowPhone)
			{
				phoneAllowControlIndex = Mathf.Max(activeUIIndex, phoneAllowControlIndex);
				if (WorldHandler.instance != null)
				{
					WorldHandler.instance.GetCurrentPlayer().phone.AllowPhone(isAllowed: false);
				}
			}
			if (iUIMenuController != null)
			{
				if (iUIMenuController.AlwaysShowOnTop)
				{
					EnsureMenuIsOnTop(iUIMenuController);
				}
				else
				{
					iUIMenuController.IsEnabled = false;
				}
			}
		}

		private IEnumerator CloseMenuASync(MenuTransitionState menuTransitionState)
		{
			yield return UpdateQueue(menuTransitionState);
			yield return TransitionOutOfMenu(menuTransitionState.menuController);
			RemoveMenu(menuTransitionState.menuController);
			DisableMenu(menuTransitionState.menuController);
			FinishTransition();
		}

		private IEnumerator DisableMenuASync(MenuTransitionState menuTransitionState)
		{
			yield return UpdateQueue(menuTransitionState);
			yield return TransitionOutOfMenu(menuTransitionState.menuController);
			ClearButtonSelection();
			DisableMenu(menuTransitionState.menuController);
			FinishTransition();
		}

		private IEnumerator TransitionOutOfMenu(IUIMenuController menu)
		{
			IMenuAnimationTransitionController menuAnimationTransitionController = menu.HideMenu();
			if (menuAnimationTransitionController != null)
			{
				menuAnimationTransitionController.ShowClosingMenuTransitionAnimation(menu.MenuType);
				yield return WaitForMenuTransition(menuAnimationTransitionController);
			}
		}

		private IEnumerator ShowPreviousMenuASync(MenuTransitionState menuTransitionState, IUIMenuController previousMenu)
		{
			yield return UpdateQueue(menuTransitionState);
			yield return TransitionOutOfMenu(menuTransitionState.menuController);
			RemoveMenu(menuTransitionState.menuController);
			DisableMenu(menuTransitionState.menuController);
			EnableMenu(previousMenu);
			yield return TransitionToNextMenu(previousMenu);
		}

		private IEnumerator PushNewMenuDelayed(IUIMenuController newMenu)
		{
			yield return new WaitForEndOfFrame();
			PushNewMenu(newMenu);
		}

		private IEnumerator PopCurrentMenuDelayed(int lastActiveUIIndex)
		{
			yield return new WaitForEndOfFrame();
			if (lastActiveUIIndex == activeUIIndex)
			{
				PopCurrentMenu();
			}
		}

		private void ClearButtonSelection()
		{
			SetSelectedGameObject(null);
		}

		private void ForceFocusOnMenu(IUIMenuController menuController)
		{
			GameObject buttonToSelect = GetButtonToSelect(menuController);
			SetSelectedGameObject(buttonToSelect);
			DetermineMenuMouseState(menuController);
		}

		private void DetermineMenuMouseState(IUIMenuController menuController)
		{
			if (menuController.AllowMouseInput && gameInput.IsPlayerControllerKeyboard())
			{
				gameInput.SetUICursorMode();
			}
			else
			{
				gameInput.SetGameCursorMode();
			}
		}

		private GameObject GetButtonToSelect(IUIMenuController menuController)
		{
			GameObject gameObject = menuController.GetLastSelectedObject();
			if (gameObject == null)
			{
				gameObject = menuController.GetFirstFocusGameObject();
			}
			return gameObject;
		}

		private void ForceHighlightOnGameObjectWithButton(GameObject gameObjectWithButton)
		{
			if (!(gameObjectWithButton == null))
			{
				Button component = gameObjectWithButton.GetComponent<Button>();
				if (!(component == null))
				{
					ForceHighlightOnButton(component);
				}
			}
		}

		private void ForceHighlightOnButton(Button button)
		{
			if (!(button == null))
			{
				button.Select();
				button.OnSelect(null);
			}
		}

		private void EnsureMenuIsOnTop(IUIMenuController menuRootObject)
		{
			if (activeUIIndex != 4)
			{
				IUIMenuController menuRootObject2 = ReplaceMenuAt(menuRootObject, activeUIIndex);
				ReplaceMenuAt(menuRootObject2, activeUIIndex + 1);
			}
		}

		private IUIMenuController ReplaceMenuAt(IUIMenuController menuRootObject, int activeMenuIndex)
		{
			if (activeMenuIndex >= 5)
			{
				return null;
			}
			IUIMenuController result = activeUIMenu[activeMenuIndex];
			activeUIMenu[activeMenuIndex] = menuRootObject;
			return result;
		}
	}
}
