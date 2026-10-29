using System;
using System.Collections.Generic;
using Rewired;
using Rewired.Config;
using UnityEngine;

namespace Reptile
{
	public class GameInput : IDisposable
	{
		public struct PlayerControllerMapIDCollection
		{
			public static readonly PlayerControllerMapIDCollection INVALID_COLLECTION = new PlayerControllerMapIDCollection(-1, new int[0]);

			public int playerId;

			public int[] controllerMapCategoryIDs;

			public PlayerControllerMapIDCollection(int playerId, int[] controllerMapCategoryIDs)
			{
				this.playerId = playerId;
				this.controllerMapCategoryIDs = controllerMapCategoryIDs;
			}
		}

		public struct ControllerSaveMap
		{
			public static readonly ControllerSaveMap INVALID_CONTROLLERMAP;

			public ControllerType controllerType;

			public int controllerId;

			public int playerId;

			public List<ControllerActionElementSaveMap> controllerActionElementSaveMaps;

			public int mapCategoryId;

			public string hardwareTypeGuid;

			public ControllerSaveMap(ControllerType controllerType, int controllerId, int playerId, List<ControllerActionElementSaveMap> controllerActionElementSaveMaps, int mapCategoryId, string hardwareTypeGuid)
			{
				this.controllerType = controllerType;
				this.controllerId = controllerId;
				this.playerId = playerId;
				this.controllerActionElementSaveMaps = controllerActionElementSaveMaps;
				this.mapCategoryId = mapCategoryId;
				this.hardwareTypeGuid = hardwareTypeGuid;
			}
		}

		public struct ControllerActionElementSaveMap
		{
			public static readonly ControllerActionElementSaveMap INVALID_CONTROLLER_ACTION_SAVE_MAP;

			public int actionId;

			public KeyCode keyCode;

			public int elementIdentifierId;

			public ControllerElementType elementType;

			public Pole axisContribution;

			public ControllerActionElementSaveMap(int actionId, KeyCode keyCode, int elementIdentifierId, ControllerElementType elementType, Pole axisContribution)
			{
				this.actionId = actionId;
				this.keyCode = keyCode;
				this.elementIdentifierId = elementIdentifierId;
				this.elementType = elementType;
				this.axisContribution = axisContribution;
			}
		}

		public delegate void ControllerConnectionDelegate(ControllerType controllerType, int controllerId);

		public const int NO_PLAYER = -1;

		public const int NO_CONTROLLER_ID = -1;

		public const int NO_ELEMENT_ID = -1;

		public const int NO_ACTION_ID = -1;

		public const int NO_CATEGORY_ID = -1;

		private GameObject rewiredInputManagerPrefab;

		private RewiredMappingHandler rewiredMappingHandler;

		private RewiredControllerHandler rewiredControllerHandler;

		private RewiredRemappingHandler rewiredRemappingHandler;

		private IRewiredControllerGuidHandler rewiredControllerGuidHandler;

		private SaveManager saveManager;

		private Rewired.Player firstRewiredPlayer;

		private APlatform platform;

		public RewiredMappingHandler RewiredMappingHandler => rewiredMappingHandler;

		public RewiredControllerHandler RewiredControllerHandler => rewiredControllerHandler;

		public RewiredRemappingHandler RewiredRemappingHandler => rewiredRemappingHandler;

		public Rewired.Player FirstRewiredPlayer => firstRewiredPlayer;

		public bool IsRewiredInitialized => ReInput.isReady;

		public CursorLockMode CurrentCursorLockMode => Cursor.lockState;

		public bool IsCursorVisible => Cursor.visible;

		public static event ControllerConnectionDelegate OnControllerConnected;

		public static event ControllerConnectionDelegate OnControllerDisconnected;

		public static event ControllerConnectionDelegate OnControllerPreDisconnect;

		public GameInput(GameObject rewiredInputManagerPrefab, SaveManager saveManager, InputControllerGuidData inputControllerGuidData, APlatform platform)
		{
			this.rewiredInputManagerPrefab = rewiredInputManagerPrefab;
			this.saveManager = saveManager;
			this.platform = platform;
			InitializeRewiredPrefab();
			InitializeRewiredParameters();
			InitializeFirstRewiredPlayer();
			rewiredControllerGuidHandler = ControllerGuidHandlerFactory.CreateRewiredControllerGuidHandler(inputControllerGuidData, platform);
			rewiredControllerHandler = new RewiredControllerHandler(firstRewiredPlayer, rewiredControllerGuidHandler, platform);
			rewiredMappingHandler = new RewiredMappingHandler(rewiredControllerHandler, firstRewiredPlayer, rewiredControllerGuidHandler, platform);
			rewiredRemappingHandler = new RewiredRemappingHandler(rewiredControllerHandler);
			ReInput.ControllerConnectedEvent += OnRewiredControllerConnected;
			ReInput.ControllerDisconnectedEvent += OnRewiredControllerDisconnected;
			ReInput.ControllerPreDisconnectEvent += OnRewiredControllerPreDisconnect;
			SetupStartInputMaps();
		}

		public void Dispose()
		{
			rewiredControllerHandler.Dispose();
			rewiredControllerHandler = null;
			rewiredMappingHandler = null;
			rewiredRemappingHandler = null;
			firstRewiredPlayer = null;
			saveManager = null;
			rewiredControllerGuidHandler.Dispose();
			rewiredControllerGuidHandler = null;
			DisposeRewired();
		}

		public void ApplyInputSettings(InputSettings inputSettings)
		{
			List<ControllerSaveMap> savedMaps = inputSettings.savedMaps;
			for (int i = 0; i < savedMaps.Count; i++)
			{
				rewiredMappingHandler.AddSavedControllerMapToPlayer(savedMaps[i]);
			}
		}

		public float GetAxis(int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetAxis(actionId) ?? 0f;
		}

		public float GetAxisPreviousFrame(int action, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetAxisPrev(action) ?? 0f;
		}

		public bool GetButtonHeld(int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetButton(actionId) ?? false;
		}

		public bool GetButtonHeldPrevFrame(int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetButtonPrev(actionId) ?? false;
		}

		public bool GetButtonHeldForControllerType(ControllerType controllerType, int actionId, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return false;
			}
			if (rewiredPlayer.IsCurrentInputSource(actionId, controllerType))
			{
				return rewiredPlayer.GetButton(actionId);
			}
			return false;
		}

		public bool GetButtonNew(int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetButtonDown(actionId) ?? false;
		}

		public bool GetButtonNewForControllerType(ControllerType controllerType, int actionId, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return false;
			}
			if (rewiredPlayer.IsCurrentInputSource(actionId, controllerType))
			{
				return rewiredPlayer.GetButtonDown(actionId);
			}
			return false;
		}

		public bool GetButtonNew(string actionName, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetButtonDown(actionName) ?? false;
		}

		public bool GetButtonUp(int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetButtonUp(actionId) ?? false;
		}

		public bool GetAnyButtonNew(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return false;
			}
			if (!rewiredPlayer.GetAnyButtonDown())
			{
				return rewiredPlayer.GetAnyNegativeButtonDown();
			}
			return true;
		}

		public bool IsOnlyButtonNew(int actionId, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return false;
			}
			if (!rewiredPlayer.GetAnyButtonPrev())
			{
				return GetButtonNew(actionId, playerId);
			}
			return false;
		}

		public bool IsInputSourceForAction(ControllerType controllerType, int actionId, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.IsCurrentInputSource(actionId, controllerType) ?? false;
		}

		public bool IsPlayerControllerJoystick(int playerId = 0)
		{
			return GetCurrentControllerType(playerId) == ControllerType.Joystick;
		}

		public bool IsPlayerControllerKeyboard(int playerId = 0)
		{
			ControllerType currentControllerType = GetCurrentControllerType(playerId);
			if (currentControllerType != ControllerType.Keyboard)
			{
				return currentControllerType == ControllerType.Mouse;
			}
			return true;
		}

		public bool IsComboHeld(ControllerType controllerType, int[] actionIds, int playerId = 0)
		{
			for (int i = 0; i < actionIds.Length; i++)
			{
				if (!GetButtonHeldForControllerType(controllerType, actionIds[i], playerId))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsAnyJoystickControllerConnected()
		{
			return rewiredControllerHandler.IsAnyJoystickConnected();
		}

		public void SetCursorVisibility(bool visible)
		{
			Cursor.visible = visible;
		}

		public void SetCursorLockMode(CursorLockMode cursorLockMode)
		{
			Cursor.lockState = cursorLockMode;
		}

		public void SetUICursorMode()
		{
			SetCursorVisibility(visible: true);
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				SetCursorLockMode(CursorLockMode.Confined);
			}
			else
			{
				SetCursorLockMode(CursorLockMode.None);
			}
		}

		public void SetGameCursorMode()
		{
			SetCursorVisibility(visible: false);
			SetCursorLockMode(CursorLockMode.Confined);
		}

		public void EnableMouse(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.Mouse.enabled = true;
			}
		}

		public void DisableMouse(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.Mouse.enabled = false;
			}
		}

		public Vector2 GetMousePosition()
		{
			return ReInput.controllers.Mouse.screenPosition;
		}

		public Vector2 GetMouseDelta()
		{
			return ReInput.controllers.Mouse.screenPositionDelta;
		}

		public void EnableControllerMap(int controllerMapCategoryID, int playerId = 0)
		{
			rewiredMappingHandler.EnableControllerMap(controllerMapCategoryID, playerId);
		}

		public void EnableControllerMaps(int[] controllerMapCategoryIDs, int playerId = 0)
		{
			for (int i = 0; i < controllerMapCategoryIDs.Length; i++)
			{
				rewiredMappingHandler.EnableControllerMap(controllerMapCategoryIDs[i], playerId);
			}
		}

		public void EnablePlayerControllerMapIDCollection(PlayerControllerMapIDCollection playerControllerMapCollection)
		{
			EnableControllerMaps(playerControllerMapCollection.controllerMapCategoryIDs, playerControllerMapCollection.playerId);
		}

		public void DisableControllerMap(int controllerMapCategoryID, int playerId = 0)
		{
			rewiredMappingHandler.DisableControllerMap(controllerMapCategoryID, playerId);
		}

		public void DisableAllControllerMaps(int playerId = 0)
		{
			rewiredMappingHandler.DisableAllControllerInputMaps(playerId);
		}

		public PlayerControllerMapIDCollection GetAllCurrentEnabledControllerMapCategoryIDs(int playerId = 0)
		{
			if (playerId <= -1)
			{
				return PlayerControllerMapIDCollection.INVALID_COLLECTION;
			}
			int[] allUniqueEnabledControllerMapCategoryIDs = rewiredMappingHandler.GetAllUniqueEnabledControllerMapCategoryIDs(playerId);
			return new PlayerControllerMapIDCollection(playerId, allUniqueEnabledControllerMapCategoryIDs);
		}

		public void ListenToLastActiveControllerChanged(RewiredControllerHandler.GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, int playerId = 0)
		{
			rewiredControllerHandler.AddListenerForLastActiveControllerChanged(playerActiveControllerChangedDelegate, playerId);
		}

		public void StopListenToLastActiveControllerChanged(RewiredControllerHandler.GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, int playerId = 0)
		{
			if (rewiredControllerHandler != null)
			{
				rewiredControllerHandler.RemoveListenerForLastActiveControllerChanged(playerActiveControllerChangedDelegate, playerId);
			}
		}

		public ControllerType GetCurrentControllerType(int playerId = 0)
		{
			return rewiredControllerHandler.GetPlayerCurrentController(playerId)?.type ?? platform.PlatformDefaultControllerType;
		}

		public PlayerSaveData GetInputSaveData(bool userAssignableMapsOnly, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.GetSaveData(userAssignableMapsOnly) ?? default(PlayerSaveData);
		}

		public void SetVibrationOnCurrentController(float leftMotor, float rightMotor, float duration, int playerId = 0)
		{
			if (GetRewiredPlayer(playerId) != null && GetCurrentControllerType(playerId) == ControllerType.Joystick)
			{
				float num = 1f;
				SaveManager saveManager = Core.Instance.SaveManager;
				if (saveManager != null && saveManager.Settings != null)
				{
					num = (saveManager.Settings.gameplaySettings.rumble ? 1f : 0f);
				}
				rewiredControllerHandler.SetVibrationOnCurrentJoystick(num * leftMotor, num * rightMotor, duration, playerId);
			}
		}

		public void StopVibrationOnCurrentController(int playerId = 0)
		{
			if (GetRewiredPlayer(playerId) != null && GetCurrentControllerType(playerId) == ControllerType.Joystick)
			{
				rewiredControllerHandler.StopVibrationOnCurrentJoystick(playerId);
			}
		}

		public void RestoreSavedInputMap(ControllerType controllerType, int controllerId, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				ApplySavedControllerMapsToController(rewiredPlayer, controllerType, controllerId);
			}
		}

		public void ApplyLayoutManagerRuleSet(string ruleSetTag, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				rewiredMappingHandler.ApplyLayoutManagerRuleSet(ruleSetTag, rewiredPlayer);
			}
		}

		private void InitializeRewiredPrefab()
		{
			UnityEngine.Object.Instantiate(rewiredInputManagerPrefab, Core.Instance.transform);
		}

		private void InitializeRewiredParameters()
		{
			ReInput.configuration.logLevel = LogLevelFlags.Off;
		}

		private void InitializeFirstRewiredPlayer()
		{
			firstRewiredPlayer = RewiredUtility.GetRewiredPlayerByPlayerId(0);
			if (firstRewiredPlayer == null)
			{
				HandleNoRewiredPlayerFound(0);
			}
		}

		private void OnRewiredControllerConnected(ControllerStatusChangedEventArgs controllerStatusChangedEventArgs)
		{
			ApplySavedControllerMapsToConnectedController(controllerStatusChangedEventArgs);
			rewiredMappingHandler.SetEnabledInputMapsOnConnectedController(controllerStatusChangedEventArgs.controller);
			if (OnControllerConnected != null)
			{
				OnControllerConnected(controllerStatusChangedEventArgs.controllerType, controllerStatusChangedEventArgs.controllerId);
			}
		}

		private void ApplySavedControllerMapsToConnectedController(ControllerStatusChangedEventArgs controllerStatusChangedEventArgs)
		{
			int assignedPlayerIDForController = rewiredControllerHandler.GetAssignedPlayerIDForController(controllerStatusChangedEventArgs.controller);
			if (assignedPlayerIDForController != -1)
			{
				Rewired.Player rewiredPlayer = GetRewiredPlayer(assignedPlayerIDForController);
				ApplySavedControllerMapsToController(rewiredPlayer, controllerStatusChangedEventArgs.controllerType, controllerStatusChangedEventArgs.controllerId);
			}
		}

		private void ApplySavedControllerMapsToController(Rewired.Player rewiredPlayer, ControllerType controllerType, int controllerId)
		{
			if (rewiredPlayer == null)
			{
				return;
			}
			foreach (ControllerMap allMap in rewiredPlayer.controllers.maps.GetAllMaps(controllerType))
			{
				ControllerSaveMap savedControllerMap = saveManager.Settings.inputSettings.GetSavedControllerMap(controllerType, controllerId, rewiredPlayer.id, allMap.categoryId);
				rewiredMappingHandler.AddSavedControllerMapToPlayer(savedControllerMap);
			}
		}

		private void OnRewiredControllerDisconnected(ControllerStatusChangedEventArgs controllerStatusChangedEventArgs)
		{
			if (OnControllerDisconnected != null)
			{
				OnControllerDisconnected(controllerStatusChangedEventArgs.controllerType, controllerStatusChangedEventArgs.controllerId);
			}
		}

		private void OnRewiredControllerPreDisconnect(ControllerStatusChangedEventArgs controllerStatusChangedEventArgs)
		{
			if (OnControllerPreDisconnect != null)
			{
				OnControllerPreDisconnect(controllerStatusChangedEventArgs.controllerType, controllerStatusChangedEventArgs.controllerId);
			}
		}

		private void SetupStartInputMaps()
		{
			DisableAllControllerMaps();
			EnableControllerMap(1);
		}

		private void DisposeRewired()
		{
			ReInput.ControllerConnectedEvent -= OnRewiredControllerConnected;
			ReInput.ControllerDisconnectedEvent -= OnRewiredControllerDisconnected;
			ReInput.ControllerPreDisconnectEvent -= OnRewiredControllerPreDisconnect;
		}

		private void HandleNoRewiredPlayerFound(int playerId)
		{
		}

		private Rewired.Player GetRewiredPlayer(int playerId = 0)
		{
			if (playerId == 0)
			{
				return firstRewiredPlayer;
			}
			return RewiredUtility.GetRewiredPlayerByPlayerId(playerId);
		}
	}
}
