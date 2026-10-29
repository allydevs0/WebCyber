using System;
using System.Collections.Generic;
using Rewired;

namespace Reptile
{
	public class RewiredMappingHandler
	{
		private RewiredControllerHandler rewiredControllerHandler;

		private Rewired.Player firstRewiredPlayer;

		private IRewiredControllerGuidHandler rewiredControllerGuidHandler;

		private APlatform platform;

		public RewiredMappingHandler(RewiredControllerHandler rewiredControllerHandler, Rewired.Player firstRewiredPlayer, IRewiredControllerGuidHandler rewiredControllerGuidHandler, APlatform platform)
		{
			this.rewiredControllerHandler = rewiredControllerHandler;
			this.firstRewiredPlayer = firstRewiredPlayer;
			this.rewiredControllerGuidHandler = rewiredControllerGuidHandler;
			this.platform = platform;
		}

		public void EnableControllerMap(int controllerMapCategoryID, int playerId = 0)
		{
			GetRewiredPlayer(playerId)?.controllers.maps.SetMapsEnabled(state: true, controllerMapCategoryID);
		}

		public void DisableControllerMap(int controllerMapCategoryID, int playerId = 0)
		{
			GetRewiredPlayer(playerId)?.controllers.maps.SetMapsEnabled(state: false, controllerMapCategoryID);
		}

		public void DisableAllControllerInputMaps(int playerId = 0)
		{
			GetRewiredPlayer(playerId)?.controllers.maps.SetAllMapsEnabled(state: false);
		}

		public void ApplyLayoutManagerRuleSet(string ruleSetTag, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.maps.layoutManager.ruleSets.Find((ControllerMapLayoutManager.RuleSet item) => item.tag.Equals(ruleSetTag)).enabled = true;
				rewiredPlayer.controllers.maps.layoutManager.Apply();
			}
		}

		public ActionElementMap GetCurrentControllerActionElementMap(string actionName, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			Controller playerCurrentController = rewiredControllerHandler.GetPlayerCurrentController(playerId);
			if (playerCurrentController == null)
			{
				return GetDefaultActionElementMap(actionName, rewiredPlayer);
			}
			if (playerCurrentController.type == ControllerType.Mouse || playerCurrentController.type == ControllerType.Keyboard)
			{
				return GetPlayerKeyboardMouseActionElementMap(actionName, rewiredPlayer);
			}
			return GetPlayerControllerActionElementMap(actionName, playerCurrentController, rewiredPlayer);
		}

		public ActionElementMap GetCurrentControllerActionElementMap(int actionId, ControllerType controllerType, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(controllerType, playerId);
			if (playerCurrentControllerByType == null)
			{
				return GetDefaultActionElementMap(actionId, controllerType, rewiredPlayer);
			}
			if (playerCurrentControllerByType.type == ControllerType.Mouse || playerCurrentControllerByType.type == ControllerType.Keyboard)
			{
				return GetPlayerKeyboardMouseActionElementMap(actionId, rewiredPlayer);
			}
			return GetPlayerControllerActionElementMap(actionId, playerCurrentControllerByType, rewiredPlayer);
		}

		public int GetCurrentControllerActionElementId(int actionId, ControllerType controllerType, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null || actionId == -1)
			{
				return -1;
			}
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(controllerType, playerId);
			ActionElementMap actionElementMap = null;
			bool flag = false;
			if (playerCurrentControllerByType == null)
			{
				actionElementMap = GetDefaultActionElementMap(actionId, controllerType, rewiredPlayer);
			}
			else if (playerCurrentControllerByType.type == ControllerType.Mouse || playerCurrentControllerByType.type == ControllerType.Keyboard)
			{
				actionElementMap = GetPlayerKeyboardActionElementMap(actionId, rewiredPlayer);
				if (actionElementMap == null)
				{
					actionElementMap = GetPlayerMouseActionElementMap(actionId, rewiredPlayer);
					flag = true;
				}
			}
			else
			{
				actionElementMap = GetPlayerControllerActionElementMap(actionId, playerCurrentControllerByType, rewiredPlayer);
			}
			int num = actionElementMap.elementIdentifierId;
			if (flag)
			{
				num = TextMeshProButtonSpriteHandler.ConvertMouseElementIdToKeyboardSpriteIndex(num);
			}
			return num;
		}

		public int GetCurrentControllerActionElementId(string action, ControllerType controllerType, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return -1;
			}
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(controllerType, playerId);
			ActionElementMap actionElementMap = null;
			actionElementMap = ((playerCurrentControllerByType == null) ? GetDefaultActionElementMap(action, rewiredPlayer) : ((playerCurrentControllerByType.type != ControllerType.Mouse && playerCurrentControllerByType.type != ControllerType.Keyboard) ? GetPlayerControllerActionElementMap(action, playerCurrentControllerByType, rewiredPlayer) : GetPlayerKeyboardMouseActionElementMap(action, rewiredPlayer)));
			return actionElementMap.elementIdentifierId;
		}

		public List<ActionElementMap> GetAllControllersActionElementMaps(int actionId, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			return GetAllPlayerControllersActionElementMaps(actionId, rewiredPlayer);
		}

		public List<ActionElementMap> GetAllControllerActionElementMaps(int actionId, int controllerId, ControllerType controllerType, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			return GetAllPlayerControllerActionElementMaps(actionId, controllerId, controllerType, rewiredPlayer);
		}

		public void RestoreDefaultControllerMapForCurrentController(int controllerMapCategoryId, ControllerType controllerType, int playerId = 0)
		{
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(controllerType);
			if (playerCurrentControllerByType != null)
			{
				int defaultCategoryForControllerType = RewiredUtility.GetDefaultCategoryForControllerType(controllerType);
				SetDefaultControllerMap(controllerMapCategoryId, playerCurrentControllerByType, defaultCategoryForControllerType, playerId);
			}
		}

		public void SetDefaultControllerMap(int controllerMapCategoryId, Controller controller, int controllerLayoutId, int playerId = 0)
		{
			if (controller != null)
			{
				GetRewiredPlayer(playerId)?.controllers.maps.LoadMap(controller.type, controller.id, controllerMapCategoryId, controllerLayoutId);
			}
		}

		public int[] GetAllUniqueEnabledControllerMapCategoryIDs(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return new int[0];
			}
			List<ControllerMap> allUniqueEnabledPlayerControllerMaps = GetAllUniqueEnabledPlayerControllerMaps(rewiredPlayer);
			if (allUniqueEnabledPlayerControllerMaps == null)
			{
				return new int[0];
			}
			return ExtractCategoryIDsFromControllerMaps(allUniqueEnabledPlayerControllerMaps);
		}

		public void SetEnabledInputMapsOnConnectedController(Controller controller)
		{
			if (controller == null)
			{
				return;
			}
			int assignedPlayerIDForController = rewiredControllerHandler.GetAssignedPlayerIDForController(controller);
			if (assignedPlayerIDForController != -1)
			{
				int[] allUniqueEnabledControllerMapCategoryIDs = GetAllUniqueEnabledControllerMapCategoryIDs(assignedPlayerIDForController);
				for (int i = 0; i < allUniqueEnabledControllerMapCategoryIDs.Length; i++)
				{
					EnableControllerMap(allUniqueEnabledControllerMapCategoryIDs[i], assignedPlayerIDForController);
				}
			}
		}

		public void AddSavedControllerMapToPlayer(GameInput.ControllerSaveMap controllerSaveMap)
		{
			if (controllerSaveMap.Equals(GameInput.ControllerSaveMap.INVALID_CONTROLLERMAP))
			{
				return;
			}
			Rewired.Player rewiredPlayer = GetRewiredPlayer(controllerSaveMap.playerId);
			if (rewiredPlayer == null)
			{
				return;
			}
			ControllerMap map = rewiredPlayer.controllers.maps.GetMap(controllerSaveMap.controllerType, controllerSaveMap.controllerId, controllerSaveMap.mapCategoryId, 0);
			if (map != null)
			{
				if (controllerSaveMap.controllerType == ControllerType.Joystick)
				{
					ApplyJoystickSavedControllerMap(controllerSaveMap, map);
				}
				else
				{
					ApplySavedControllerMapToControllerMap(controllerSaveMap, map);
				}
			}
		}

		public void ApplySavedControllerMapToControllerMap(GameInput.ControllerSaveMap controllerSaveMap, ControllerMap controllerMap)
		{
			if (controllerSaveMap.Equals(GameInput.ControllerSaveMap.INVALID_CONTROLLERMAP))
			{
				return;
			}
			List<GameInput.ControllerActionElementSaveMap> controllerActionElementSaveMaps = controllerSaveMap.controllerActionElementSaveMaps;
			if (controllerActionElementSaveMaps.Count == 0)
			{
				return;
			}
			controllerMap.ClearElementMaps();
			foreach (GameInput.ControllerActionElementSaveMap item in controllerActionElementSaveMaps)
			{
				CreateNewActionElementMap(item, controllerMap, controllerSaveMap.controllerType);
			}
		}

		public void DeleteActionElementMap(ControllerType controllerType, int actionElementMapId, int controllerMapCategoryId, int playerId = 0)
		{
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(controllerType);
			if (playerCurrentControllerByType != null)
			{
				rewiredControllerHandler.GetControllerMap(controllerMapCategoryId, playerCurrentControllerByType, playerId)?.DeleteElementMap(actionElementMapId);
			}
		}

		private int[] ExtractCategoryIDsFromControllerMaps(List<ControllerMap> controllerMaps)
		{
			if (controllerMaps == null)
			{
				return new int[0];
			}
			int[] array = new int[controllerMaps.Count];
			for (int i = 0; i < controllerMaps.Count; i++)
			{
				array[i] = controllerMaps[i].categoryId;
			}
			return array;
		}

		private ActionElementMap GetPlayerKeyboardActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (!rewiredPlayer.controllers.hasKeyboard)
			{
				return null;
			}
			Controller keyboard = rewiredPlayer.controllers.Keyboard;
			return GetPlayerControllerActionElementMap(actionName, keyboard, rewiredPlayer);
		}

		private ActionElementMap GetPlayerKeyboardActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (!rewiredPlayer.controllers.hasKeyboard)
			{
				return null;
			}
			Controller keyboard = rewiredPlayer.controllers.Keyboard;
			return GetPlayerControllerActionElementMap(actionId, keyboard, rewiredPlayer);
		}

		private ActionElementMap GetPlayerMouseActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null || !rewiredPlayer.controllers.hasMouse)
			{
				return null;
			}
			Controller mouse = rewiredPlayer.controllers.Mouse;
			return GetPlayerControllerActionElementMap(actionName, mouse, rewiredPlayer);
		}

		private ActionElementMap GetPlayerMouseActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null || !rewiredPlayer.controllers.hasMouse)
			{
				return null;
			}
			Controller mouse = rewiredPlayer.controllers.Mouse;
			return GetPlayerControllerActionElementMap(actionId, mouse, rewiredPlayer);
		}

		private ActionElementMap GetPlayerKeyboardMouseActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			ActionElementMap actionElementMap = GetPlayerKeyboardActionElementMap(actionName, rewiredPlayer);
			if (actionElementMap == null)
			{
				actionElementMap = GetPlayerMouseActionElementMap(actionName, rewiredPlayer);
			}
			return actionElementMap;
		}

		private ActionElementMap GetPlayerKeyboardMouseActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			ActionElementMap actionElementMap = GetPlayerKeyboardActionElementMap(actionId, rewiredPlayer);
			if (actionElementMap == null)
			{
				actionElementMap = GetPlayerMouseActionElementMap(actionId, rewiredPlayer);
			}
			return actionElementMap;
		}

		private ActionElementMap GetPlayerControllerActionElementMap(string actionName, Controller controller, Rewired.Player rewiredPlayer)
		{
			return rewiredPlayer?.controllers.maps.GetFirstElementMapWithAction(controller, actionName, skipDisabledMaps: false);
		}

		private ActionElementMap GetPlayerControllerActionElementMap(int actionId, Controller controller, Rewired.Player rewiredPlayer)
		{
			return rewiredPlayer?.controllers.maps.GetFirstElementMapWithAction(controller, actionId, skipDisabledMaps: false);
		}

		private List<ActionElementMap> GetAllPlayerControllersActionElementMaps(int actionId, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			List<ActionElementMap> list = new List<ActionElementMap>();
			rewiredPlayer.controllers.maps.GetElementMapsWithAction(actionId, skipDisabledMaps: false, list);
			return list;
		}

		private List<ActionElementMap> GetAllPlayerControllerActionElementMaps(int actionId, int controllerId, ControllerType controllerType, Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			List<ActionElementMap> list = new List<ActionElementMap>();
			rewiredPlayer.controllers.maps.GetElementMapsWithAction(controllerType, controllerId, actionId, skipDisabledMaps: false, list);
			return list;
		}

		private JoystickMap GetJoystickInstanceFirstActionElementMap(string actionName, Controller controller, Rewired.Player rewiredPlayer)
		{
			if (controller.type != ControllerType.Joystick)
			{
				return null;
			}
			Guid controllerGuid = rewiredControllerGuidHandler.GetControllerGuid(controller);
			return GetJoystickInstanceFirstActionElementMapByGUID(actionName, controllerGuid, rewiredPlayer);
		}

		private JoystickMap GetJoystickInstanceFirstActionElementMap(int actionId, Controller controller, Rewired.Player rewiredPlayer)
		{
			if (controller.type != ControllerType.Joystick)
			{
				return null;
			}
			Guid controllerGuid = rewiredControllerGuidHandler.GetControllerGuid(controller);
			foreach (InputMapCategory mapCategory in ReInput.mapping.MapCategories)
			{
				JoystickMap joystickMapInstance = ReInput.mapping.GetJoystickMapInstance(controllerGuid, mapCategory.id, 0);
				if (joystickMapInstance.ContainsAction(actionId))
				{
					return joystickMapInstance;
				}
			}
			return null;
		}

		private JoystickMap GetJoystickInstanceFirstActionElementMapByGUID(string actionName, Guid controllerGuid, Rewired.Player rewiredPlayer)
		{
			foreach (InputMapCategory mapCategory in ReInput.mapping.MapCategories)
			{
				JoystickMap joystickMapInstance = ReInput.mapping.GetJoystickMapInstance(controllerGuid, mapCategory.id, 0);
				if (joystickMapInstance.ContainsAction(actionName))
				{
					return joystickMapInstance;
				}
			}
			return null;
		}

		private JoystickMap GetJoystickInstanceFirstActionElementMapByGUID(int actionId, Guid controllerGuid, Rewired.Player rewiredPlayer)
		{
			foreach (InputMapCategory mapCategory in ReInput.mapping.MapCategories)
			{
				JoystickMap joystickMapInstance = ReInput.mapping.GetJoystickMapInstance(controllerGuid, mapCategory.id, 0);
				if (joystickMapInstance.ContainsAction(actionId))
				{
					return joystickMapInstance;
				}
			}
			return null;
		}

		private List<ControllerMap> GetAllUniqueEnabledPlayerControllerMaps(Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			List<ControllerMap> list = new List<ControllerMap>();
			foreach (ControllerMap allMap in rewiredPlayer.controllers.maps.GetAllMaps())
			{
				if (allMap.enabled)
				{
					AddPossibleUniqueEnabledPlayerControllerMap(list, allMap);
				}
			}
			return list;
		}

		private void AddPossibleUniqueEnabledPlayerControllerMap(List<ControllerMap> uniqueEnabledPlayerControllerMaps, ControllerMap candidate)
		{
			if (uniqueEnabledPlayerControllerMaps != null && candidate != null && uniqueEnabledPlayerControllerMaps.FindIndex((ControllerMap enabledControllerMap) => enabledControllerMap.categoryId == candidate.categoryId) < 0)
			{
				uniqueEnabledPlayerControllerMaps.Add(candidate);
			}
		}

		private Rewired.Player GetRewiredPlayer(int playerId = 0)
		{
			if (playerId == 0)
			{
				return firstRewiredPlayer;
			}
			return RewiredUtility.GetRewiredPlayerByPlayerId(playerId);
		}

		private void ApplyJoystickSavedControllerMap(GameInput.ControllerSaveMap controllerSaveMap, ControllerMap controllerMap)
		{
			Guid.TryParse(controllerSaveMap.hardwareTypeGuid, out var result);
			bool flag = result.Equals(Guid.Empty);
			bool flag2 = controllerMap.hardwareGuid.Equals(Guid.Empty);
			if ((!flag || flag2) && !(!flag & flag2))
			{
				ApplySavedControllerMapToControllerMap(controllerSaveMap, controllerMap);
			}
		}

		private void ReplaceExistingActionElementMap(GameInput.ControllerActionElementSaveMap controllerActionElementSaveMap, ControllerMap controllerMap, ControllerType controllerType)
		{
			ActionElementMap firstButtonMapWithAction = controllerMap.GetFirstButtonMapWithAction(controllerActionElementSaveMap.actionId, skipDisabledMaps: true);
			if (firstButtonMapWithAction == null)
			{
				ElementAssignment elementAssignment = RewiredStructUtility.CreateNewBindingElementAssignment(controllerActionElementSaveMap, controllerType);
				controllerMap.ReplaceOrCreateElementMap(elementAssignment);
			}
			else
			{
				ElementAssignment elementAssignment2 = RewiredStructUtility.CreateExistingBindingElementAssignment(controllerActionElementSaveMap, controllerType, firstButtonMapWithAction.id);
				controllerMap.ReplaceElementMap(elementAssignment2);
			}
		}

		private void CreateNewActionElementMap(GameInput.ControllerActionElementSaveMap controllerActionElementSaveMap, ControllerMap controllerMap, ControllerType controllerType)
		{
			ElementAssignment elementAssignment = RewiredStructUtility.CreateNewBindingElementAssignment(controllerActionElementSaveMap, controllerType);
			controllerMap.CreateElementMap(elementAssignment);
		}

		private ActionElementMap GetDefaultActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			if (platform.PlatformDefaultControllerType == ControllerType.Keyboard)
			{
				return GetDefaultKeyboardMouseActionElementMap(actionName, rewiredPlayer);
			}
			return GetDefaultJoystickActionElementMap(actionName, rewiredPlayer);
		}

		private ActionElementMap GetDefaultActionElementMap(int actionId, ControllerType controllerType, Rewired.Player rewiredPlayer)
		{
			if (platform.PlatformDefaultControllerType == ControllerType.Keyboard && (controllerType == ControllerType.Keyboard || controllerType == ControllerType.Mouse))
			{
				return GetDefaultKeyboardMouseActionElementMap(actionId, rewiredPlayer);
			}
			return GetDefaultJoystickActionElementMap(actionId, rewiredPlayer);
		}

		private ActionElementMap GetDefaultActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			if (platform.PlatformDefaultControllerType == ControllerType.Keyboard)
			{
				return GetDefaultKeyboardMouseActionElementMap(actionId, rewiredPlayer);
			}
			return GetDefaultJoystickActionElementMap(actionId, rewiredPlayer);
		}

		private ActionElementMap GetDefaultKeyboardMouseActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			ActionElementMap playerKeyboardActionElementMap = GetPlayerKeyboardActionElementMap(actionName, rewiredPlayer);
			if (playerKeyboardActionElementMap == null)
			{
				return GetPlayerMouseActionElementMap(actionName, rewiredPlayer);
			}
			return playerKeyboardActionElementMap;
		}

		private ActionElementMap GetDefaultKeyboardMouseActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			ActionElementMap playerKeyboardActionElementMap = GetPlayerKeyboardActionElementMap(actionId, rewiredPlayer);
			if (playerKeyboardActionElementMap == null)
			{
				return GetPlayerMouseActionElementMap(actionId, rewiredPlayer);
			}
			return playerKeyboardActionElementMap;
		}

		private ActionElementMap GetDefaultJoystickActionElementMap(string actionName, Rewired.Player rewiredPlayer)
		{
			Guid defaultControllerGuid = rewiredControllerGuidHandler.GetDefaultControllerGuid();
			return GetJoystickInstanceFirstActionElementMapByGUID(actionName, defaultControllerGuid, rewiredPlayer).GetFirstElementMapWithAction(actionName);
		}

		private ActionElementMap GetDefaultJoystickActionElementMap(int actionId, Rewired.Player rewiredPlayer)
		{
			Guid defaultControllerGuid = rewiredControllerGuidHandler.GetDefaultControllerGuid();
			return GetJoystickInstanceFirstActionElementMapByGUID(actionId, defaultControllerGuid, rewiredPlayer).GetFirstElementMapWithAction(actionId);
		}
	}
}
