using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace Reptile
{
	public class OptionsMenuInputTab : AOptionsMenuTab
	{
		[SerializeField]
		private GameObject inputFields;

		[SerializeField]
		private GameObject gamepadColumnHeader;

		[SerializeField]
		private GameObject keyboardColumnHeader;

		[SerializeField]
		private GameObject cancelBindingContainer;

		private GameInput gameInput;

		private RewiredControllerHandler rewiredControllerHandler;

		private RewiredRemappingHandler rewiredRemappingHandler;

		private RewiredMappingHandler rewiredMappingHandler;

		private InputSelectionRowController[] inputSelectionRowControllers;

		private InputSelectionRowController listeningFor;

		private Controller currentController;

		private RewiredInputUIActionBinder inputUIActionBinder;

		private bool hasConflictedKeyboardMouseBindings;

		private bool hasConflictedGamepadBindings;

		private bool canAdjustKeyboardMouseBindings = true;

		private bool canAdjustGamepadBindings = true;

		private int rewiredPlayerId;

		private int listeningForCategoryId = -1;

		public void StartAttemptMappingAction(InputSelectionRowController inputSelectionRowController)
		{
			if (currentController.type != ControllerType.Custom && (!inputSelectionRowController.IsMovementAxis || currentController.type != ControllerType.Joystick))
			{
				listeningForCategoryId = inputSelectionRowController.GetControllerMapCategoryId(currentController.type);
				if (listeningForCategoryId == -1)
				{
					listeningForCategoryId = -1;
					return;
				}
				SetupListeningControllerMaps();
				listeningFor = inputSelectionRowController;
				DisableInputSelectionRowsButtons();
				SetupControllerForMappingAttempt(listeningFor, currentController);
				ShowCancelBindingContainer();
				StartCoroutine(DelayListening());
			}
		}

		public override void Activate()
		{
			base.Activate();
			gameInput.ListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
			GameInput.OnControllerConnected += OnControllerConnected;
			GameInput.OnControllerDisconnected += OnControllerDisconnected;
			GameInput.OnControllerPreDisconnect += OnControllerPreDisconnect;
			DetermineControllerAdjustments();
			SetupControllerColumns();
			HideCancelBindingContainer();
			HideGamepadMovementAxisBindings();
			rewiredPlayerId = gameInput.FirstRewiredPlayer.id;
			currentController = rewiredControllerHandler.GetPlayerCurrentController();
		}

		public override void Deactivate()
		{
			if (isActive)
			{
				OnDeactivateInputTab();
			}
			base.Deactivate();
		}

		public void ResetToDefault()
		{
			if (canAdjustGamepadBindings)
			{
				ResetControllerTypeToDefault(ControllerType.Joystick);
				ResetGamepadColumn();
			}
			if (canAdjustKeyboardMouseBindings)
			{
				ResetControllerTypeToDefault(ControllerType.Keyboard);
				ResetControllerTypeToDefault(ControllerType.Mouse);
				ResetKeyboardMouseColumn();
			}
			hasConflictedGamepadBindings = false;
			hasConflictedKeyboardMouseBindings = false;
			saveManager.SaveSettings();
		}

		protected override void Awake()
		{
			base.Awake();
			gameInput = Core.Instance.GameInput;
			inputSelectionRowControllers = inputFields.GetComponentsInChildren<InputSelectionRowController>();
			rewiredControllerHandler = gameInput.RewiredControllerHandler;
			rewiredRemappingHandler = gameInput.RewiredRemappingHandler;
			rewiredMappingHandler = gameInput.RewiredMappingHandler;
			inputUIActionBinder = new RewiredInputUIActionBinder(rewiredMappingHandler, rewiredRemappingHandler, rewiredControllerHandler);
		}

		protected void Update()
		{
			if (inputUIActionBinder.IsMappingAction)
			{
				UpdateListeningForInput();
			}
		}

		private void SetupListeningControllerMaps()
		{
			gameInput.EnableControllerMap(5);
			gameInput.DisableControllerMap(1);
		}

		private void DisableInputSelectionRowsButtons()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].DisableRaycastTarget();
			}
		}

		private void SetupControllerForMappingAttempt(InputSelectionRowController inputSelectionRowController, Controller controller)
		{
			if (controller.type == ControllerType.Joystick)
			{
				inputSelectionRowController.ShowAttemptMappingGamepadActionBinding();
			}
			else if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
			{
				inputSelectionRowController.ShowAttemptMappingKeyboardMouseActionBinding();
			}
		}

		private void ShowCancelBindingContainer()
		{
			cancelBindingContainer.SetActive(value: true);
		}

		private IEnumerator DelayListening()
		{
			yield return null;
			int[] allCurrentBindableCategories = GetAllCurrentBindableCategories();
			inputUIActionBinder.StartAttemptMappingAction(rewiredPlayerId, allCurrentBindableCategories);
		}

		private void OnLastActiveControllerChanged(int playerId, ControllerType controllerType)
		{
			Controller lastActiveController = currentController;
			currentController = rewiredControllerHandler.GetPlayerCurrentController();
			if (controllerType == ControllerType.Joystick)
			{
				OnHandleLastActiveControllerChangedToGamepad(lastActiveController);
			}
		}

		private void OnHandleLastActiveControllerChangedToGamepad(Controller lastActiveController)
		{
			if (hasConflictedGamepadBindings && lastActiveController.type == ControllerType.Joystick)
			{
				HandleHasConflictedBindings(ControllerType.Joystick, lastActiveController.id, rewiredPlayerId);
			}
			if (canAdjustGamepadBindings)
			{
				RefreshGamepadButtonGlyphs();
			}
		}

		private void RefreshGamepadButtonGlyphs()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].RefreshGamepadActionElementMap();
				inputSelectionRowControllers[i].RefreshGamepadButtonGlyph();
			}
		}

		private void OnControllerConnected(ControllerType controllerType, int controllerId)
		{
			DetermineControllerAdjustments();
			if (controllerType == ControllerType.Joystick)
			{
				OnGamepadConnected();
			}
		}

		private void DetermineControllerAdjustments()
		{
			canAdjustKeyboardMouseBindings = true;
			canAdjustGamepadBindings = gameInput.IsAnyJoystickControllerConnected();
		}

		private void OnGamepadConnected()
		{
			if (canAdjustGamepadBindings)
			{
				ShowGamepadColumn();
			}
		}

		private void ShowGamepadColumn()
		{
			gamepadColumnHeader.SetActive(canAdjustGamepadBindings && canAdjustKeyboardMouseBindings);
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].ShowGamepadBinding();
				inputSelectionRowControllers[i].RefreshGamepadActionElementMap();
				inputSelectionRowControllers[i].RefreshGamepadButtonGlyph();
			}
		}

		private void ShowKeyboardMouseColumn()
		{
			keyboardColumnHeader.SetActive(value: true);
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].ShowKeyboardMouseBinding();
				inputSelectionRowControllers[i].RefreshKeyboardMouseActionElementMap();
				inputSelectionRowControllers[i].RefreshKeyboardMouseBinding();
			}
		}

		private void OnControllerDisconnected(ControllerType controllerType, int controllerId)
		{
			DetermineControllerAdjustments();
			currentController = rewiredControllerHandler.GetPlayerCurrentController();
			if (canAdjustGamepadBindings)
			{
				RefreshGamepadButtonGlyphs();
			}
			else
			{
				DisableGamepadColumn();
			}
		}

		private void HandleHasConflictedBindings(ControllerType controllerType, int controllerId, int playerId)
		{
			gameInput.RestoreSavedInputMap(controllerType, controllerId, playerId);
			ResetInputSelectionRowsConflictStates(controllerType);
		}

		private void OnControllerPreDisconnect(ControllerType controllerType, int controllerId)
		{
			if (controllerType == ControllerType.Joystick)
			{
				HandleGamepadPredisconnect(controllerId);
			}
		}

		private void HandleGamepadPredisconnect(int controllerId)
		{
			if (hasConflictedGamepadBindings)
			{
				HandleHasConflictedBindings(ControllerType.Joystick, controllerId, rewiredPlayerId);
			}
			if (inputUIActionBinder.IsMappingAction && currentController.type == ControllerType.Joystick)
			{
				CancelInputMapping();
			}
		}

		private void ResetInputSelectionRowsConflictStates(ControllerType controllerType)
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				SetConflictState(controllerType, inputSelectionRowControllers[i], hasConflict: false);
			}
			switch (controllerType)
			{
			case ControllerType.Joystick:
				hasConflictedGamepadBindings = false;
				break;
			case ControllerType.Keyboard:
			case ControllerType.Mouse:
				hasConflictedKeyboardMouseBindings = false;
				break;
			}
		}

		private bool DetermineAxisConflict(List<RewiredRemappingHandler.ConflictInfo> conflicts, int actionId, Pole axisContribution)
		{
			return conflicts.FindIndex(0, (RewiredRemappingHandler.ConflictInfo conflictInfo) => conflictInfo.actionId == actionId && conflictInfo.axisContribution == axisContribution) >= 0;
		}

		private bool DetermineConflict(List<RewiredRemappingHandler.ConflictInfo> conflicts, int actionId)
		{
			return conflicts.FindIndex(0, (RewiredRemappingHandler.ConflictInfo conflictInfo) => conflictInfo.actionId == actionId) >= 0;
		}

		private void SetConflictState(ControllerType controllerType, InputSelectionRowController inputSelectionRowController, bool hasConflict)
		{
			switch (controllerType)
			{
			case ControllerType.Joystick:
				inputSelectionRowController.SetGamepadConflictedState(hasConflict);
				break;
			case ControllerType.Keyboard:
			case ControllerType.Mouse:
				inputSelectionRowController.SetKeyboardMouseConflictedState(hasConflict);
				break;
			}
		}

		private void HandleStopAttemptMappingAction()
		{
			HandleConflicts(currentController);
			StopShowingMappingAttemptOnController(listeningFor, currentController);
			HideCancelBindingContainer();
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].RefreshGamepadButtonGlyph();
				inputSelectionRowControllers[i].RefreshKeyboardMouseBinding();
				inputSelectionRowControllers[i].EnableRaycastTarget();
			}
			listeningFor = null;
			listeningForCategoryId = -1;
		}

		private void HandleConflicts(Controller controller)
		{
			if (inputUIActionBinder.HasConflict)
			{
				HandleConflictingInputBindings(controller);
			}
			else
			{
				ResetInputSelectionRowsConflictStates(controller.type);
			}
		}

		private void HandleConflictingInputBindings(Controller controller)
		{
			List<RewiredRemappingHandler.ConflictInfo> conflictingActionIds = inputUIActionBinder.GetConflictingActionIds();
			if (conflictingActionIds != null)
			{
				SetInputSelectionRowsConflictStates(controller.type, conflictingActionIds);
			}
		}

		private void SetInputSelectionRowsConflictStates(ControllerType controllerType, List<RewiredRemappingHandler.ConflictInfo> conflicts)
		{
			bool flag = false;
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				bool flag2 = false;
				flag2 = ((!inputSelectionRowControllers[i].IsMovementAxis) ? DetermineConflict(conflicts, inputSelectionRowControllers[i].InputActionId) : DetermineAxisConflict(conflicts, inputSelectionRowControllers[i].InputActionId, inputSelectionRowControllers[i].AxisContribution));
				SetConflictState(controllerType, inputSelectionRowControllers[i], flag2);
				if (flag2)
				{
					flag = true;
				}
			}
			switch (controllerType)
			{
			case ControllerType.Joystick:
				hasConflictedGamepadBindings = flag;
				break;
			case ControllerType.Keyboard:
			case ControllerType.Mouse:
				hasConflictedKeyboardMouseBindings = flag;
				break;
			}
		}

		private void DisableGamepadColumn()
		{
			gamepadColumnHeader.SetActive(value: false);
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].HideGamepadBinding();
			}
		}

		private void SetupControllerColumns()
		{
			if (canAdjustKeyboardMouseBindings)
			{
				ShowKeyboardMouseColumn();
			}
			else
			{
				DisableKeyboardColumn();
			}
			if (canAdjustGamepadBindings)
			{
				ShowGamepadColumn();
			}
			else
			{
				DisableGamepadColumn();
			}
		}

		private void DisableKeyboardColumn()
		{
			keyboardColumnHeader.SetActive(value: false);
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].HideKeyboardMouseBinding();
			}
		}

		private void HideCancelBindingContainer()
		{
			cancelBindingContainer.SetActive(value: false);
		}

		private void HideGamepadMovementAxisBindings()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				if (inputSelectionRowControllers[i].IsMovementAxis)
				{
					inputSelectionRowControllers[i].HideGamepadBinding();
				}
			}
		}

		private void HandleConflictedStates(int inputmapCategoryId, int playerId)
		{
			if (hasConflictedGamepadBindings)
			{
				HandleHasConflictedBindings(ControllerType.Joystick, currentController.id, playerId);
			}
			if (hasConflictedKeyboardMouseBindings)
			{
				HandleHasConflictedBindings(ControllerType.Keyboard, currentController.id, playerId);
				HandleHasConflictedBindings(ControllerType.Mouse, currentController.id, playerId);
			}
		}

		private void StopShowingMappingAttemptOnController(InputSelectionRowController inputSelectionRowController, Controller controller)
		{
			if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
			{
				inputSelectionRowController.StopShowingAttemptMappingKeyboardMouseActionBinding();
			}
			else if (controller.type == ControllerType.Joystick)
			{
				inputSelectionRowController.StopShowingAttemptMappingGamepadActionBinding();
			}
		}

		private void OnDeactivateInputTab()
		{
			if (inputUIActionBinder.IsMappingAction)
			{
				inputUIActionBinder.StopAttemptMappingAction();
				HandleStopAttemptMappingAction();
			}
			gameInput.StopListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
			GameInput.OnControllerConnected -= OnControllerConnected;
			GameInput.OnControllerDisconnected -= OnControllerDisconnected;
			GameInput.OnControllerPreDisconnect -= OnControllerPreDisconnect;
			HandleConflictedStates(0, rewiredPlayerId);
			hasConflictedGamepadBindings = false;
			hasConflictedKeyboardMouseBindings = false;
			currentController = null;
			ClearColumns();
			HideCancelBindingContainer();
		}

		private void ClearColumns()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].RemoveGamepadBinding();
				inputSelectionRowControllers[i].RemoveKeyboardBinding();
				inputSelectionRowControllers[i].RemoveMouseBinding();
			}
		}

		private void ResetGamepadColumn()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].SetGamepadConflictedState(isConflicted: false);
				inputSelectionRowControllers[i].RefreshGamepadActionElementMap();
				inputSelectionRowControllers[i].RefreshGamepadButtonGlyph();
			}
		}

		private void ResetKeyboardMouseColumn()
		{
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				inputSelectionRowControllers[i].SetKeyboardMouseConflictedState(isConflicted: false);
				inputSelectionRowControllers[i].RefreshKeyboardMouseActionElementMap();
				inputSelectionRowControllers[i].RefreshKeyboardMouseBinding();
			}
		}

		private void UpdateListeningForInput()
		{
			if (gameInput.GetButtonNew(55))
			{
				CancelInputMapping();
				if (canAdjustGamepadBindings)
				{
					RefreshGamepadButtonGlyphs();
				}
			}
			else
			{
				UpdateAttemptMappingAction(currentController, listeningFor, listeningForCategoryId);
				if (!inputUIActionBinder.IsMappingAction)
				{
					StopAttemptMappingAction();
				}
			}
		}

		private void CancelInputMapping()
		{
			inputUIActionBinder.StopAttemptMappingAction();
			inputUIActionBinder.DetectConflictingMappings(currentController);
			StopAttemptMappingAction();
		}

		private void UpdateAttemptMappingAction(Controller controller, InputSelectionRowController inputSelectionRowController, int controllerMapCategoryId)
		{
			if (controller.type == ControllerType.Joystick)
			{
				inputUIActionBinder.UpdateAttemptMappingSingleController(controller, inputSelectionRowController, controllerMapCategoryId);
			}
			else if (controller.type == ControllerType.Keyboard)
			{
				inputUIActionBinder.UpdateAttemptMappingDoubleController(controller, inputSelectionRowController, ControllerType.Mouse, controllerMapCategoryId);
			}
			else if (controller.type == ControllerType.Mouse)
			{
				inputUIActionBinder.UpdateAttemptMappingDoubleController(controller, inputSelectionRowController, ControllerType.Keyboard, controllerMapCategoryId);
			}
		}

		private void StopAttemptMappingAction()
		{
			HandleStopAttemptMappingAction();
			StartCoroutine(DelayStopListening());
		}

		private IEnumerator DelayStopListening()
		{
			yield return null;
			SetupMenuControllerMaps();
		}

		private void SetupMenuControllerMaps()
		{
			gameInput.EnableControllerMap(1);
			gameInput.DisableControllerMap(5);
		}

		private void ResetControllerTypeToDefault(ControllerType controllerType)
		{
			int[] allCurrentBindableCategories = GetAllCurrentBindableCategories();
			for (int i = 0; i < allCurrentBindableCategories.Length; i++)
			{
				rewiredMappingHandler.RestoreDefaultControllerMapForCurrentController(allCurrentBindableCategories[i], controllerType);
			}
		}

		private int[] GetAllCurrentBindableCategories()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < inputSelectionRowControllers.Length; i++)
			{
				int bindableControllerMapCategoryId = inputSelectionRowControllers[i].GetBindableControllerMapCategoryId();
				if (bindableControllerMapCategoryId != -1 && !list.Contains(bindableControllerMapCategoryId))
				{
					list.Add(bindableControllerMapCategoryId);
				}
			}
			return list.ToArray();
		}
	}
}
