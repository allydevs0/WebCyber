using System.Collections.Generic;
using Rewired;

namespace Reptile
{
	public class RewiredRemappingHandler
	{
		public struct ConflictInfo
		{
			public int actionId;

			public Pole axisContribution;

			public ConflictInfo(int actionId, Pole axisContribution)
			{
				this.actionId = actionId;
				this.axisContribution = axisContribution;
			}
		}

		private RewiredControllerHandler rewiredControllerHandler;

		private ControllerPollingInfo lastPolledControllerInfo;

		public ControllerPollingInfo LastPolledControllerInfo => lastPolledControllerInfo;

		public RewiredRemappingHandler(RewiredControllerHandler rewiredControllerHandler)
		{
			this.rewiredControllerHandler = rewiredControllerHandler;
		}

		public bool AttemptElementReassignment(Controller controller, ControllerPollingInfo info, int actionId, int controllerMapCategoryId, Pole axisContribution, ActionElementMap actionElementMapToUpdate, out ActionElementMap updatedActionElementMap)
		{
			updatedActionElementMap = null;
			if (controller == null || actionElementMapToUpdate == null)
			{
				return false;
			}
			rewiredControllerHandler.GetPlayerControllerHelper();
			ControllerMap controllerMap = rewiredControllerHandler.GetControllerMap(controllerMapCategoryId, controller);
			if (controllerMap == null)
			{
				return false;
			}
			ElementAssignment elementAssignment = RewiredStructUtility.CreateElementReassignmentFromPollingInfo(controller.type, info, actionId, actionElementMapToUpdate.id, axisContribution);
			return controllerMap.ReplaceOrCreateElementMap(elementAssignment, out updatedActionElementMap);
		}

		public bool AttemptNewElementAssignment(Controller controller, ControllerPollingInfo info, int actionId, int controllerMapCategoryId, Pole axisContribution, out ActionElementMap newActionElementMap)
		{
			newActionElementMap = null;
			if (controller == null)
			{
				return false;
			}
			rewiredControllerHandler.GetPlayerControllerHelper();
			ControllerMap controllerMap = rewiredControllerHandler.GetControllerMap(controllerMapCategoryId, controller);
			if (controllerMap == null)
			{
				return false;
			}
			ElementAssignment elementAssignment = RewiredStructUtility.CreateNewElementAssignmentFromPollingInfo(controller.type, info, actionId, axisContribution);
			return controllerMap.ReplaceOrCreateElementMap(elementAssignment, out newActionElementMap);
		}

		public bool DetectConflictInMapping(Controller controller, ActionElementMap actionElementMap, int playerId = 0)
		{
			Rewired.Player.ControllerHelper playerControllerHelper = rewiredControllerHandler.GetPlayerControllerHelper();
			ElementAssignmentConflictCheck conflictCheck = RewiredStructUtility.CreateElementAssignmentConflictCheck(controller, actionElementMap, playerId);
			return playerControllerHelper.conflictChecking.DoesElementAssignmentConflict(conflictCheck, skipDisabledMaps: false);
		}

		public void RetrieveConflictingActionIds(Controller controller, int[] controllerMapCategories, List<ConflictInfo> actionIds, int playerId = 0)
		{
			if (controller != null)
			{
				actionIds.Clear();
				if (controller.type == ControllerType.Joystick)
				{
					RetrieveConflictingActionIDs(controller, controllerMapCategories, actionIds, playerId);
				}
				else
				{
					RetrieveConflictingActionIDsKeyboardMouse(controller, controllerMapCategories, actionIds, playerId);
				}
			}
		}

		public bool DetectPollForButton(Controller controller, out ControllerPollingInfo info)
		{
			info = default;
			if (controller == null)
			{
				return false;
			}
			Rewired.Player.ControllerHelper playerControllerHelper = rewiredControllerHandler.GetPlayerControllerHelper();
			if (controller.type == ControllerType.Mouse)
			{
				info = playerControllerHelper.polling.PollControllerForFirstButtonDown(controller.type, controller.id);
			}
			else if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Joystick)
			{
				info = playerControllerHelper.polling.PollControllerForFirstElementDown(controller.type, controller.id);
			}
			if (info.success)
			{
				return HandleSuccessFullPollingForButton(info, controller);
			}
			return false;
		}

		private bool HandleSuccessFullPollingForButton(ControllerPollingInfo info, Controller pollingForController)
		{
			if (pollingForController == null)
			{
				return false;
			}
			if (info.controllerId != pollingForController.id)
			{
				return false;
			}
			if (info.controllerType == ControllerType.Joystick && pollingForController.type == ControllerType.Joystick)
			{
				return true;
			}
			if ((info.controllerType == ControllerType.Keyboard || info.controllerType == ControllerType.Mouse) && (pollingForController.type == ControllerType.Keyboard || pollingForController.type == ControllerType.Mouse))
			{
				return true;
			}
			return false;
		}

		private void RetrieveConflictingActionIDs(Controller controller, int[] controllerMapCategories, List<ConflictInfo> actionIds, int playerId)
		{
			Rewired.Player rewiredPlayerByPlayerId = RewiredUtility.GetRewiredPlayerByPlayerId(playerId);
			for (int i = 0; i < controllerMapCategories.Length; i++)
			{
				ControllerMap controllerMap = rewiredControllerHandler.GetControllerMap(controllerMapCategories[i], controller, playerId);
				RetrieveConflictingActionIDsForControllerMap(actionIds, controller.type, controller.id, rewiredPlayerByPlayerId, controllerMap);
			}
		}

		private void RetrieveConflictingActionIDsKeyboardMouse(Controller controller, int[] controllerMapCategories, List<ConflictInfo> actionIds, int playerId)
		{
			Rewired.Player rewiredPlayerByPlayerId = RewiredUtility.GetRewiredPlayerByPlayerId(playerId);
			for (int i = 0; i < controllerMapCategories.Length; i++)
			{
				ControllerMap controllerMapByType = rewiredControllerHandler.GetControllerMapByType(controllerMapCategories[i], controller, ControllerType.Keyboard, playerId);
				ControllerMap controllerMapByType2 = rewiredControllerHandler.GetControllerMapByType(controllerMapCategories[i], controller, ControllerType.Mouse, playerId);
				RetrieveConflictingActionIDsForControllerMap(actionIds, ControllerType.Keyboard, controller.id, rewiredPlayerByPlayerId, controllerMapByType);
				RetrieveConflictingActionIDsForControllerMap(actionIds, ControllerType.Mouse, controller.id, rewiredPlayerByPlayerId, controllerMapByType2);
			}
		}

		private void RetrieveConflictingActionIDsForControllerMap(List<ConflictInfo> conflicts, ControllerType controllerType, int controllerId, Rewired.Player rewiredPlayer, ControllerMap controllerMap)
		{
			foreach (ElementAssignmentConflictInfo item in rewiredPlayer.controllers.conflictChecking.ElementAssignmentConflicts(controllerType, controllerId, controllerMap))
			{
				conflicts.Add(new ConflictInfo(item.actionId, item.elementMap.axisContribution));
			}
		}
	}
}
